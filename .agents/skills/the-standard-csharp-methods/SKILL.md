---
name: the-standard-csharp-methods
version: 0.1.0
csharp-standard-version: v0.8
applies-to: ["*.cs"]
depends-on: ["the-standard-csharp-variables"]
---

# The Standard C# — Methods

## 0/ Context (Purpose — WHY this skill exists)

0.0/ Where: Any C# project following the C# Coding Standard.
0.1/ Who: Engineers writing or reviewing C# method definitions and calls.
0.2/ What: Enforces method naming, parameter naming, one-liner vs. multi-liner style, return spacing, chaining (uglification), and 120-character line limits.
0.3/ Applies to: *.cs
0.4/ Version: v0.8
0.5/ Depends on: the-standard-csharp-variables

## 1/ Actual (Dependency — WHAT the rules are and what they depend on)

1.0/ Dos:
  1. Method names must contain a verb describing the action performed → see rules/rules.md#tsc-csharp-methods-001
  2. Async methods must be postfixed with `Async` → see rules/rules.md#tsc-csharp-methods-002
  3. Input parameters must be named for the specific property or action they represent → see rules/rules.md#tsc-csharp-methods-003
  4. Action-specific parameters must include the entity name (e.g., `studentId`, not `id`) → see rules/rules.md#tsc-csharp-methods-004
  5. Pass named aliases when variable names differ from parameter names → see rules/rules.md#tsc-csharp-methods-005
  6. One-liner methods must use fat-arrow (`=>`) syntax → see rules/rules.md#tsc-csharp-methods-006
  7. Fat-arrow lines exceeding 120 characters must break after `=>` with one extra tab indent → see rules/rules.md#tsc-csharp-methods-007
  8. Multi-liner methods must use a scoped block (`{ }`) → see rules/rules.md#tsc-csharp-methods-008
  9. In multi-liner methods, the final `return` statement must be preceded by a blank line → see rules/rules.md#tsc-csharp-methods-009
  10. When two consecutive calls are both under 120 characters, stack them; otherwise separate with a blank line → see rules/rules.md#tsc-csharp-methods-010
  11. Method declarations exceeding 120 characters must break parameters onto the next line (one per line) → see rules/rules.md#tsc-csharp-methods-011
  12. Method chains must use uglification: each additional call indented one extra tab → see rules/rules.md#tsc-csharp-methods-012

1.1/ Don'ts:
  1. Must not name a method without a verb → see validations/anti-patterns.md#no-verb-in-name
  2. Must not omit `Async` suffix on async methods → see validations/anti-patterns.md#missing-async-suffix
  3. Must not use generic parameter names like `text`, `name`, `id` without entity prefix → see validations/anti-patterns.md#generic-param-name
  4. Must not pass unnamed literal values to method calls → see validations/anti-patterns.md#unnamed-literal-args
  5. Must not use fat-arrow for multi-liner logic or chained calls → see validations/anti-patterns.md#fat-arrow-multiline
  6. Must not omit blank line before `return` in multi-liner methods → see validations/anti-patterns.md#no-blank-before-return
  7. Must not start a chain on a new line without uglification indentation → see validations/anti-patterns.md#no-uglification

1.2/ Ask:
  - Ask when a method name contains multiple verbs — it may need to be split into two methods.

1.3/ Defaults:
  - Default line limit: 120 characters per line.
  - Default: one-liner → fat arrow; multi-liner → scoped block.

1.4/ Examples:
  - ✅ see examples/good/example_good_methods.cs
  - ❌ see examples/bad/example_bad_methods.cs

1.5/ Templates:
  - Scaffold a method: see templates/

1.6/ Checklists:
  - Pre-review checklist: see validations/checklist.md

1.7/ Contracts:
  - Formal contracts: see contracts/contracts.json

## 2/ Expected (Exposure — WHAT comes out)

2.0/ Format: C# method definitions and call sites.
2.1/ Outcome: All methods use verbs, correct async suffix, descriptive parameter names, appropriate one-liner/multi-liner style, correct return spacing, and proper uglification for chains.
2.2/ Tone: Direct. Cite rule IDs. Correct violations without asking.
