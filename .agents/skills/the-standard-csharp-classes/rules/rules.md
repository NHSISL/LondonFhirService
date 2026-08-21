---
skill: the-standard-csharp-classes
type: rules
source-section: "4. Classes"
---

# The Standard C# — Classes — Rules

## Class Naming

**tsc-csharp-classes-001** [ERROR] Model class names must match the entity name exactly — no suffix (e.g., `Student`, not `StudentModel`).
**tsc-csharp-classes-002** [ERROR] Service class names must be singular: `{Entity}Service` (e.g., `StudentService`, not `StudentsService`).
**tsc-csharp-classes-003** [ERROR] Service class names must not use alternative suffixes such as `BusinessLogic` or `BL`.
**tsc-csharp-classes-004** [ERROR] Broker class names must be singular: `{Entity}Broker` (e.g., `StudentBroker`, not `StudentsBroker`).
**tsc-csharp-classes-005** [ERROR] Controller class names must be plural: `{Entities}Controller` (e.g., `StudentsController`, not `StudentController`).

## Field Naming

**tsc-csharp-classes-006** [ERROR] Class fields must use camelCase (e.g., `studentName`, not `StudentName` or `_studentName`).
**tsc-csharp-classes-007** [ERROR] Class fields must not be prefixed with an underscore.

## Field Referencing

**tsc-csharp-classes-008** [ERROR] Private class fields must be referenced using `this.` to distinguish them from local variables (e.g., `this.studentName = studentName`).

## Instantiation

**tsc-csharp-classes-009** [ERROR] Use `var` on the left side of object instantiation; must not use `Student student = new (...)` target-typed new with an explicit type.
**tsc-csharp-classes-010** [ERROR] Must not pass positional unnamed literal values to constructors — use named parameters or matching variable names.
**tsc-csharp-classes-011** [ERROR] Property assignment in object initializers must match the order in which properties are declared in the class.
**tsc-csharp-classes-012** [ERROR] Constructor parameter order must match the order in which fields are declared in the class.
