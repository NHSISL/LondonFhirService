# Telemetry and Metrics

How a request is identified, what is measured, and where each thing is written.

Scanned against the source on 2026-09-17. If you change the correlation flow, the
metric span tree or either sink, update this file with it.

## At a glance

One request produces up to four kinds of record, in three places:

| Record | Written by | Where it lands | Retained |
|---|---|---|---|
| Request telemetry | Application Insights SDK collectors | Application Insights | Per the App Insights workspace |
| Metric spans (replayed) | `MetricTelemetryPublisher` → `TrackDependency` | Application Insights, as dependencies | Per the App Insights workspace (default 90 days, configurable) |
| Metric spans (authoritative) | `MetricService` → `AuditAndMetricStorageBroker` | `Metrics` table | `RetentionPeriodInDays`, when purging is enabled — see **Retention** |
| Audit entries | `AuditService` → `AuditAndMetricStorageBroker` | `Audits` table | Never purged — there is no audit sweep |

Everything above is tied together by one value: the **correlation id**.

---

## 1. Correlating a request

### The correlation id is a W3C trace id

The correlation id is not invented by this service. It is the **W3C Trace Context
trace id** of the request, taken from `Activity.Current`.

ASP.NET parses the inbound `traceparent` header and hangs the trace id off the
request's `Activity` before any application code runs. Application Insights
correlates on the same value. `CorrelationBroker` reads it from there, so the
audit trail, the metrics table and the telemetry all name the same trace rather
than inventing a second identifier for one request.

It is carried as a `Guid` because `IMetric.CorrelationId` and the indexed column
behind it already are. The two are the same 128 bits and the same 32 hex
characters:

```
correlationId.ToString("N")  ==  activity.TraceId.ToHexString()
```

So an id read off a response header or an `Audits` row can be pasted straight
into the telemetry viewer. Conversion goes through the hex string, never the raw
bytes — `Guid` stores its first three fields little-endian and a trace id does
not, so a byte-level conversion silently scrambles the value.

### Resolution order

`CorrelationBroker.GetCorrelationIdAsync()` resolves once per request and caches
the result on `HttpContext.Items`:

1. **Already on `HttpContext.Items`** — return it. Every later reader in the
   request sees the same value.
2. **`Activity.Current.TraceId`**, when there is a W3C activity with a non-zero
   trace id. This covers both the caller who sent `traceparent` and the caller
   who sent nothing, because ASP.NET generates a trace id either way.
3. **A fresh `Guid` from `IIdentifierBroker`** — no `HttpContext` (a background
   worker), no activity (nothing listening for activities), or a non-W3C
   hierarchical activity.

`Guid.Empty` is never used. It would fail the coordination service's argument
validation and would stamp every row written under it with the same meaningless
value.

