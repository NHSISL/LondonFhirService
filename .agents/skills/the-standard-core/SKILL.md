---
name: the-standard-core
version: 0.1.0
standard-version: v2.50.0
applies-to: ["*.cs"]
depends-on: []
---

# The Standard — Core Principles

## 0/ Context (Purpose — WHY this skill exists)

0.0/ Where: Any Standard-compliant system, all layers.
0.1/ Who: Engineers designing, implementing, or reviewing any component.
0.2/ What: Enforces the foundational theory, purposing, modeling, simulation, and principles of The Standard.
0.3/ Applies to: *.cs
0.4/ Version: v2.50.0
0.5/ Depends on: none

## 1/ Actual (Dependency — WHAT the rules are and what they depend on)

1.0/ Dos:
  1. Every component must serve one and only one purpose → see rules/rules.md#ts-core-001
  2. Name every artifact by what it is, not how it works → see rules/rules.md#ts-core-002
  3. Depend only on the layer directly below — never skip layers → see rules/rules.md#ts-core-003
  4. Every public member must be testable without modification → see rules/rules.md#ts-core-004
  5. Expose only what callers need; hide all implementation details → see rules/rules.md#ts-core-005
  6. All business rules must live in a service, never in a controller or broker → see rules/rules.md#ts-core-006
  7. Code must communicate intent clearly without requiring comments → see rules/rules.md#ts-core-007
  8. Favour small, composable units over large monolithic implementations → see rules/rules.md#ts-core-008
  9. Separate what changes frequently from what stays stable → see rules/rules.md#ts-core-009

1.1/ Don'ts:
  1. Must not let a layer skip a level and call two layers below it → see validations/anti-patterns.md#layer-skipping
  2. Must not mix infrastructure and business concerns in the same class → see validations/anti-patterns.md#mixed-concerns
  3. Must not name a method or variable in a way that requires a comment to explain it → see validations/anti-patterns.md#poor-naming

1.2/ Ask:
  - Ask when a component's purpose is ambiguous — clarify before implementing.
  - Ask when a dependency relationship crosses more than one layer boundary.

1.3/ Defaults:
  - When a name is unclear, rename it — do not add a comment.
  - When a class grows beyond one responsibility, split it.
  - Default layer order: Broker → Foundation → Processing → Orchestration → Aggregation → Exposer.

1.4/ Examples:
  - ✅ see examples/good/example_good_single_responsibility.cs
  - ❌ see examples/bad/example_bad_mixed_concerns.cs

1.5/ Templates:
  - Scaffold a new component: see templates/

1.6/ Checklists:
  - Pre-review checklist: see validations/checklist.md

1.7/ Contracts:
  - Formal layer contracts: see contracts/contracts.json

## 2/ Expected (Exposure — WHAT comes out)

2.0/ Format: C# source code.
2.1/ Outcome: Components that are single-purpose, correctly named, layer-aware, and fully testable.
2.2/ Tone: Direct. Cite rule IDs. Violations must be fixed, not suggested.
