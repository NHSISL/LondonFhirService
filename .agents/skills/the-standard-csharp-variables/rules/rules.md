---
skill: the-standard-csharp-variables
type: rules
source-section: "0. Variables"
---

# The Standard C# — Variables — Rules

## Naming

**tsc-csharp-variables-001** [ERROR] Variable names must fully spell out the concept — no abbreviations, single letters, or shortened names (e.g., `student`, not `s` or `stdnt`).
**tsc-csharp-variables-002** [ERROR] Collection variable names must be plural (e.g., `students`) — must not be suffixed with `List`, `Array`, or a type name (e.g., `studentList` is forbidden).
**tsc-csharp-variables-003** [ERROR] Variable names must not include the type as a suffix or prefix (e.g., `studentModel`, `studentObj` are forbidden).
**tsc-csharp-variables-004** [ERROR] Variables holding a null or default value must be prefixed with `no` (e.g., `noStudent`, `noChangeCount`).

## Declarations

**tsc-csharp-variables-005** [ERROR] Use `var` when the right-hand side type is immediately clear from the expression (e.g., `new Student()`, literals).
**tsc-csharp-variables-006** [ERROR] Use the explicit type when the right-hand side is a method call whose return type is not immediately visible (e.g., `Student student = GetStudent()`).
**tsc-csharp-variables-007** [WARN]  Use `var` for anonymous types where no explicit type exists.
**tsc-csharp-variables-008** [ERROR] For single-property instantiation: declare the object, then assign the property on the next line. Must not use an object initializer block for a single property.
**tsc-csharp-variables-009** [ERROR] For two or more properties: use an object initializer block. Must not chain individual property assignments on separate lines.

## Organization

**tsc-csharp-variables-010** [ERROR] Variable declarations exceeding 120 characters must break at the `=` sign, with the right-hand side indented on the next line.
**tsc-csharp-variables-011** [ERROR] Single-line declarations must be stacked with no blank lines between them.
**tsc-csharp-variables-012** [ERROR] Declarations that occupy two or more lines must be surrounded by one blank line above and one blank line below.
