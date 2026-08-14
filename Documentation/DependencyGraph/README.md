# Solution Dependency Graph

An interactive, self-contained dependency graph of the London FHIR Service
solution: project boundaries, per-component method blocks, and colour-coded
data flows. No build step and no server — open [index.html](./index.html) in
a browser.

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

At the last scan, 80 declared components and 343 declared edges draw as
**77 components · 318 flows** in the single-copy view and **265 nodes ·
811 flows** per consumer (325 · 896 with utility brokers on).

`.github/workflows/pages.yml` publishes this folder to GitHub Pages on every
push to `main` that touches it — `index.html` is the site root. Nothing is
compiled; `index.html` and `graph-data.js` are copied as-is. Pages has to be
enabled once in the repository's Settings → Pages (source: GitHub Actions).

## Current truths captured in the data (scanned 2026-08-14)

- **`LondonFhirService.Core` has no event bus.** Every flow is a direct call.
  The comparison half of the solution is driven by polling, not messaging:
  `ComparisonWorker` (a `BackgroundService` in the API host) is the *only*
  entry point into `ComparisonCoordinationService`.
- **`AuditBroker` → `AuditClient` → `AuditService`** is the one place where a
  broker calls back into a foundation service, so the audit write path
  crosses the layering backwards. It is drawn faithfully: the `AuditService`
  copy under each `AuditClient` copy sits to the *left* of its consumer.
- **`AuditService` is the only foundation service that takes
  `IStorageBrokerFactory`.** Every write and every read-by-id opens and
  disposes its own `StorageBroker`, because audits are written from parallel
  provider calls and from background work where the request-scoped context is
  unsafe. Only `RetrieveAllAuditsAsync` uses the injected scoped broker.
  `Stu3PatientService` uses the factory for the same reason.
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
- **`HashBroker` is registered but never consumed.** Both hosts wire
  `IHashBroker`, but the SHA-256 hash of the NHS number existed only for the
  in-process PDS check; the remote consumer-access API does its own hashing.
  It shows on the graph as a root with no inbound flows.
- **`Stu3FhirBroker` exposes 103 members and the solution consumes one.**
  Only the `FhirProviders` collection is read (by `Stu3PatientService`); the
  102 typed STU3 resource accessors are forwarded to
  `IFhirAbstractionProvider` but never called.
- **`Stu3FhirReconciliationService` does not reconcile yet.** It returns the
  first non-empty bundle and throws when every provider came back empty. It is
  modelled — and now lives — as an orchestration: it sits alongside
  `Stu3PatientOrchestrationService` under the coordination service, and its
  exceptions are the `FhirReconciliationOrchestration*` family.
- **The two hosts are not equivalent.** `LondonFhirService.Api` registers the
  whole stack (providers, orchestrations, processings, coordinations,
  background worker). `LondonFhirService.Manage` registers brokers, clients
  four foundation services and the reconciliation orchestration only — its
  `AddProcessingServices` and `AddCoordinationServices` are empty methods.
- **`LondonFhirService.Manage.Client` is a thin SPA.** It reaches only two
  Manage endpoints: `GET /api/FrontendConfigurations` (anonymous, called
  before MSAL exists) and `GET /api/Features`.

## Modelling decisions

These are the judgement calls baked into `graph-data.js`; keep them stable so
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

## Updating the graph

The data is a scanned snapshot of the source, not a build artifact — refresh
it whenever services, brokers, or cross-project wiring change by running the
`/update-dependency-graph` skill in Claude Code (defined in
`.claude/skills/update-dependency-graph/SKILL.md`). It re-scans the solution,
diffs against the current data, updates `graph-data.js`, and re-verifies the
rendered graph.

For small changes you can also edit by hand: all data lives in
[graph-data.js](./graph-data.js) (`window.LFS_DATA`);
[index.html](./index.html) is the renderer — it holds both views
(`buildSingleCopyInstances` / `layoutBands` and `buildDuplicatedInstances` /
`layoutTrees`, dispatched on `state.view`) and should rarely need changes.

- The three uniform CRUD foundation services are generated from the
  `CRUD_ENTITIES` config (entity name + read/write variant A/B/C). A new one
  is usually one added line, and it extends the `StorageBroker` surface
  automatically.
- The 21 resource matchers come from `MATCHERS`, the 4 ignore rules from
  `IGNORE_RULES`, the REST controllers from `restControllers`, and
  `Stu3FhirBroker`'s 102 typed accessors from `FHIR_RESOURCES`.
- Everything else is declared explicitly with `C(...)` components and
  `D(from, to)` direct edges (`null` method = header-level link).
  `P(component, method, event)` / `S(event, component, handler)` exist for a
  future event bus.
- Component options: `col` (layout column), `utility: true` (hidden behind the
  toggle), `shared: true` (consumers link to one copy instead of duplicating —
  **must** also appear in `roots`, or its inbound edges are dropped).
- Add new roots to the `roots` list in project order; it controls layout.
