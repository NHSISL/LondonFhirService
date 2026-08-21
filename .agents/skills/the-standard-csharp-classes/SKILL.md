---
name: the-standard-csharp-classes
version: 0.1.0
csharp-standard-version: v0.8
applies-to: ["*.cs"]
depends-on: ["the-standard-csharp-files", "the-standard-csharp-variables"]
---

# The Standard C# — Classes

## 0/ Context (Purpose — WHY this skill exists)

0.0/ Where: Any C# project following the C# Coding Standard.
0.1/ Who: Engineers writing, naming, or reviewing C# classes and interfaces.
0.2/ What: Enforces class naming conventions, field naming and referencing, and instantiation patterns.
0.3/ Applies to: *.cs
0.4/ Version: v0.8
0.5/ Depends on: the-standard-csharp-files, the-standard-csharp-variables

## 1/ Actual (Dependency — WHAT the rules are and what they depend on)

1.0/ Dos:
  1. Model class names must match the entity name with no suffix → see rules/rules.md#tsc-csharp-classes-001
  2. Service class names must be singular: `{Entity}Service` → see rules/rules.md#tsc-csharp-classes-002
  3. Broker class names must be singular: `{Entity}Broker` → see rules/rules.md#tsc-csharp-classes-003
  4. Controller class names must be plural: `{Entities}Controller` → see rules/rules.md#tsc-csharp-classes-004
  5. Class fields must use camelCase naming → see rules/rules.md#tsc-csharp-classes-005
  6. Private fields must be referenced with `this.` keyword inside constructors and methods → see rules/rules.md#tsc-csharp-classes-006
  7. Instantiation must use named parameters when parameter values do not match variable names → see rules/rules.md#tsc-csharp-classes-007
  8. Use `var` (not explicit type) on the left side of `new` instantiation → see rules/rules.md#tsc-csharp-classes-008
  9. Property assignment in object initializers must honor declaration order → see rules/rules.md#tsc-csharp-classes-009

1.1/ Don'ts:
  1. Must not suffix model names with `Model` → see validations/anti-patterns.md#model-suffix
  2. Must not use plural names for service classes → see validations/anti-patterns.md#plural-service
  3. Must not use descriptive suffixes like `BusinessLogic` or `BL` → see validations/anti-patterns.md#wrong-service-suffix
  4. Must not prefix fields with underscore (`_`) → see validations/anti-patterns.md#underscore-field
  5. Must not use `PascalCase` for private fields → see validations/anti-patterns.md#pascalcase-field
  6. Must not pass positional unnamed literal values to constructors → see validations/anti-patterns.md#unnamed-literals
  7. Must not use explicit type on the left of `new(...)` with target-type new → see validations/anti-patterns.md#explicit-type-new
  8. Must not assign properties out of declaration order in initializers → see validations/anti-patterns.md#wrong-property-order

1.2/ Ask:
  - Ask when unsure whether a class is a model, service, broker, or controller — the suffix rules differ per type.

1.3/ Defaults:
  - Default field casing: camelCase.
  - Default field reference style: `this.fieldName`.
  - Default instantiation: `var entity = new EntityName(param: value)`.

1.4/ Examples:
  - ✅ see examples/good/example_good_classes.cs
  - ❌ see examples/bad/example_bad_classes.cs

1.5/ Templates:
  - Scaffold a class: see templates/

1.6/ Checklists:
  - Pre-review checklist: see validations/checklist.md

1.7/ Contracts:
  - Formal contracts: see contracts/contracts.json

## 2/ Expected (Exposure — WHAT comes out)

2.0/ Format: C# class definitions.
2.1/ Outcome: All classes use the correct naming suffix per type, fields use camelCase with `this.` reference, and instantiation uses named parameters in declaration order.
2.2/ Tone: Direct. Cite rule IDs. Non-compliant class names and field patterns must be corrected.