The request's own **span id** is captured in the same step, from the same
activity, and cached alongside. See [section 3](#3-what-application-insights-gets).

### Header behaviour

| Header | Direction | Behaviour |
|---|---|---|
| `traceparent` | **Inbound** | Parsed by ASP.NET. Its trace id becomes the correlation id. This is the only header that influences the id. |
| `X-Correlation-Id` | **Outbound only** | Written on every response. It is the id actually in force — an echo of the caller's trace when they sent one, an assignment when they did not. |
| `X-Request-Id` | — | **Not used.** Neither read nor written. |

> **Note.** `X-Correlation-Id` is **not read inbound**. A caller who sends one
> will get a different value back, because the response header always names the
> trace the work was actually filed under. If honouring it as a fallback is
> wanted — for a caller that sends `X-Correlation-Id` but no `traceparent` —
> that is a deliberate change to `CorrelationBroker`, not current behaviour.
> Be aware it lets a caller choose the key the audit and metric rows are filed
> under, including colliding ids across unrelated requests.

### Why the id is settled in the pipeline

`CorrelationMiddleware` runs **first**, ahead of authentication, authorization
and the request-timeout policy, and registers an `OnStarting` callback that
writes `X-Correlation-Id`.

That placement is the point. A request rejected with a 401, refused with a 403,
abandoned on timeout, or failed on a malformed body never reaches a controller —
and those are exactly the responses a consumer rings up about. The id used to be
drawn inside the coordination service, so those responses carried nothing to
quote. The header is deferred to `OnStarting` rather than written inline so it
lands on whatever response is eventually produced, including one written by a
handler further down the pipeline.

`PatientController` reads the same id and passes it to
`Stu3PatientCoordinationService`, which no longer mints one of its own. It still
draws its **span** ids from `IIdentifierBroker`.

### End to end

```
caller ──traceparent: 00-<trace-id>-<span-id>-01──▶ CorrelationMiddleware
                                                     │ correlation id = <trace-id>
                                                     ▼
                                            PatientController
                                                     │ correlationId
                                                     ▼
                                     Stu3PatientCoordinationService
                                                     │
                            ┌────────────────────────┴───────────────────────┐
                            ▼                                                ▼
                   Audits.CorrelationId                          Metrics.CorrelationId
                            │                                                │
                            └──────────────── same value ────────────────────┘
                                                     │
caller ◀──X-Correlation-Id: <trace-id>───────────────┘
```

---

## 2. What is measured

A **metric span** is one measured piece of work. Spans sharing a
`CorrelationId` form one request; `ParentId` links them into a tree whose root is
the span with no parent.

### The span tree for `$getstructuredrecord`

```
Request                          the coordination service, end to end
├── AccessCheck                  the consumer access permission check
├── ProviderRequests             everything involved in getting provider data
│   ├── ProviderDiscovery        resolving the active providers to fan out to
│   └── ProviderFanOut           the parallel barrier waiting on every provider
│       ├── Provider             one provider task (in parallel)
│       │   └── Persist          deferred write of the retrieved payload
│       └── Provider
│           └── Persist
└── Consolidation                reconciling the bundles into one response
```

Two figures fall out of that shape without any span kind being special-cased:

```
sibling wait  =  ProviderFanOut − Provider            per provider: time spent idle
                                                      waiting for the slowest sibling
API overhead  =  Request − (AccessCheck + ProviderRequests + Consolidation)
```

`Persist` is the one span whose duration is **not** part of its ancestors'. The
write is dispatched to a background queue, so it starts around the time its
`Provider` parent finishes and costs the request nothing.

`Request` measures the coordination service, not wire-to-wire. Controller and
middleware time sits above it and is not measured.

`Orchestration`, `Foundation` and `ProviderCall` are **no longer recorded**. Each
wrapped a single child and differed from it only by that layer's own overhead, so
they added depth to every tree without adding a derivable figure. The enum
members are kept because the column is persisted as text: removing one would fail
to parse historic rows still inside the retention window.

### Status

Durations are only comparable **within one status**. A span that failed fast or
timed out at its ceiling distorts any average or percentile that includes it.

| Status | Meaning |
|---|---|
| `Succeeded` | Completed normally. |
| `Failed` | A genuine fault. |
| `TimedOut` | Hit a configured ceiling. |
| `Cancelled` | The client went away. |
| `Skipped` | Not attempted. |

The root `Request` span classifies a failure by walking the inner exception
chain, because by the time an abort reaches the coordination layer it has already
been localised into a dependency exception. Without that walk, every localised
timeout and cancellation would record as `Failed` and a 130-second timeout would
be averaged against a fast validation failure.

---

## 3. What Application Insights gets

Three streams, from three different mechanisms.

### a. Request telemetry — automatic

`AddApplicationInsightsTelemetry()` registers the classic SDK collectors, which
track incoming HTTP requests as `RequestTelemetry` and outgoing HTTP/SQL calls as
dependencies. Nothing in this repository writes these; they carry the operation
id taken from the request `Activity`, which is the same trace id used as the
correlation id.

`EnableAdaptiveSampling` is **`false`**, so nothing is sampled away.

### b. Metric spans — replayed as dependencies

The metric library publishes each completed span to an `ActivitySource`
(`LondonFhirService.Metrics`) as a second sink alongside the database.

Nothing in the classic SDK subscribes to arbitrary activity sources, so every
published span used to be dropped before it reached Application Insights.
`MetricTelemetryPublisher`, a hosted service registered by both hosts, registers
the `ActivityListener` that makes them real and forwards each one with
`TrackDependency`.

An `ActivityListener` rather than the OpenTelemetry Azure Monitor distro on
purpose: the distro would run a second pipeline alongside the existing SDK and
double-report requests and dependencies.

Each span arrives as a `DependencyTelemetry`:

| Field | Source |
|---|---|
| `Name` | `{Method}/{Name}` |
| `Type` | the `metric.type` tag, e.g. `Provider` |
| `Target` | `Metric.Target` |
| `Duration` | the measured duration, not the replay duration |
| `Timestamp` | `Metric.Started` |
| `Success` | false when the span status is an error |
| `ResultCode` | `Metric.ErrorCode` |
| `Id` | the replayed activity's span id |

Plus every span field as a custom property: `metric.id`, `metric.parentId`,
`metric.correlationId`, `metric.method`, `metric.type`, `metric.name`,
`metric.target`, `metric.durationMs`, `metric.status`, `metric.errorCode`,
`metric.payloadBytes`, `metric.consumer`.

Sampling is forced to `AllDataAndRecorded`: the span has already happened and
been persisted by the time it is replayed, so sampling it away would leave the
metrics table and the telemetry disagreeing about what ran.

**Shape in the transaction view.** Replayed spans are deliberately **flat** —
every span of a request is given the same parent rather than the tree it actually
forms. Reproducing the real nesting made the metric view too noisy to read; the
telemetry copy is a scannable overview, and the exact tree stays in the
`metric.id` / `metric.parentId` properties and in the metrics table. **Do not
turn this into a faithful hierarchy without agreeing the UI change that goes with
it.**

That shared parent is the **HTTP request's own span id**, so the flat group hangs
*under* the incoming request rather than floating beside it. Flattening and
anchoring are independent. The span id reaches the replay through
`IRequestTraceBroker`, a port the host satisfies from `CorrelationBroker`:
`MetricService` reads it while the request is still alive and carries it into the
deferred write, because the replay itself runs on a background worker with no
request left to ask. Without one — a background worker, or a host that registers
no implementation and gets the library's null object — it falls back to a parent
derived from the correlation id, which groups correctly but places the spans at
the top of the trace.

### c. Traces — from `ILogger`

`LoggingBroker` is an `ILogger` passthrough, and the Application Insights logger
provider ships everything at `Information` and above (`Logging:ApplicationInsights:LogLevel:Default`).

### What Application Insights does *not* get

**Audit entries.** Audits are written to the database only. They are the
information-governance record of who read which patient's data, and they are
retained and controlled for that purpose rather than shipped to a telemetry
store.

### Host differences

`MetricTelemetryPublisher` lives in `LondonFhirService.Core/Workers` and is
registered by **both hosts**, so metric spans from the API and from Manage both
reach Application Insights. It sits in Core rather than in either host because
both need it, and rather than in `LondonFhirService.Clients.AuditAndMetrics`
because that library deliberately carries no telemetry vendor — it publishes to
an `ActivitySource` and leaves the choice of listener to whoever hosts it.

Two differences remain. The API host registers the three services that actually
record metric spans (`Stu3PatientCoordinationService`,
`Stu3PatientOrchestrationService`, `Stu3PatientService`); Manage registers none
of them, so in practice almost nothing is published from there today — the
listener is registered so that changes if Manage ever grows such a path, not
because it is busy now. Manage also registers no `ICorrelationBroker`, so its
spans group by trace but are not anchored under a request.

And the API host registers the bounded
`AuditAndMetricsDispatcher`, Manage does not. Manage therefore falls back to the
library's `ThreadPoolDispatcher` — deferred writes still happen, but one work
item per write, unbounded, with nothing draining them on shutdown.

---

## 4. What the database gets

### `Metrics`

The authoritative store, and the only place the true span tree exists.

| Column | Notes |
|---|---|
| `Id` | The span's own id. |
| `ParentId` | The enclosing span, `null` for the root. |
| `CorrelationId` | The W3C trace id as a `Guid`. Ties every span of one request together. |
| `UserId` | Opaque account id, or null for background work. Stamped by `MetricService`. |
| `Consumer` | The calling consumer's display name, where one resolved. |
| `Method` | The operation, matching the audit type string — e.g. `STU3-Patient-GetStructuredRecordSerialised`. The FHIR version is part of it, so STU3 and R4 timings never merge. |
| `Type` | `MetricType`, persisted **as text**. |
| `Name` | What was measured, e.g. a provider friendly name. |
| `Target` | The stable identifier behind `Name`, e.g. a provider's fully qualified name. Survives a rename. |
| `Started` | Wall-clock start, from one monotonic timestamp per request, so siblings are comparable and never appear to start before their parent. |
| `Completed` | Always `Started + DurationMs`, by construction — never a second clock read. |
| `DurationMs` | Measured with `Stopwatch`, held as a `double` because the fastest spans are sub-millisecond. |
| `Status` | `MetricStatus`. |
| `ErrorCode` | A short classification, never an exception message. |
| `PayloadBytes` | Provider durations are not comparable without it — a provider returning a large bundle slowly is not necessarily the slower provider. |
| `Description` | Free text for a dashboard reader. |
| `CreatedDate` | When the row was written, deliberately separate from `Started`. |

`Type` is text rather than an ordinal because this table is queried ad hoc for
reporting, where `WHERE Type = 'Provider'` is readable and an ordinal is not, and
where an enum reorder would silently rewrite the meaning of historic rows.

Indexed on `CorrelationId`, `ParentId`, `CreatedDate`, `Completed`, and the
composites `(Method, Type, Started)`, `(Name, Started)`, `(Consumer, Started)`.

> **This table must never carry patient identifiable data** — no NHS number, date
> of birth or any other patient identifier. The correlation id is the join key
> back to the audit trail when that detail is needed. Keeping the table free of
> PII is what allows it to be retained, aggregated and reported on independently
> of the audit retention rules. That applies to `Description` too.

### `Audits`

`Id`, `CorrelationId` (string), `AuditType`, `Title`, `Message`, `FileName`,
`LogLevel`, `CreatedBy`, `CreatedDate`, `UpdatedBy`, `UpdatedDate`.

Indexed on `CorrelationId`, `LogLevel`, `CreatedDate`, and the composites
`(AuditType, CreatedDate)` and `(Title, CreatedDate)`.

Most audit writes are deferred like metrics. The exception is the **access
decision** — every allow and every denial — which is awaited and surfaces
failures, because losing one to a process restart is not acceptable.

### How writes are deferred

Recording must not lengthen the work being recorded, so writes go through
`IAuditAndMetricsDispatcher`: a bounded queue owned by the host, drained by
`AuditAndMetricsDispatchWorker`.

- Values that depend on the request — `CreatedDate`, `UserId`, `Consumer`, the
  request span id — are **stamped before the deferral**, while the request is
  still alive. Only the write itself is deferred.
- Queue rejection is a **return value, not an exception**: the caller is
  recording telemetry, and a full queue must not take down the request it is
  trying to measure.
- `StopAsync` completes the queue and waits for in-flight writes, so a shutdown
  does not silently drop entries that were already accepted.
- Without a host-supplied dispatcher the library falls back to
  `ThreadPoolDispatcher` — one work item per write, unbounded, draining nothing.

Current settings: `Capacity` 10000, `DrainConcurrency` 4, `ShutdownGraceSeconds` 5.

### Retention

`MetricPurgeWorker` runs the metric retention sweep (`SweepIntervalHours` 24,
`InitialDelayMinutes` 5). Whether anything is deleted is decided by
`AuditAndMetricsConfigurations`:

```jsonc
"AuditAndMetricsConfigurations": {
  "IsPurgingAllowed": false,      // repo default — see the note below
  "RetentionPeriodInDays": 90,
  "PurgeBatchSize": 5000
}
```

> **The values in `appsettings.json` are development defaults, not the deployed
> configuration.** `Program.cs` adds environment variables last, so they win over
> both JSON files. Deployed environments enable purging that way
> (`AuditAndMetricsConfigurations__IsPurgingAllowed`), and the operative retention
> is whatever that environment sets — read the App Service configuration, not this
> file, to know what a given environment is doing.

Two things that are true regardless of environment:

- **There is no audit purge at all.** No sweep exists for the `Audits` table, and
  no setting turns one on. Audit rows accumulate indefinitely by design — they
  are the information-governance record.
- A zero or negative retention period is **rejected rather than obeyed**. It
  would put the cut-off at the present or the future and delete the entire table.

Sizing note: the `Metrics` table takes a row **per span**, not per request. A
single `$getstructuredrecord` against two providers writes eight or more rows.

---

## 5. Tracing one request

1. Take the `X-Correlation-Id` from the response (or the caller's `traceparent`
   trace id — they are the same value).
2. **Application Insights**: search that value as the operation id. The request
   telemetry and the replayed metric spans are both under it, the spans nested
   beneath the request.
3. **`Metrics` table**: `WHERE CorrelationId = '<the id>'` gives the true tree —
   order by `Started`, follow `ParentId`.
4. **`Audits` table**: `WHERE CorrelationId = '<the id>'` gives the narrative,
   including the access decision and the reason codes behind it.

```sql
-- the span tree for one request, root first
SELECT Type, Name, Target, Status, DurationMs, PayloadBytes, ParentId, Id
FROM   Metrics
WHERE  CorrelationId = '00000000-0000-0000-0000-000000000000'
ORDER  BY Started;
```

Note the id is a `Guid` in `Metrics` and a string in `Audits`, and that the
telemetry viewer wants the 32-character form without dashes
(`Guid.ToString("N")`).

---

## 6. Configuration reference

Values below are the **repo defaults** from `LondonFhirService.Api/appsettings.json`.
`Program.cs` layers configuration as `appsettings.json` → `appsettings.Development.json`
→ environment variables, so a deployed environment overrides any of them with
`Section__Key` (for example `AuditAndMetricsConfigurations__IsPurgingAllowed`).
Treat this column as "what you get with nothing set", not as what production runs.

| Setting | Repo default | Effect |
|---|---|---|
| `AuditAndMetricsConfigurations:IsEnabled` | `true` | Master switch. Off, `MetricService` returns without writing or publishing. |
| `AuditAndMetricsConfigurations:IsPurgingAllowed` | `false` | Whether the retention sweep deletes anything. |
| `AuditAndMetricsConfigurations:RetentionPeriodInDays` | `90` | Age beyond which metric rows are eligible for purge. |
| `AuditAndMetricsConfigurations:PurgeBatchSize` | `5000` | Rows deleted per batch. |
| `AuditAndMetricsConfigurations:ActivitySourceName` | `LondonFhirService.Metrics` | The source the publisher subscribes to. Bound once and shared, so the two cannot drift. |
| `AuditAndMetricsDispatcherSettings:Capacity` | `10000` | Bounded queue depth. |
| `AuditAndMetricsDispatcherSettings:DrainConcurrency` | `4` | Parallel drain workers. |
| `AuditAndMetricsDispatcherSettings:ShutdownGraceSeconds` | `5` | How long shutdown waits for in-flight writes. |
| `MetricPurgeWorkerSettings:SweepIntervalHours` | `24` | How often the sweep is attempted. |
| `MetricPurgeWorkerSettings:InitialDelayMinutes` | `5` | Delay before the first sweep after start. |
| `ApplicationInsights:EnableAdaptiveSampling` | `false` | Sampling off, so telemetry and the metrics table agree. |
| `Logging:ApplicationInsights:LogLevel:Default` | `Information` | Floor for `ILogger` traces reaching App Insights. |

---

## See also

- [Dependency graph](./DependencyGraph/README.md) — the components named here and
  how they wire together.
