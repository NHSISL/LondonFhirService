---
skill: the-standard-csharp-classes
type: anti-patterns
source-section: "4. Classes"
---

# The Standard C# — Classes — Anti-Patterns

## Model Suffix

**Violates:** tsc-csharp-classes-001
**What happens:** A model class is named `StudentModel` instead of `Student`.
**Why it's wrong:** The `Model` suffix is redundant — the class IS the model. Suffixing it adds noise and violates Standard naming.
**Fix:** Remove the suffix: `Student`.

## Plural Service Name

**Violates:** tsc-csharp-classes-002
**What happens:** A service class is named `StudentsService` (plural).
**Why it's wrong:** Service class names must be singular. The class represents operations on a single entity type.
**Fix:** Rename to `StudentService`.

## Wrong Service Suffix

**Violates:** tsc-csharp-classes-003
**What happens:** A service class is named `StudentBusinessLogic` or `StudentBL`.
**Why it's wrong:** Only the `Service` suffix is permitted for business logic classes. Alternative suffixes are non-standard and obscure intent.
**Fix:** Rename to `StudentService`.

## Underscore Field Prefix

**Violates:** tsc-csharp-classes-007
**What happens:** A field is declared as `private readonly string _studentName`.
**Why it's wrong:** Fields must use plain camelCase without any prefix. The `this.` keyword is used at the call site to disambiguate scope.
**Fix:** Rename to `studentName` and use `this.studentName` when referencing it.

## PascalCase Field

**Violates:** tsc-csharp-classes-006
**What happens:** A field is declared as `private readonly string StudentName`.
**Why it's wrong:** Fields must use camelCase. PascalCase is reserved for properties and public members.
**Fix:** Rename to `studentName`.

## Missing `this.` Reference

**Violates:** tsc-csharp-classes-008
**What happens:** A constructor assigns `_studentName = studentName` without `this.`.
**Why it's wrong:** Without `this.`, the distinction between the field and the local parameter is lost. `this.` is mandatory for private field access.
**Fix:** Use `this.studentName = studentName`.

## Unnamed Positional Literals

**Violates:** tsc-csharp-classes-010
**What happens:** `new Student("Josh", 150)` passes literal values without names.
**Why it's wrong:** Unnamed positional literals make code unreadable and brittle — parameter reordering causes silent bugs.
**Fix:** Use named parameters: `new Student(name: "Josh", score: 150)`.

## Wrong Property Initializer Order

**Violates:** tsc-csharp-classes-011
**What happens:** Properties in an object initializer are listed out of the order they are declared in the class.
**Why it's wrong:** The initializer order must mirror the class declaration order to aid readability and code review.
**Fix:** Reorder the initializer properties to match the class declaration sequence.

## Target-Typed New With Explicit Left-Hand Type

**Violates:** tsc-csharp-classes-009
**What happens:** `Student student = new (...)` uses an explicit type on the left with target-typed new.
**Why it's wrong:** When using `new`, the left-hand side must be `var`. Mixing explicit type with `new(...)` is inconsistent.
**Fix:** Use `var student = new Student(...)`.
