# Telemetry and Metrics

How a request is identified, what is measured, and where each thing is written.

Scanned against the source on 2026-09-30. If you change the correlation flow, the
metric span tree or either sink, update this file with it.

Audits and metrics are recorded by the NuGet package
`NHSOneLondon.AuditAndMetrics.Clients` (version 0.1.1, referenced from
`LondonFhirService.Core/LondonFhirService.Core.csproj`). This service no longer
carries its own copy of that library. The package is the source of truth for how
recording, deferred writes, purging and span replay work, and for what each
setting means. Its [README](https://github.com/NHSISL/NHSOneLondon.AuditAndMetrics)
documents it. This file covers how this service uses the package.

## At a glance

One request produces up to four kinds of record, in three places:

| Record | Written by | Where it lands | Retained |
|---|---|---|---|
| Request telemetry | Application Insights SDK collectors | Application Insights | Per the App Insights workspace |
| Metric spans (replayed) | `MetricTelemetryPublisher` → `TrackDependency` | Application Insights, as dependencies | Per the App Insights workspace (default 90 days, configurable) |
| Metric spans (authoritative) | `MetricService` → `AuditAndMetricBroker` → the package → `AuditAndMetricStorageBroker` | `Metrics` table | `MetricsRetentionPeriodInDays`, when `IsMetricsPurgingAllowed` is on — see **Retention** |
| Audit entries | `AuditService` → `AuditAndMetricBroker` → the package → `AuditAndMetricStorageBroker` | `Audits` table | `AuditRetentionPeriodInDays`, when `IsAuditPurgingAllowed` is on — see **Retention** |

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

`CorrelationBroker.GetCorrelationIdAsync()` resolves once and caches the result.
In a request it is cached on `HttpContext.Items`. With no `HttpContext` (a
background worker) it is cached in a field of the broker, which is registered
scoped, so it lasts as long as that unit of work:

1. **Already resolved** — return it. Every later reader sees the same value.
2. **`Activity.Current.TraceId`**, when there is a W3C activity with a non-zero
   trace id. This covers both the caller who sent `traceparent` and the caller
   who sent nothing, because ASP.NET generates a trace id either way. It is
   also tried when there is no `HttpContext`.
3. **A fresh `Guid` from `IIdentifierBroker`** — no activity (nothing listening
   for activities), or a non-W3C hierarchical activity.

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

The package's `MetricBroker` publishes each completed span to an
`ActivitySource` as a second sink alongside the database. This service names the
source `LondonFhirService.Metrics` through `ActivitySourceName`; the package's
own default is `NHSOneLondon.AuditAndMetrics`.

The library does not know this service's span types. It receives `IMetric.Type`
as text and tags it unchanged, and takes the activity's kind from
`IMetric.SpanKind` — `Server`, `Client` or `Internal`. `Metric` derives that from
its own `MetricType`: `Request` is `Server`; `AccessCheck`, `Provider`,
`ProviderCall` and `Persist` are `Client`; everything else is `Internal`.

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
| `Success` | false for any status other than `Succeeded` |
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
`IRequestTraceBroker`, a package port that this service's `CorrelationBroker`
implements itself, alongside `ICorrelationBroker` — one scoped instance behind
both interfaces, rather than a second broker forwarding to it. The package's
`MetricService` reads it while the request is still alive and stamps it on
`IMetric.RequestSpanId` before the write is deferred, because the replay itself
runs on a background worker with no request left to ask. Without one — a
background worker, or a host that passes no `IRequestTraceBroker` to
`AuditAndMetricsClient` and gets the package's `UnknownRequestTraceBroker` — it
falls back to a parent derived from the correlation id, which groups correctly
but places the spans at the top of the trace.

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
both need it, and rather than in the `NHSOneLondon.AuditAndMetrics.Clients`
package because that library deliberately carries no telemetry vendor — it publishes to
an `ActivitySource` and leaves the choice of listener to whoever hosts it.

Three differences remain. The API host registers the three services that actually
record metric spans (`Stu3PatientCoordinationService`,
`Stu3PatientOrchestrationService`, `Stu3PatientService`); Manage registers none
of them, so in practice almost nothing is published from there today — the
listener is registered so that changes if Manage ever grows such a path, not
because it is busy now. Manage also registers no `ICorrelationBroker` and passes
no `IRequestTraceBroker` to `AuditAndMetricsClient`, so its spans group by trace
but are not anchored under a request.

The API host registers the bounded
`AuditAndMetricsDispatcher`, Manage does not. Manage therefore falls back to the
library's `ThreadPoolDispatcher` — deferred writes still happen, but one work
item per write, unbounded, with nothing draining them on shutdown.

Only the API host registers `AuditAndMetricPurgeWorker`, so the retention sweeps
run there and nowhere else. See **Retention**.

---

## 4. What the database gets

### `Metrics`

The authoritative store, and the only place the true span tree exists.

| Column | Notes |
|---|---|
| `Id` | The span's own id. |
| `ParentId` | The enclosing span, `null` for the root. |
| `CorrelationId` | The W3C trace id as a `Guid`. Ties every span of one request together. |
| `UserId` | Opaque account id, or empty for background work. Stamped by the package's `MetricService`, from this service's `AuditUserBroker`. |
| `Consumer` | The calling consumer's display name, or its user id (oid) when it has none — an application calling the API has no display name. Stamped by the package's `MetricService`. |
| `Method` | The operation, matching the audit type string — e.g. `STU3-Patient-GetStructuredRecordSerialised`. The FHIR version is part of it, so STU3 and R4 timings never merge. |
| `Type` | `MetricType`, persisted **as text**. |
| `Name` | What was measured, e.g. a provider friendly name. |
| `Target` | The stable identifier behind `Name`, e.g. a provider's fully qualified name. Survives a rename. |
| `Started` | Wall-clock start (UTC), read by the span itself as it begins. See **How span times are taken** below. |
| `Completed` | Always `Started + DurationMs`, by construction — never a second clock read. |
| `DurationMs` | Measured with the span's own `Stopwatch`, held as a `double` because the fastest spans are sub-millisecond. |
| `Status` | `MetricStatus`. |
| `ErrorCode` | A short classification, never an exception message. |
| `PayloadBytes` | Provider durations are not comparable without it — a provider returning a large bundle slowly is not necessarily the slower provider. |
| `Description` | Free text for a dashboard reader. |
| `CreatedDate` | When the row was written, deliberately separate from `Started`. |

`Type` is text rather than an ordinal because this table is queried ad hoc for
reporting, where `WHERE Type = 'Provider'` is readable and an ordinal is not, and
where an enum reorder would silently rewrite the meaning of historic rows.

**How span times are taken.** There is no shared timestamp per request. Every
span reads the wall clock once, through `IDateTimeBroker`
(`DateTimeOffset.UtcNow`), as it starts, and starts a `Stopwatch` of its own.
`DurationMs` is that stopwatch's elapsed time and `Completed` is
`Started + DurationMs`. A span's duration is therefore exact and monotonic, but
its place on the timeline is only as good as the wall clock was at the moment
that span read it. In practice:

- A parent reads the clock before any of its children start, so a child's
  `Started` is normally at or after its parent's, and sequential siblings —
  `AccessCheck`, `ProviderRequests`, `Consolidation` — normally sort in the
  order they ran. The gap between two such readings is real elapsed time, spent
  on the work in between.
- Parallel siblings, the `Provider` spans under one `ProviderFanOut`, each read
  the clock as their own task starts. The differences between their `Started`
  values reflect when each task actually got going, which is thread scheduling
  as much as anything the provider did.
- None of that ordering is guaranteed. The wall clock is not monotonic: if it is
  stepped or slewed between two readings (an NTP correction, say), a child can
  appear to start before its parent, or to complete after it. Durations are
  unaffected, because none is ever the difference of two clock readings.

So build the tree from `ParentId` rather than by sorting on `Started`, compare
spans by `DurationMs`, and treat the gaps between spans' timestamps as
approximate.

Indexed on `CorrelationId`, `ParentId`, `CreatedDate`, `Completed`, and the
composites `(Method, Type, Started)`, `(Name, Started)`, `(Consumer, Started)`.

> **This table must never carry patient identifiable data** — no NHS number, date
> of birth or any other patient identifier. The correlation id is the join key
> back to the audit trail when that detail is needed. Keeping the table free of
> PII is what allows it to be retained, aggregated and reported on independently
> of the audit retention rules. That applies to `Description` too.

### Exporting requests to CSV

The management portal's metrics page (`/admin/metrics`) pages its request list
50 at a time as you scroll. **Export to CSV** instead downloads every request
matching the page's filter — correlation id, user id and date range — in one
file, whether or not those rows have been scrolled into view.

It is served by `GET api/metrics/exports?correlationId=&userId=&fromDate=&toDate=`
on the Manage host (every parameter optional, dates inclusive and matched against
`CreatedDate`). `MetricProcessingService` left-joins each root `Request` span to
its `ProviderRequests` span in one query, and `MetricOrchestrationService` writes
the rows through `NHSISL.CsvHelperClient`. One row per request, newest first:

| Column | Notes |
|---|---|
| `StartedUtc` | `yyyy-MM-dd HH:mm:ss.fff`, UTC. |
| `CorrelationId`, `Method`, `Name`, `Status`, `ErrorCode` | From the `Request` span. |
| `DurationMs` | The whole request. |
| `ProviderRequestsMs` | Empty when the request never reached its providers — a failed access check, say. |
| `ProxyOverheadMs` | `DurationMs − ProviderRequestsMs`, clamped at zero and rounded to four places; empty when provider requests are. The same figure as the portal's Proxy overhead column. |
| `Consumer`, `UserId` | As stamped on the span. |

### `Audits`

`Id`, `CorrelationId` (string), `AuditType`, `Title`, `Message`, `FileName`,
`LogLevel`, `CreatedBy`, `CreatedDate`, `UpdatedBy`, `UpdatedDate`.

`FileName` belongs to this service's `Audit` entity only. The
`IAudit` contract (from the `NHSOneLondon.AuditAndMetrics.Abstractions` package)
does not carry it, so entries the library builds (`LogInformationAsync`,
`RecordAuditAsync`) leave it empty; it is set only when a caller hands in a whole `Audit`, as the Manage
audits API does.

Indexed on `CorrelationId`, `LogLevel`, `CreatedDate`, and the composites
`(AuditType, CreatedDate)` and `(Title, CreatedDate)`.

Most audit writes are deferred like metrics. The exception is the **access
decision** — every allow and every denial — which is awaited and surfaces
failures, because losing one to a process restart is not acceptable.

### How writes are deferred

Recording must not lengthen the work being recorded, so writes go through
`IAuditAndMetricsDispatcher`, a port the package defines and the host fills. On
the API host that is `AuditAndMetricsDispatcher`, a bounded queue drained by
`AuditAndMetricsDispatchWorker` (both in `LondonFhirService.Api/Dispatchers`).

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

`AuditAndMetricPurgeWorker` (`LondonFhirService.Api/Workers`) runs two retention
sweeps on one timer: the audit sweep first, then the metric sweep. Its settings
are in `AuditAndMetricPurgeWorkerSettings` (`SweepIntervalHours` 24,
`InitialDelayMinutes` 5). Only the API host registers it.

Each sweep runs in its own scope and its own `try` block. If one fails, the error
is logged and the other still runs; the next sweep picks up whatever was missed.
The worker calls this service's `AuditService` and `MetricService`, which pass
through `AuditAndMetricBroker` to the package.

The worker only decides **when** to sweep. Whether anything is deleted is decided
by the package, from `AuditAndMetricsConfigurations`. Audits and metrics each have
their own switch and retention period; `PurgeBatchSize` is shared:

```jsonc
"AuditAndMetricsConfigurations": {
  "IsAuditPurgingAllowed": false,      // repo default — see the note below
  "AuditRetentionPeriodInDays": 90,
  "IsMetricsPurgingAllowed": false,    // repo default — see the note below
  "MetricsRetentionPeriodInDays": 90,
  "PurgeBatchSize": 5000
}
```

> **The values in `appsettings.json` are development defaults, not the deployed
> configuration.** `Program.cs` adds environment variables last, so they win over
> both JSON files. A deployed environment turns purging on that way, for example
> `AuditAndMetricsConfigurations__IsMetricsPurgingAllowed` and
> `AuditAndMetricsConfigurations__IsAuditPurgingAllowed`, and the operative
> retention is whatever that environment sets — read the App Service
> configuration, not this file, to know what a given environment is doing.
>
> The old single keys `IsEnabled`, `IsPurgingAllowed` and `RetentionPeriodInDays`
> are **no longer read**. An environment variable that still uses one of them
> does nothing, so check the App Service settings use the new names.

Things that are true regardless of environment:

- **Audits can be purged, but only when `IsAuditPurgingAllowed` is on.** The
  repo default is off, so with nothing set audit rows are kept. Audits are the
  information-governance record, so audit purging is switched separately from
  metric purging: an environment can age out metrics and still keep every audit.
- A zero or negative retention period, or a zero or negative `PurgeBatchSize`,
  is **rejected rather than obeyed**. A bad retention period would put the
  cut-off at the present or the future and delete the entire table. The sweep
  fails, the worker logs the error, and nothing is deleted.
- Rows are deleted in batches of `PurgeBatchSize`, in the database, until a
  batch comes back smaller than that. The first sweep against a table that has
  never been purged does not take one long lock.
- If a key is missing from every configuration source, the **package default**
  applies, and it is not the same as this repo's: both purges allowed, 30 days'
  retention, source name `NHSOneLondon.AuditAndMetrics`. See the package
  [README](https://github.com/NHSISL/NHSOneLondon.AuditAndMetrics).

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

Note the id is a `Guid` in `Metrics` and a string in `Audits`. The patient
services write the `Audits` string in the 32-character form without dashes
(`Guid.ToString("N")`), which is also the form the telemetry viewer wants.

---

## 6. Configuration reference

Values below are the **repo defaults** from `LondonFhirService.Api/appsettings.json`.
`Program.cs` layers configuration as `appsettings.json` → `appsettings.Development.json`
→ environment variables, so a deployed environment overrides any of them with
`Section__Key` (for example `AuditAndMetricsConfigurations__IsMetricsPurgingAllowed`).
Treat this column as "what you get with nothing set", not as what production runs.

The `AuditAndMetricsConfigurations` section is owned by the package. It is bound
once by `AuditAndMetricsClient.BindConfigurations` and registered as a singleton
in each host. `LondonFhirService.Manage/appsettings.json` carries the same values.
The package [README](https://github.com/NHSISL/NHSOneLondon.AuditAndMetrics)
describes each key and gives the package's own defaults, which differ from the
repo defaults below.

| Setting | Repo default | Effect |
|---|---|---|
| `AuditAndMetricsConfigurations:IsAuditEnabled` | `true` | Audit switch. Off, audit writes are skipped. Metrics are not affected. |
| `AuditAndMetricsConfigurations:IsAuditPurgingAllowed` | `false` | Whether the audit sweep deletes anything. |
| `AuditAndMetricsConfigurations:AuditRetentionPeriodInDays` | `90` | Age beyond which audit rows are eligible for purge. |
| `AuditAndMetricsConfigurations:IsMetricsEnabled` | `true` | Metric switch. Off, the package's `MetricService` returns without writing or publishing. Audits are not affected. |
| `AuditAndMetricsConfigurations:IsMetricsPurgingAllowed` | `false` | Whether the metric sweep deletes anything. |
| `AuditAndMetricsConfigurations:MetricsRetentionPeriodInDays` | `90` | Age beyond which metric rows are eligible for purge. |
| `AuditAndMetricsConfigurations:PurgeBatchSize` | `5000` | Rows deleted per batch. Shared by both sweeps. |
| `AuditAndMetricsConfigurations:ActivitySourceName` | `LondonFhirService.Metrics` | The source the publisher subscribes to. Bound once and shared, so the two cannot drift. |
| `AuditAndMetricsDispatcherSettings:Capacity` | `10000` | Bounded queue depth. |
| `AuditAndMetricsDispatcherSettings:DrainConcurrency` | `4` | Parallel drain workers. |
| `AuditAndMetricsDispatcherSettings:ShutdownGraceSeconds` | `5` | How long shutdown waits for in-flight writes. |
| `AuditAndMetricPurgeWorkerSettings:SweepIntervalHours` | `24` | How often both sweeps are attempted. |
| `AuditAndMetricPurgeWorkerSettings:InitialDelayMinutes` | `5` | Delay before the first sweep after start. |
| `ApplicationInsights:EnableAdaptiveSampling` | `false` | Sampling off, so telemetry and the metrics table agree. |
| `Logging:ApplicationInsights:LogLevel:Default` | `Information` | Floor for `ILogger` traces reaching App Insights. |

---

## See also

- [Dependency graph](./DependencyGraph/README.md) — the components named here and
  how they wire together.
- [NHSOneLondon.AuditAndMetrics](https://github.com/NHSISL/NHSOneLondon.AuditAndMetrics)
  — the package README: how the library records, defers, purges and replays, and
  every setting it reads.
