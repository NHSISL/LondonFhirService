---
name: the-standard-csharp-directives
version: 0.1.0
csharp-standard-version: v0.8
applies-to: ["*.cs"]
depends-on: []
---

# The Standard C# — Directives

## 0/ Context (Purpose — WHY this skill exists)

0.0/ Where: Any C# project following the C# Coding Standard.
0.1/ Who: Engineers writing or reviewing `using` directive blocks at the top of C# files.
0.2/ What: Enforces the ordering, grouping, and placement of `using` directives.
0.3/ Applies to: *.cs
0.4/ Version: v0.8
0.5/ Depends on: none

## 1/ Actual (Dependency — WHAT the rules are and what they depend on)

1.0/ Dos:
  1. Place all `using` directives at the top of the file, outside any namespace block → see rules/rules.md#tsc-csharp-directives-001
  2. Order `using` groups as: System → Microsoft → third-party → project-local → see rules/rules.md#tsc-csharp-directives-002
  3. Separate each group with one blank line → see rules/rules.md#tsc-csharp-directives-003
  4. Sort directives alphabetically within each group → see rules/rules.md#tsc-csharp-directives-004
  5. Remove all unused `using` directives → see rules/rules.md#tsc-csharp-directives-005

1.1/ Don'ts:
  1. Must not place `using` directives inside a namespace block → see validations/anti-patterns.md#directives-inside-namespace
  2. Must not mix groups without a blank line separator → see validations/anti-patterns.md#missing-group-separator
  3. Must not leave unused `using` directives in the file → see validations/anti-patterns.md#unused-directives

1.2/ Ask:
  - Ask when it is unclear whether a reference belongs to the project namespace or a third-party package.

1.3/ Defaults:
  - Default group order: System → Microsoft → third-party → project-local.
  - Default within-group ordering: alphabetical.

1.4/ Examples:
  - ✅ see examples/good/example_good_directives.cs
  - ❌ see examples/bad/example_bad_directives.cs

1.5/ Templates:
  - Scaffold directive block: see templates/

1.6/ Checklists:
  - Pre-review checklist: see validations/checklist.md

1.7/ Contracts:
  - Formal contracts: see contracts/contracts.json

## 2/ Expected (Exposure — WHAT comes out)

2.0/ Format: C# `using` directive blocks.
2.1/ Outcome: All directives are outside the namespace, in the correct group order with blank-line separators, sorted alphabetically within each group, and unused directives are removed.
2.2/ Tone: Direct. Cite rule IDs. Reorder and remove directives without asking.
