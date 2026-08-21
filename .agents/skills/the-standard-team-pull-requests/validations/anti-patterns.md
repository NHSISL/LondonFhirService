---
skill: the-standard-team-pull-requests
type: anti-patterns
source-section: "4.1.4 Pull Requests"
---

# The Standard Team — Pull Requests — Anti-Patterns

## Wrong Case Category

**Violates:** tst-pull-requests-008
**What happens:** The PR title uses a lowercase or mixed-case category, e.g., `foundations: Add Student` or `Foundations: Add Student`.
**Why it's wrong:** Categories must always be in CAPS. Inconsistent casing breaks PR filtering, tooling integrations, and review workflows.
**Fix:** Uppercase the entire category: `FOUNDATIONS: Add Student`.

## Vague PR Title

**Violates:** tst-pull-requests-009
**What happens:** The PR title description is generic — `fix`, `update`, `changes`, or `wip` — without identifying what was changed.
**Why it's wrong:** PR titles must communicate exactly what work was done. Vague titles make code review, changelog generation, and traceability impossible.
**Fix:** Use a specific entity and action: `BROKERS: Insert Student` or `FOUNDATIONS: Add Student Validation`.

## Invalid Category

**Violates:** tst-pull-requests-010
**What happens:** The PR title uses a category that does not appear in the approved category list, e.g., `HELPERS: Add Utility` or `MISC: Various Changes`.
**Why it's wrong:** Categories must come from the approved list. Unapproved categories cannot be consistently interpreted by reviewers or tooling.
**Fix:** Map the work to the closest approved category. If uncertain, ask before creating the PR.

## Missing Issue Link

**Violates:** tst-pull-requests-005
**What happens:** The PR fixes or resolves a tracked issue but no `Closes #[issue-number]` appears in the description.
**Why it's wrong:** Without the closing keyword, the issue remains open after merging and requires manual closure, creating backlog noise.
**Fix:** Add `Closes #[issue-number]` to the PR description. For multiple issues: `Closes #10, closes #123`.
