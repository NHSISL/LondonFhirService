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

At the last scan, 92 declared components and 431 declared edges draw as
**89 components · 406 flows** in the single-copy view and **258 nodes ·
1016 flows** per consumer (92 · 431 and 294 · 1069 with utility brokers on).

`.github/workflows/pages.yml` publishes this folder to GitHub Pages on every
push to `main` that touches it — `index.html` is the site root. Nothing is
compiled; `index.html`, `graph.yml` and `projects/` are copied as-is. Pages
has to be enabled once in the repository's Settings → Pages (source: GitHub
Actions).

## Current truths captured in the data (scanned 2026-08-25)

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
  `MetricTelemetryPublisher` on the API host is what subscribes; before it
  existed every published span was dropped before reaching Application Insights.
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
  (`ComparisonWorker`, `MetricPurgeWorker`, `AuditAndMetricsDispatchWorker`,
  `MetricTelemetryPublisher`). `LondonFhirService.Manage` carries
  `Audits`, `Metrics`, `Providers`, `FhirRecords` and
  `FhirRecordDifferences` — audit rows carry whole patient payloads, and Manage
  is reachable only from the business IP range.
- **`Audits` and `Metrics` hide their write verbs; `Providers` does not.** All
  three are `[Authorize(Roles = "Administrators,Users")]`. Audit and metric
  writes additionally carry `[InvisibleApi]`, so the middleware answers 404
  without the key header and they exist for the acceptance suite to seed and
  tear down. Provider writes instead narrow to `Administrators` with a second
  `[Authorize]` — a provider row decides who the patient fan-out calls and
  which source is primary, so it is operator-managed configuration rather than
  a record only tests should touch. `Metrics` has no PUT at all: a span records
  work that already happened.
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
