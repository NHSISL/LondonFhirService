---
name: the-standard-team-commits
version: 0.1.0
standard-team-version: v0.1.0
applies-to: ["*", ".git/*"]
depends-on: ["the-standard-team-branching"]
---

# The Standard Team — Commits

## 0/ Context (Purpose — WHY this skill exists)

0.0/ Where: Any Standard Team-compliant repository — commit message conventions.
0.1/ Who: Engineers writing commit messages for any code change.
0.2/ What: Enforces TDD commit format (FAIL/PASS) for TDD categories and category-description format for non-TDD categories.
0.3/ Applies to: All commit messages.
0.4/ Version: v0.1.0
0.5/ Depends on: the-standard-team-branching

## 1/ Actual (Dependency — WHAT the rules are and what they depend on)

1.0/ Dos:
  1. For TDD categories: commit a failing test as `[Test Name] -> FAIL` → see rules/rules.md#tst-commits-001
  2. For TDD categories: commit the passing implementation as `[Test Name] -> PASS` → see rules/rules.md#tst-commits-002
  3. For non-TDD categories: format as `[CATEGORY]: [Description Of Work Completed]` → see rules/rules.md#tst-commits-003
  4. `[CATEGORY]` must always be in CAPS and taken from the approved category list → see rules/rules.md#tst-commits-004
  5. `[Description Of Work Completed]` must be in Pascal Case → see rules/rules.md#tst-commits-005

1.1/ Don'ts:
  1. Must not use `[CATEGORY]: [Description]` format for TDD commits → see validations/anti-patterns.md#wrong-format-for-tdd
  2. Must not use lowercase or mixed-case categories → see validations/anti-patterns.md#lowercase-category
  3. Must not use vague descriptions: `fix`, `update`, `changes`, `wip` without specifics → see validations/anti-patterns.md#vague-description

1.2/ Ask:
  - Ask when the category for a commit is ambiguous.
  - Ask when TDD vs non-TDD distinction for the work type is unclear.

1.3/ Defaults:
  - TDD categories: FOUNDATIONS, PROCESSINGS, ORCHESTRATIONS, COORDINATIONS, MANAGEMENTS, AGGREGATIONS, VIEWS, COMPONENTS, PAGES, ACCEPTANCE, INTEGRATION.
  - Non-TDD categories: DATA, BROKERS, CONTROLLERS, INFRA, CONFIG, DOCUMENTATION, DESIGN, IMPORT, STATUS, PROVISION, RELEASE.
  - When a test is failing: `[Test Name] -> FAIL`.
  - When a test is passing: `[Test Name] -> PASS`.

1.4/ Examples:
  - ✅ `ShouldAddStudentAsync -> FAIL`
  - ✅ `ShouldAddStudentAsync -> PASS`
  - ✅ `BROKERS: Insert Student`
  - ✅ `DATA: Add Student Model`
  - ❌ `FOUNDATIONS: Add Student` (TDD category — must use FAIL/PASS format)
  - ❌ `foundations: Insert Student` (lowercase category)
  - ❌ see examples/bad/example_bad_commit_messages.md

1.5/ Templates:
  - Commit message templates: see templates/

1.6/ Checklists:
  - Pre-review checklist: see validations/checklist.md

1.7/ Contracts:
  - Commit format contracts and category list: see contracts/contracts.json

## 2/ Expected (Exposure — WHAT comes out)

2.0/ Format: Commit message string in either `[Test Name] -> FAIL/PASS` or `[CATEGORY]: [Pascal Case Description]` format.
2.1/ Outcome: All commits follow a consistent, reviewable message format that maps to the TDD workflow or category-based non-TDD work.
2.2/ Tone: Direct. Cite rule IDs. Non-compliant commit messages must be corrected.
