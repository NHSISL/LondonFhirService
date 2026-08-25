---
name: update-dependency-graph
description: Re-scan the solution and regenerate Documentation/DependencyGraph/graph-data.js so the interactive dependency graph matches the current source. Use when services, brokers, controllers, workers, client libraries, or cross-project wiring have changed, or when the user asks to refresh/rebuild the dependency graph.
version: 0.1.0
---

# Update Solution Dependency Graph

Regenerate `Documentation/DependencyGraph/graph-data.js` from the current
source. `index.html` is the renderer — do not change it unless a new concept
cannot be expressed in data (new edge kind, new layer). It carries BOTH views
behind `state.view`: `buildSingleCopyInstances` + `layoutBands` (the default)
and `buildDuplicatedInstances` + `layoutTrees`. Anything you change in one
builder usually needs the mirror change in the other.

## 1/ Load the current model

Read `Documentation/DependencyGraph/README.md` and `graph-data.js` first.
The data file is the previous scan's snapshot; your job is a diff-and-update,
not a rewrite. Preserve its modelling rules:

- Per-consumer duplication is done by the renderer — declare each component
  ONCE; never hand-duplicate.
- `shared: true` on client-library / external exposers and on the 21 resource
  matchers (a DI fan-in registry with no subtree). A `shared` component MUST
  also be in `roots` or its inbound edges are silently dropped.
- `utility: true` on the DateTime / Identifier / Logging brokers (hidden
  behind a toggle).
- Happy-path calls and denial logging are drawn; exception-path (`TryCatch` /
  `CreateAndLog*`) logging is NOT. Validation partials run inside the same
  `TryCatch` as the happy path, so their broker calls ARE drawn.
- Private helpers are attributed to the public method that reaches them.
- Interface dispatch on an instance resolved at runtime is not an edge; DI
  fan-in is.
- This solution has no event bus — `events` is empty, `eventBrokerId` is
  `null`, and every edge is `kind: "direct"`. If an EventBroker ever lands,
  the renderer already supports `P(...)` / `S(...)` and automatic
  circular-flow detection; do not hand-colour anything.
- Column map (0–12) is documented at the top of `graph-data.js` — keep new
  components consistent with it.

## 2/ Re-scan the source

Cover these four areas. Read the interfaces for the public surface and the
implementation `.cs` for the per-method calls; a quick way to get per-method
dependency calls out of a C# tree is a small throwaway script that finds
method declarations and then the `this.<field>.<Method>` calls between one
declaration and the next (whitespace-normalise first — most calls in this
codebase wrap across lines).

1. **Foundation services** — `LondonFhirService.Core\Services\Foundations\*`:
   folder list, public interface methods, per-method broker calls
   (Storage / StorageBrokerFactory / SecurityAudit / Security / Audit /
   DateTime / Identifier / Logging / Hash). The uniform CRUD services are
   templated: classify each into variant A (audited CRUD, hard delete),
   B (audited CRUD, remove-audit stamp + Update before the Delete), or
   C (no security-audit broker) — or flag a genuine deviation.
   `AuditService`, `Stu3PatientService`, `Stu3FhirReconciliationService`,
   `JsonElementService` and the resource matchers are declared explicitly.
2. **Processings, orchestrations, coordinations** —
   `Services\Processings\*`, `Services\Orchestrations\*`,
   `Services\Coordinations\*`: dependencies and per-method calls. Watch for
   `IEnumerable<T>` constructor injection (matchers, ignore rules) — that is
   fan-in and gets an edge per implementation.
3. **Brokers and clients** — `Core\Brokers\*`, `Core\Clients\*`: public
   surface plus which external package each one wraps
   (`ISL.Security.Client`, `STX.EFCore.Client`, `EFxceptions` / EF Core,
   `LondonFhirService.Providers.FHIR.STU3.*`, `Microsoft.Extensions.Logging`,
   `Azure.Identity`, `System.Security.Cryptography`). Note any broker that is
   registered but never consumed — that is a headline truth.
4. **Hosts and SPA** — `LondonFhirService.Api` and `LondonFhirService.Manage`:
   every controller route → service call, `Workers\*`, and the
   `Program.Configurations.cs` `Add*` methods (the two hosts do NOT register
   the same stack). Then `LondonFhirService.Manage.Client\src`: components →
   `services\foundations\*` → `brokers\*` → the host endpoints they call.

## 3/ Update graph-data.js

- New/changed uniform CRUD service → edit the `CRUD_ENTITIES` config (usually
  one line: entity name + plural + variant). It extends the `StorageBroker`
  surface and its `EFCoreClient` edges automatically. A structural deviation
  from the template → extend the generator or declare it explicitly next to
  `AuditService`, whichever is smaller.
- New matcher / ignore rule / REST controller / FHIR accessor → add a line to
  `MATCHERS`, `IGNORE_RULES`, `restControllers` or `FHIR_RESOURCES`.
- Everything else → explicit declarations: `C({...})` components and
  `D(from, to)` direct edges (`null` method = header-level link).
- Add new roots to the `roots` list in project order (it controls layout).
- External / library components' method rows are DERIVED from the edges at the
  bottom of the file — add the id to that loop rather than hand-listing rows.

## 4/ Verify in the browser

Both views read the same data. Serve the folder over HTTP — a `file://` load
blocks `graph-data.js` as a sub-resource and the page renders empty:

```bash
python -m http.server 8731 --bind 127.0.0.1
```

Verify BOTH views — the header toggle, or `setView("single")` /
`setView("duplicated")` from `javascript_tool`. Confirm:

- No console errors; the header count is in the expected range (last scan:
  81 components · 388 flows single-copy; 334 nodes · 995 flows per consumer,
  420 · 1110 with utility brokers on).
- No node-rect overlaps and no project-box overlaps — query `state.instances`
  and `state.projBoxes` with `javascript_tool` and intersect pairwise, in each
  view, with the utility toggle both off and on.
- Switching view preserves the selection (by component id) and both views
  agree on the edge-kind counts they share.
- No dropped edges: every `shared` component appears in `roots`.
- Click one foundation service, one orchestration, one shared client exposer
  and one method row: the side-panel flows in / out must match the scan.
- Selecting a header must light the component's whole fan-out (the same
  upstream + downstream slice a method row gets, seeded from every row), not
  just its first hop, and the selection must be outlined in amber. Clearing
  the selection must restore the graph exactly — snapshot every node's
  attributes before and after and compare.
- Red edges appear ONLY if a real publish/subscribe cycle now exists — if one
  shows up, verify it against the source before accepting it.

## 5/ Finish

Update the "Current truths" section and scan date in
`Documentation/DependencyGraph/README.md` (and the node/flow counts if they
moved), and summarize what changed since the previous snapshot — new
components, new flows, anything that became unreachable or newly consumed.
