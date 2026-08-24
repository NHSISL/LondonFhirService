---
skill: the-standard-csharp-methods
type: rules
source-section: "1. Methods"
---

# The Standard C# — Methods — Rules

## Naming

**tsc-csharp-methods-001** [ERROR] Method names must contain a verb describing the action performed (e.g., `GetStudents`, not `Students`).
**tsc-csharp-methods-002** [ERROR] Async methods returning `Task` or `ValueTask` must be postfixed with `Async`.
**tsc-csharp-methods-003** [ERROR] Input parameter names must reflect the specific property or action — not generic names like `text`, `name`, or `id` without the entity prefix.
**tsc-csharp-methods-004** [ERROR] Parameters representing an entity's field must include the entity name (e.g., `studentId`, not `id`; `studentName`, not `name`).
**tsc-csharp-methods-005** [ERROR] When calling a method, use named parameter aliases when the passed variable name does not fully match the parameter name. Must not pass unnamed literal values.

## Organization — One-Liners

**tsc-csharp-methods-006** [ERROR] Methods with exactly one line of logic must use fat-arrow (`=>`) syntax — no scoped block.
**tsc-csharp-methods-007** [ERROR] A fat-arrow one-liner exceeding 120 characters must break after `=>` and indent the continuation with one extra tab.

## Organization — Multi-Liners

**tsc-csharp-methods-008** [ERROR] Methods containing multiple lines of logic or chained calls must use a scoped block (`{ }`). Must not use fat-arrow for multi-liner logic.
**tsc-csharp-methods-009** [ERROR] In multi-liner methods, the final `return` statement must be preceded by one blank line.
**tsc-csharp-methods-010** [ERROR] Two consecutive non-return calls under 120 characters may be stacked without a blank line. A call that breaks onto multiple lines must be separated from the next statement by a blank line.

## Declaration Length

**tsc-csharp-methods-011** [ERROR] Method declarations exceeding 120 characters must break the parameter list onto the next line with one parameter per line, indented by one tab.

## Chaining (Uglification)

**tsc-csharp-methods-012** [ERROR] Method chains must use uglification: the first call stays on the invocation line; each subsequent chained call is indented one additional tab beyond the previous.
