# Solution Dependency Graph

An interactive dependency graph of the London FHIR Service solution: project
boundaries, per-component method blocks, and colour-coded data flows.

Data and renderer are separate files, all in this folder:

- [graph.yml](./graph.yml) — the manifest: solution name, project list (which
  also names each project's data file), root order, and the event registry.
- `projects/*.yml` — one file per project / package boundary, each declaring
  that project's components with their methods and outbound flows.
- [index.html](./index.html) — the renderer. It fetches the manifest and the
  project files, assembles them, and draws. No build step, but because the
  data is fetched the page must be **served** rather than double-clicked:

```bash
python -m http.server 8731 --bind 127.0.0.1
```

then open `http://127.0.0.1:8731/` — or use the published GitHub Pages copy.

It carries two ways of drawing the same data, switched from the segmented
control in the header:

- **single copy** *(default)* — every component appears exactly once with its
  full method surface, and all consumers' flows converge on it (the one
  StorageBroker shows all 21 per-entity method rows). Best for "who touches
  this?".
- **per consumer** — dependencies are duplicated once per consumer, each copy
  showing only the method rows that consumer uses. Best for "what does this
  one call path actually do?".

The choice lands in the URL (`#single` / `#duplicated`), so a link keeps the
view you were on, and switching carries your current selection across.

## Reading the graph

- **Left → right layering**: SPA → host exposers → coordinations →
  orchestrations → processings → foundations → brokers → clients → client
  libraries → external services.
- **Dashed boxes** are project / package boundaries. External packages show
  only the public surface that this solution calls.
- **Edge colours**:
  - **blue** — direct method call
  - **green** — event publish, **purple** — event subscribe,
    **red** — a publish/subscribe pair in a circular event flow. None appear
    today: this solution has no event bus. The machinery is kept in the
    renderer so an EventBroker can be modelled later without touching
    `index.html`.
- **Duplication over line-spaghetti** (the *per consumer* view only): a
  dependency is drawn once per consumer, showing only the method rows that
  consumer uses, instead of many lines converging on one shared node. The
  exception is components marked "shared" in the side panel — client-library
  / external exposers, plus the 21 resource matchers (see "modelling
  decisions" below). In the *single copy* view nothing is duplicated, so the
  `shared` flag makes no difference there.
- **Click a method row** to trace that single method's path — the full
  upstream + downstream slice lights up and everything else dims.
- **Click a component header** for the same slice seeded from *every* row of
  that copy at once: the component's whole fan-out, not just its first hop.
  Other copies of the same component stay half-lit so you can find them.
- Whatever is selected is outlined and lettered in **amber**; rows the traced
  path passes through carry a faint blue tint. Click the background or Reset
  to clear. Search finds components and methods. The **utility brokers**
  toggle reveals the DateTime / Identifier / Logging broker copies that are
  hidden by default for readability.

At the last scan, 95 declared components and 447 declared edges draw as
**92 components · 415 flows** in the single-copy view and **263 nodes ·
1008 flows** per consumer (308 · 1086 with utility brokers on).

> When re-verifying locally, serve on a **fresh port**. The page fetches the data
> files, and a browser that has already loaded them on that port will keep
> serving the cached copies — the counts then look unchanged however much the
> YAML was edited.

`.github/workflows/pages.yml` publishes this folder to GitHub Pages on every
push to `main` that touches it — `index.html` is the site root. Nothing is
compiled; `index.html`, `graph.yml` and `projects/` are copied as-is. Pages
has to be enabled once in the repository's Settings → Pages (source: GitHub
Actions).

## Current truths captured in the data (scanned 2026-09-17)

- **`LondonFhirService.Core` has no event bus.** Every flow is a direct call.
  The comparison half of the solution is driven by polling, not messaging:
  `ComparisonWorker` (a `BackgroundService` in the API host) is the *only*
  entry point into `ComparisonCoordinationService`.
- **Auditing and metrics moved out of Core into their own library.**
  `LondonFhirService.Clients.AuditAndMetrics` owns the validation, stamping and
  telemetry; Core reaches it through the single `AuditAndMetricBroker`, and the
  library reaches back down through ports declared in
  `LondonFhirService.Core.Abstractions`. That is what makes the broker legal:
  recording is service work, every layer needs it, and a broker may not call a
  service — but it *may* wrap an external dependency. `AuditBroker` and Core's
  old `AuditClient` are gone.
