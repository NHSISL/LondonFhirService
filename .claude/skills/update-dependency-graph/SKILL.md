---
name: update-dependency-graph
description: Re-scan the solution and regenerate the dependency graph data files (Documentation/DependencyGraph/graph.yml + projects/*.yml) so the interactive dependency graph matches the current source. Use when services, brokers, controllers, workers, client libraries, or cross-project wiring have changed, or when the user asks to refresh/rebuild the dependency graph.
version: 0.1.0
---

# Update Solution Dependency Graph

Regenerate the data files — `Documentation/DependencyGraph/graph.yml` and
`projects/*.yml` — from the current source. `index.html` is the renderer — do not change it unless a new concept
cannot be expressed in data (new edge kind, new layer). It carries BOTH views
behind `state.view`: `buildSingleCopyInstances` + `layoutBands` (the default)
and `buildDuplicatedInstances` + `layoutTrees`. Anything you change in one
builder usually needs the mirror change in the other.

## 1/ Load the current model

Read `Documentation/DependencyGraph/README.md`, then `graph.yml` and the
`projects/*.yml` files it lists. The data files are the previous scan's
snapshot; your job is a diff-and-update,
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
- This solution has no event bus — `events` is empty, `eventBroker` is
  `null`, and every flow is a `calls` entry. If an EventBroker ever lands,
  the data supports `publishes` / `subscribes` per component and the renderer
  detects circular flows automatically; do not hand-colour anything.
- Column map (0–12) is documented in `graph.yml` — keep new components
  consistent with it.
- The historical CRUD template knowledge (variants A/B/C below) guides the
  SCAN; the data itself is fully expanded YAML with no generators. When a
  change touches many components the same way (a new broker call in every
  CRUD service, a new entity's StorageBroker rows), write a throwaway script
  against the YAML rather than hand-editing dozens of entries.

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

## 3/ Update the data files

The YAML schema is documented in the README's "The data files" section —
components live in `projects/<project>.yml`, each with `methods` and its
outbound `calls` (`from: null` = header-level link); manifest-level lists
(`projects`, `roots`, `events`) live in `graph.yml`.

- A new component → add it to its project's file AND to `roots` in
  `graph.yml` (project order; `shared` components must be roots).
- A new uniform CRUD service → replicate an existing sibling's block (the
  variant patterns above say which broker calls it makes), and add the
  entity's five StorageBroker rows + their `LIB.EFCoreClient` calls to the
  StorageBroker component in the core file.
- Externals / library exposers with `deriveMethods: true` get their rows
  derived from inbound edges at load time — never hand-list rows on them.
- Strings with characters beyond letters, digits, spaces and `_.-/()` must be
  double-quoted JSON strings; the renderer parses a small YAML subset
  (single-line scalars only, no anchors, no multi-line blocks).
- Sanity-check your edits parse before opening the browser:
  `node -e "eval(...)"` is gone — instead fetch-and-parse via any quick
  script that mirrors index.html's `parseYaml`, or just load the page and
  watch for its "graph data did not load" panel, which prints the error.

## 4/ Verify in the browser

Both views read the same data. Serve the folder over HTTP — the page fetches graph.yml and the project
files, and browsers block those fetches from file:// pages:

```bash
python -m http.server 8731 --bind 127.0.0.1
```

Verify BOTH views — the header toggle, or `window.__graph.setView("single")` /
`window.__graph.setView("duplicated")` from `javascript_tool` (the renderer
exposes `window.__graph` = { state, setView, select, selectRow,
clearSelection, rebuild, fit, tracePath }). Confirm:

- No console errors; the header count is in the expected range (last scan:
  81 components · 388 flows single-copy; 334 nodes · 995 flows per consumer,
  420 · 1110 with utility brokers on).
- No node-rect overlaps and no project-box overlaps — query `window.__graph.state.instances`
  and `.projBoxes` with `javascript_tool` and intersect pairwise, in each
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
