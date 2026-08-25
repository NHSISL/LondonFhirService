---
name: the-standard-ui
version: 0.1.0
standard-version: v2.50.0
applies-to: ["*.razor", "*Component*.cs", "*Base*.cs", "*Page*.cs"]
depends-on: ["the-standard-core"]
---

# The Standard — User Interfaces (Blazor / Web)

## 0/ Context (Purpose — WHY this skill exists)

0.0/ Where: The UI layer of Standard-compliant web applications (Blazor, Razor components).
0.1/ Who: Engineers implementing or reviewing UI components, pages, and view services.
0.2/ What: Enforces component decomposition, view service dependency, single responsibility, and separation of rendering from logic.
0.3/ Applies to: *.razor, *Component*.cs, *Base*.cs, *Page*.cs
0.4/ Version: v2.50.0
0.5/ Depends on: the-standard-core

## 1/ Actual (Dependency — WHAT the rules are and what they depend on)

1.0/ Dos:
  1. UI components must delegate all data access and business calls to a view service → see rules/rules.md#ts-ui-001
  2. UI components must be decomposed — each component must have a single rendering concern → see rules/rules.md#ts-ui-002
  3. View services must map UI-layer exceptions and expose only view-friendly models → see rules/rules.md#ts-ui-003
  4. Pages must use a base component class (`*Base.cs`) to separate code-behind logic from markup → see rules/rules.md#ts-ui-004
  5. UI components must handle loading, error, and empty states explicitly → see rules/rules.md#ts-ui-005
  6. Component names must describe what they render — not how → see rules/rules.md#ts-ui-006

1.1/ Don'ts:
  1. Must not call services or brokers directly from `.razor` markup → see validations/anti-patterns.md#razor-calls-service
  2. Must not mix data-fetching logic with rendering markup in the same file → see validations/anti-patterns.md#mixed-concerns
  3. Must not use code-behind `@code {}` blocks for anything beyond UI state binding → see validations/anti-patterns.md#code-behind-logic
  4. Must not create mega-components that render an entire page in a single component → see validations/anti-patterns.md#mega-component

1.2/ Ask:
  - Ask when a component renders data from more than one view model — it may need decomposition.

1.3/ Defaults:
  - Base class naming: `{ComponentName}Base` inherits `ComponentBase`.
  - View service naming: `I{Entity}ViewService` / `{Entity}ViewService`.
  - Loading state: show a spinner or skeleton until data is loaded.
  - Error state: show a user-friendly message, log internally.

1.4/ Examples:
  - ✅ see examples/good/example_good_component.razor
  - ❌ see examples/bad/example_bad_component.razor

1.5/ Templates:
  - Scaffold a new Blazor component: see templates/

1.6/ Checklists:
  - Pre-review checklist: see validations/checklist.md

1.7/ Contracts:
  - Component and view service naming contracts: see contracts/contracts.json

## 2/ Expected (Exposure — WHAT comes out)

2.0/ Format: Razor (.razor) and C# source code.
2.1/ Outcome: UI components with single rendering concerns, clean separation of logic, and correct view service dependency.
2.2/ Tone: Direct. Cite rule IDs. Violations must be fixed, not suggested.