- **The arrows into `AuditAndMetricStorageBroker` run right-to-left.** It
  implements `IAuditAndMetricStorageBroker`, so the library calls back down
  into the application that hosts it. `IStorageBroker` inherits that port,
  which is what lets the standalone library share Core's `StorageBroker`
  without ever being handed a concrete type.
- **`AuditAndMetricStorageBroker` is the only component that takes
  `IStorageBrokerFactory` for its writes.** Every write opens and disposes its
  own `StorageBroker`, because a deferred write outlives the request scope that
  owns the injected one; only the reads use the scoped broker, because they
  hand back an `IQueryable` the caller enumerates. `Stu3PatientService` uses
  the factory for the same reason.
- **Deferred writes are handed back to the host.** `IAuditAndMetricsDispatcher`
  is a port too: the library defers writes so recording does not lengthen the
  work being recorded, but only a host with a lifecycle can bound the queue and
  drain it on shutdown. Without one the library falls back to
  `ThreadPoolDispatcher` — one work item per write, unbounded, draining nothing.
- **One span, two sinks.** The library's `MetricService` persists each span
  through the storage port *and* publishes it to an `ActivitySource`.
  `MetricTelemetryPublisher` is what subscribes; before it existed every
  published span was dropped before reaching Application Insights. It lives in
  `LondonFhirService.Core/Workers` and **both hosts register it** — in Core
  rather than in either host because both need it, and rather than in the metrics
  library because that library deliberately carries no telemetry vendor of its
  own. Metric rows are purged on a retention timer, so the telemetry copy is the
  only record that outlives the sweep; a host that did not publish would lose its
  spans entirely.
- **The access decision is now delegated to a remote service, and lives with
  the patient orchestration.** `Stu3PatientOrchestrationService.ValidateAccess`
  resolves the caller, builds a `ValidateAccessRequest` (consumer user id +
  NHS number + correlation id) and hands it to `ConsumerAccessService`, a
  single-method passthrough over `ConsumerAccessBroker`. The returned
  `ConsumerAccess` decides the outcome: `IsAccessAllowed == false` audits
  "Access Forbidden" with the returned reason codes and throws; allowed audits
  "Access Allowed" naming the organisations that granted it. Every allow and
  every denial is still written to the audit trail. There is no
  `AccessOrchestrationService` any more — with one service dependency it was
  no longer an orchestration.
- **`AccessConfigurations.CheckAccessPermissions` gates the check inside
  `ValidateAccess`,** not at the coordination layer: off, it audits the skip
  and returns. `GetStructuredRecordSerialisedAsync` runs the same check first
  via the private helper `ValidateAccess` wraps, so a forbidden caller is
  localised once rather than twice.
- **The correlation id is established in the request pipeline, not in Core.**
  `CorrelationMiddleware` runs first in the API host, ahead of authentication,
  authorization and the timeout policy. It settles the id through
  `CorrelationBroker` — which keeps it on `HttpContext.Items`, so every reader
  in the request sees one value — and registers an `OnStarting` callback that
  returns it as the `X-Correlation-Id` response header. `PatientController`
  reads that same id and passes it to `Stu3PatientCoordinationService`, which
  no longer mints one of its own; it still draws its span ids from
  `IdentifierBroker`. A response that never reaches a controller — a 401, a
  403, a timeout — therefore carries the id its logs and metric spans were
  written under, which it previously could not.
- **The correlation id is a W3C trace id, not one of our own.** A caller or
  gateway that sends `traceparent` is already naming the trace the request
  belongs to; ASP.NET parses it into `Activity.Current` before any of our code
  runs, and Application Insights correlates on the same value. `CorrelationBroker`
  takes the trace id from there rather than minting a second id for the same
  request, and falls back to `IdentifierBroker` only when there is no W3C
  activity to read — a background worker, or a host with nothing listening for
  activities. The two are the same 128 bits and the same 32 hex characters, so
  `Guid.ToString("N")` of a correlation id **is** the trace id and an id read
  off a response header or an audit row pastes straight into the telemetry
  viewer. `MetricBroker.CreateTraceContext` rebuilds it from the hex for the
  same reason: it previously went through `Guid.ToByteArray`, and because Guid
  stores its first three fields little endian while a trace id is not stored
  that way at all, every replayed span landed under a byte-shuffled trace of
  its own instead of under the caller's.
