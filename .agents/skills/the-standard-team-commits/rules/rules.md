---
skill: the-standard-team-commits
type: rules
source-section: "4.1.3 Commits"
---

# The Standard Team — Commits — Rules

## TDD Commit Format

**tst-commits-001** [ERROR] When committing a failing test (TDD category), the message must be: `[Test Name] -> FAIL`.
**tst-commits-002** [ERROR] When committing the passing implementation (TDD category), the message must be: `[Test Name] -> PASS`.
**tst-commits-003** [ERROR] TDD commits must not use the `[CATEGORY]: [Description]` format.

## Non-TDD Commit Format

**tst-commits-004** [ERROR] Non-TDD commits must use the format: `[CATEGORY]: [Description Of Work Completed]`.
**tst-commits-005** [ERROR] `[CATEGORY]` must be in CAPS and taken from the approved non-TDD category list.
**tst-commits-006** [ERROR] `[Description Of Work Completed]` must be in Pascal Case.
**tst-commits-007** [ERROR] Category must not be lowercase or mixed-case.
**tst-commits-008** [ERROR] Description must not be vague — `fix`, `update`, `changes`, `wip` without specific context are forbidden.

## Category Classification

**tst-commits-009** [ERROR] TDD categories (requiring FAIL/PASS format): FOUNDATIONS, PROCESSINGS, ORCHESTRATIONS, COORDINATIONS, MANAGEMENTS, AGGREGATIONS, VIEWS, COMPONENTS, PAGES, ACCEPTANCE, INTEGRATION.
**tst-commits-010** [ERROR] Non-TDD categories (requiring CATEGORY: Description format): DATA, BROKERS, CONTROLLERS, INFRA, CONFIG, DOCUMENTATION, DESIGN, IMPORT, STATUS, PROVISION, RELEASE.
