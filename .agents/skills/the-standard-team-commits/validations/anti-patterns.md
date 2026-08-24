---
skill: the-standard-team-commits
type: anti-patterns
source-section: "4.1.3 Commits"
---

# The Standard Team — Commits — Anti-Patterns

## Wrong Format for TDD

**Violates:** tst-commits-003, tst-commits-009
**What happens:** A TDD category commit (e.g., FOUNDATIONS) uses the `[CATEGORY]: [Description]` format instead of `[Test Name] -> FAIL/PASS`.
**Why it's wrong:** TDD categories follow the red-green cycle. The commit message must reflect the test outcome, not a category description.
**Fix:** Use `ShouldAddStudentAsync -> FAIL` for the failing test and `ShouldAddStudentAsync -> PASS` for the passing implementation.

## Lowercase Category

**Violates:** tst-commits-007
**What happens:** The category is written in lowercase or mixed-case, e.g., `foundations: Add Student` or `Foundations: Add Student`.
**Why it's wrong:** Categories must always be in CAPS. Inconsistent casing breaks commit parsing, filtering, and review tooling.
**Fix:** Uppercase the category: `FOUNDATIONS` or use the correct FAIL/PASS format for TDD categories.

## Vague Description

**Violates:** tst-commits-008
**What happens:** The commit description is generic — `fix`, `update`, `changes`, or `wip`.
**Why it's wrong:** Vague commit messages provide no traceability. Reviewers cannot determine what was changed or why from the commit history.
**Fix:** Be specific. Use the name of the entity and action: `BROKERS: Insert Student` or `DATA: Add Post Model`.

## Non-Pascal Case Description

**Violates:** tst-commits-006
**What happens:** The description uses all lowercase or sentence case: `BROKERS: insert student`.
**Why it's wrong:** The convention requires Pascal Case for all words in the description.
**Fix:** Capitalise each word: `BROKERS: Insert Student`.