- **The compare queue is safe for more than one worker, in three separate ways.**
  The claim is a single conditional `UPDATE` (`ExecuteUpdateAsync`) returning
  rows-affected, so two workers can never both take a row — on the Pending arm
  the status flip invalidates the loser, and on the reclaim arm, where
  `Processing → Processing` would make a status check vacuous, the
  `UpdatedDate <= leaseExpiry` term does it. Workers pick at **random from a
  window** of the oldest candidates rather than the head, because every worker
  runs the same ordered query and taking the head made all N converge on one row
  every cycle. And `TryRetainClaimAsync` re-asserts the claim immediately before
  anything is written: a lease that expires while work is still running looks
  exactly like one left by a worker that died, so an overtaken worker has to be
  able to find out and discard its result rather than write a second difference
  row for the pair. The primary record is completed through a conditional
  transition for the same reason — it is shared by every secondary of a
  correlation, so several workers reach it and a read-then-write there is a check
  by one and a write by another.
- **Replayed metric spans are flattened on purpose, and anchored under the
  request.** Every span of a request is given the same parent, so they reach
  Application Insights as siblings rather than as the tree they actually form.
  Reproducing the real nesting made the metric view too noisy to read; the
  telemetry copy is meant to be a scannable overview, and the exact tree stays
  in the `metric.id` / `metric.parentId` tags and in the metrics table, which is
  the authoritative store. Do not turn this into a faithful hierarchy without
  agreeing the UI change that goes with it. That shared parent is the HTTP
  request's own span, so the flat group hangs *under* the incoming request in
  the transaction view rather than beside it — flattening and anchoring are
  independent, and only the second changed. The span id reaches the replay
  through `IRequestTraceBroker`, a port the host satisfies from
  `CorrelationBroker`: `MetricService` asks for it while the request is still
  alive and carries it into the deferred write, because the replay itself runs
  on a background worker with no request left to ask. Without a request span —
  a background worker, or a host that registers no implementation and gets the
  library's null object — it falls back to a parent derived from the correlation
  id, which groups correctly but places the spans at the top of the trace.
- **Reconciliation moved up to `Stu3PatientCoordinationService`.** The
  orchestration returns a `StructuredRecordsResponse` (primary provider +
  per-provider bundles); the coordination service hands them to
  `Stu3FhirReconciliationService` — itself an orchestration — and returns the
  single serialised bundle the API hands back.
- **Consumer access is no longer held locally.** The `Consumer`, `OdsData` and
  `PdsData` entities, their foundation services and their `StorageBroker`
  partials are gone, along with the local `ConsumerAccess` table. `IStorageBroker`
  now covers `Audit`, `FhirRecord`, `FhirRecordDifference` and `Provider` only.
- **`Stu3FhirBroker` exposes 103 members and the solution consumes one.**
  Only the `FhirProviders` collection is read (by `Stu3PatientService`); the
  102 typed STU3 resource accessors are forwarded to
  `IFhirAbstractionProvider` but never called.
- **`Stu3FhirReconciliationService` does not reconcile yet.** It returns the
  first non-empty bundle and throws when every provider came back empty. It is
  modelled — and now lives — as an orchestration: it sits alongside
  `Stu3PatientOrchestrationService` under the coordination service, and its
  exceptions are the `FhirReconciliationOrchestration*` family.
- **The two hosts are not equivalent, and every admin CRUD controller now
  lives on Manage.** `LondonFhirService.Api` keeps the headline patient
  endpoint, the two config endpoints and all the background work
  (`ComparisonWorker`, `MetricPurgeWorker`, `AuditAndMetricsDispatchWorker`).
  `MetricTelemetryPublisher` is the one hosted service both run.
  `LondonFhirService.Manage` carries
  `Audits`, `Metrics`, `Providers`, `FhirRecords` and
  `FhirRecordDifferences` — audit rows carry whole patient payloads, and Manage
  is reachable only from the business IP range.
- **`Audits` and `Metrics` hide their write verbs; `Providers` does not.**
  `Metrics` and `Providers` admit `Administrators` and `Users`; `Audits` admits
  `Administrators` only, because an audit row carries a whole patient payload.
  Audit and metric writes additionally carry `[InvisibleApi]`, so the middleware
  answers 404 without the key header and they exist for the acceptance suite to
  seed and tear down. Provider writes instead narrow to `Administrators` with a
  second `[Authorize]` — a provider row decides who the patient fan-out calls
  and which source is primary, so it is operator-managed configuration rather
  than a record only tests should touch. `Metrics` has no PUT at all: a span
  records work that already happened. The role names come from `ManageRoles`,
  one constant per name the app registration carries.
