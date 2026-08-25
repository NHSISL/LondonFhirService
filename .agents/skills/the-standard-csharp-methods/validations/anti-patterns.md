---
skill: the-standard-csharp-methods
type: anti-patterns
source-section: "1. Methods"
---

# The Standard C# — Methods — Anti-Patterns

## No Verb in Method Name

**Violates:** tsc-csharp-methods-001
**What happens:** A method is named `Students()` or `Report()` without a verb.
**Why it's wrong:** Method names must express an action. A noun-only name does not communicate what the method does.
**Fix:** Add a verb: `GetStudents()`, `GenerateReport()`.

## Missing Async Suffix

**Violates:** tsc-csharp-methods-002
**What happens:** An async method returning `ValueTask<Student>` is named `GetStudent()` without `Async`.
**Why it's wrong:** The `Async` suffix signals to callers that the method is asynchronous and must be awaited.
**Fix:** Rename to `GetStudentAsync()`.

## Generic Parameter Name

**Violates:** tsc-csharp-methods-003, tsc-csharp-methods-004
**What happens:** A parameter is named `id`, `name`, or `text` instead of `studentId`, `studentName`.
**Why it's wrong:** Generic names hide what the parameter represents and make the API ambiguous when multiple similar parameters exist.
**Fix:** Prefix with the entity name: `studentId`, `studentName`.

## Unnamed Literal Arguments

**Violates:** tsc-csharp-methods-005
**What happens:** `GetStudentByNameAsync("Todd")` passes a literal without a named alias.
**Why it's wrong:** Unnamed literals obscure intent and break when parameter order changes.
**Fix:** Use named alias: `GetStudentByNameAsync(studentName: "Todd")`.

## Fat-Arrow for Multi-Liner

**Violates:** tsc-csharp-methods-008
**What happens:** A method with two or more statements uses fat-arrow: `public Student Add(Student student) => Validate(student).Insert(student)`.
**Why it's wrong:** Fat-arrow is only permitted for single-line logic. Multi-liner logic must use a scoped block.
**Fix:** Replace with a `{ }` scoped block.

## No Blank Line Before Return

**Violates:** tsc-csharp-methods-009
**What happens:** The `return` statement immediately follows the previous line with no blank line.
**Why it's wrong:** The blank line before `return` visually separates preparation logic from the result, improving readability.
**Fix:** Insert one blank line before the `return`.

## No Uglification in Chain

**Violates:** tsc-csharp-methods-012
**What happens:** A method chain starts on a new line with all calls at the same indentation level.
**Why it's wrong:** Flat chain indentation makes it hard to distinguish chained calls from new statements. Uglification forces chains to become visually unappealing as they grow, naturally driving decomposition.
**Fix:** Apply uglification: first call on the invocation line, each subsequent call indented one extra tab.
