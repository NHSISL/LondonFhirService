---
skill: the-standard-csharp-variables
type: anti-patterns
source-section: "0. Variables"
---

# The Standard C# — Variables — Anti-Patterns

## Abbreviated Names

**Violates:** tsc-csharp-variables-001
**What happens:** A variable is named `s`, `stdnt`, or `st` instead of `student`.
**Why it's wrong:** Abbreviated names reduce readability and force the reader to decode meaning. All names must be fully spelled out.
**Fix:** Use the full concept name: `student`.

## Collection Type Suffix

**Violates:** tsc-csharp-variables-002
**What happens:** A collection is named `studentList` or `studentArray` instead of `students`.
**Why it's wrong:** The type is already communicated by the declaration — the name must only express the plural concept, not duplicate type information.
**Fix:** Rename to the plural: `students`.

## Type Suffix in Name

**Violates:** tsc-csharp-variables-003
**What happens:** A variable is named `studentModel` or `studentObj`.
**Why it's wrong:** Variable names must not repeat the type. The name should express what the value represents, not how it is typed.
**Fix:** Rename to `student`.

## Missing No Prefix for Null/Default

**Violates:** tsc-csharp-variables-004
**What happens:** `Student student = null;` names the variable as if it holds a real student.
**Why it's wrong:** A variable set to null or a default value must communicate that fact through its name. Using a regular noun implies the variable holds a valid value.
**Fix:** Rename to `noStudent`.

## Explicit Type When Clear

**Violates:** tsc-csharp-variables-005
**What happens:** `Student student = new Student();` uses an explicit type when the right-hand side already makes the type obvious.
**Why it's wrong:** When the right-hand side unambiguously shows the type (`new Student()`), `var` must be used to avoid redundancy.
**Fix:** `var student = new Student();`

## Var When Semi-Clear

**Violates:** tsc-csharp-variables-006
**What happens:** `var student = GetStudent();` — the reader cannot determine the type without reading the method.
**Why it's wrong:** When the type is not immediately apparent from the right-hand expression, the explicit type must be declared to aid readability.
**Fix:** `Student student = GetStudent();`

## Object Initializer for Single Property

**Violates:** tsc-csharp-variables-008
**What happens:** A single-property type is instantiated with an initializer block `new StudentEvent { Student = x }`.
**Why it's wrong:** Single-property initializations must use the declare-then-assign style for clarity and consistency.
**Fix:** `var inputStudentEvent = new StudentEvent(); inputStudentEvent.Student = x;`

## Missing Blank Lines Around Multi-Line Declaration

**Violates:** tsc-csharp-variables-012
**What happens:** A declaration spanning two lines is immediately followed by the next declaration with no blank line.
**Why it's wrong:** Multi-line declarations must be visually isolated with blank lines above and below to separate them from adjacent single-line declarations.
**Fix:** Add one blank line before and after the multi-line declaration.