- **`HashBroker` and the NHS-number hashing config are gone.** The SHA-256 hash
  existed only to build the patient identifier for the in-process PDS check;
  the remote consumer-access API does its own hashing, so `IHashBroker`,
  `System.Security.Cryptography` and `AccessConfigurations.UseHashedNhsNumber` /
  `HashPepper` were all removed. `CheckAccessPermissions` is the only setting
  left on `AccessConfigurations`.
- **`LondonFhirService.Manage.Client` is a thin SPA.** It reaches only two
  Manage endpoints: `GET /api/FrontendConfigurations` (anonymous, called
  before MSAL exists) and `GET /api/Features`.

## Modelling decisions

These are the judgement calls baked into the data files; keep them stable so
successive scans stay comparable.

- **Happy-path calls and denial logging are drawn; exception-path (`TryCatch`
  / `CreateAndLog*`) logging is NOT.** Validation partials run inside the same
  `TryCatch` as the happy path, so their `securityAuditBroker.GetUserIdAsync`
  and `dateTimeBroker` calls *are* drawn.
- **Private helpers are attributed to the public method that reaches them** —
  e.g. `Stu3PatientService.GetStructuredRecordSerialisedAsync` carries the
  calls made by `GetFhirProviders` and
  `ExecuteGetStructuredRecordSerialisedWithTimeoutAsync`.
- **Interface dispatch on a resolved instance is not drawn as an edge.**
  `ComparisonOrchestrationService` calls `MatchAsync` / `GetMatchKeyAsync` on
  whichever matcher `GetMatcherAsync` returned; follow `GetMatcherAsync` to
  see the candidates. DI fan-in (the 21 matchers, the 4 ignore rules) *is*
  drawn.
- **The 21 resource matchers are marked `shared`** even though they are
  in-solution components. They are a DI fan-in registry with no subtree of
  their own, and duplicating them per consumer chain added ~85 empty nodes.

## The data files

All data is declarative YAML — no code runs to produce the model, and
[index.html](./index.html) is a pure renderer (it holds both views,
`buildSingleCopyInstances` / `layoutBands` and `buildDuplicatedInstances` /
`layoutTrees`, dispatched on `state.view`, and should rarely need changes).

**`graph.yml`** is the manifest:

- `projects` — id, name, kind (`internal` / `library` / `external`) and the
  `file` holding that project's components. List order controls the
  single-copy view's band order.
- `roots` — component ids in layout order for the per-consumer view. A
  component flagged `shared` **must** appear here, or its inbound edges are
  silently dropped.
- `events` / `eventBroker` — empty / `null` today; ready for an event bus.

**`projects/<name>.yml`** declares one project's components:

```yaml
- id: API.Patient
  name: PatientController (STU3)
  layer: exposer          # exposer|view|coordination|orchestration|
                          # processing|foundation|broker|client|external
  col: 3                  # layout column — map documented in graph.yml
  shared: true            # optional: consumers link to ONE copy
  utility: true           # optional: hidden behind the header toggle
  deriveMethods: true     # optional: rows derived from inbound edges
                          # (externals — rows can never drift from arrows)
  description: "..."
  methods: [...]
  calls:                  # outbound flows, one per call
    - from: <method or null>   # null = header-level link
      to: <component id>
      method: <method or null>
  publishes:              # for a future event bus
    - method: M
      event: E
  subscribes:
    - event: E
      handler: H
```

Strings containing anything beyond letters, digits, spaces, `_.-/()` are
double-quoted JSON strings — the renderer parses a deliberately small YAML
subset, so stick to the shapes above (single-line scalars, no anchors, no
multi-line blocks).

## Updating the graph

The data is a scanned snapshot of the source, not a build artifact — refresh
it whenever services, brokers, or cross-project wiring change by running the
`/update-dependency-graph` skill in Claude Code (defined in
`.claude/skills/update-dependency-graph/SKILL.md`). It re-scans the solution,
diffs against the current data files, updates them, and re-verifies the
rendered graph. Small changes (a new method, one new call) are comfortable
hand-edits in the project file that owns the calling component.
