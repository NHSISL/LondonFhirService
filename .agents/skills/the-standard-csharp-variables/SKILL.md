---
name: the-standard-csharp-variables
version: 0.1.0
csharp-standard-version: v0.8
applies-to: ["*.cs"]
depends-on: []
---

# The Standard C# — Variables

## 0/ Context (Purpose — WHY this skill exists)

0.0/ Where: Any C# project following the C# Coding Standard.
0.1/ Who: Engineers writing or reviewing local variable declarations in C# files.
0.2/ What: Enforces variable naming, `var` vs. explicit type rules, single-property instantiation, 120-char line limits, and multi-declaration spacing.
0.3/ Applies to: *.cs
0.4/ Version: v0.8
0.5/ Depends on: none

## 1/ Actual (Dependency — WHAT the rules are and what they depend on)

1.0/ Dos:
  1. Variable names must be concise and fully spell out the concept — no abbreviations → see rules/rules.md#tsc-csharp-variables-001
  2. Collection variables must be plural — not suffixed with `List`, `Array`, or type names → see rules/rules.md#tsc-csharp-variables-002
  3. Variable names must not include the type as a suffix or prefix → see rules/rules.md#tsc-csharp-variables-003
  4. Null or default-valued variables must be named `no{Entity}` or `no{Concept}` → see rules/rules.md#tsc-csharp-variables-004
  5. Use `var` when the right-hand side type is immediately clear (new, literal) → see rules/rules.md#tsc-csharp-variables-005
  6. Use explicit type when the right-hand side type is semi-clear (method return) → see rules/rules.md#tsc-csharp-variables-006
  7. Use `var` for anonymous or unknown types → see rules/rules.md#tsc-csharp-variables-007
  8. For single-property instantiation, declare the object then assign the property on the next line → see rules/rules.md#tsc-csharp-variables-008
  9. For two or more properties, use an object initializer block → see rules/rules.md#tsc-csharp-variables-009
  10. Break variable declarations exceeding 120 characters at the `=` sign → see rules/rules.md#tsc-csharp-variables-010
  11. Single-line declarations must be stacked with no blank lines between them → see rules/rules.md#tsc-csharp-variables-011
  12. Declarations that occupy two or more lines must be surrounded by blank lines → see rules/rules.md#tsc-csharp-variables-012

1.1/ Don'ts:
  1. Must not use single-letter or abbreviated variable names → see validations/anti-patterns.md#abbreviated-names
  2. Must not suffix collection names with `List` or type name → see validations/anti-patterns.md#collection-type-suffix
  3. Must not suffix variable names with `Model`, `Obj`, or type name → see validations/anti-patterns.md#type-suffix
  4. Must not name a null/default variable without the `no` prefix → see validations/anti-patterns.md#missing-no-prefix
  5. Must not use explicit type when the right side is clearly typed → see validations/anti-patterns.md#explicit-when-clear
  6. Must not use `var` when the right-hand type requires inference from a method name → see validations/anti-patterns.md#var-when-semi-clear
  7. Must not use an object initializer for a single-property type → see validations/anti-patterns.md#initializer-for-single-property
  8. Must not leave a multi-line declaration without surrounding blank lines → see validations/anti-patterns.md#missing-blank-lines

1.2/ Ask:
  - Ask when unsure if a method return type is "clear" or "semi-clear" — if the method name alone communicates the type, it is semi-clear (use explicit type).

1.3/ Defaults:
  - Default naming: full word, camelCase, describes the concept.
  - Default null naming: `no{Entity}` e.g., `noStudent`.
  - Default: `var` for clear types; explicit for semi-clear; `var` for anonymous.

1.4/ Examples:
  - ✅ see examples/good/example_good_variables.cs
  - ❌ see examples/bad/example_bad_variables.cs

1.5/ Templates:
  - Scaffold variable declarations: see templates/

1.6/ Checklists:
  - Pre-review checklist: see validations/checklist.md

1.7/ Contracts:
  - Formal contracts: see contracts/contracts.json

## 2/ Expected (Exposure — WHAT comes out)

2.0/ Format: C# local variable declarations.
2.1/ Outcome: All variables are fully named, correctly typed with `var` or explicit type, null variables use `no` prefix, single-property types use direct assignment, and multi-line declarations are surrounded by blank lines.
2.2/ Tone: Direct. Cite rule IDs. Correct violations without asking.
