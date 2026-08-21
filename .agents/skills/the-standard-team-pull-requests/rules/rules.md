---
skill: the-standard-team-pull-requests
type: rules
source-section: "4.1.4 Pull Requests"
---

# The Standard Team — Pull Requests — Rules

## PR Title Format

**tst-pull-requests-001** [ERROR] PR title must follow the format: `[CATEGORY]: [Description Of Work Completed]`.
**tst-pull-requests-002** [ERROR] `[CATEGORY]` must be in CAPS and taken from the approved category list.
**tst-pull-requests-003** [ERROR] `[Description Of Work Completed]` must be in Pascal Case.
**tst-pull-requests-008** [ERROR] PR title category must not be lowercase or mixed-case — `foundations:` and `Foundations:` are both wrong.
**tst-pull-requests-009** [ERROR] PR title description must not be vague — `fix`, `update`, `changes`, `wip` without specifics are forbidden.
**tst-pull-requests-010** [ERROR] PR title must not use a category not in the approved category list.

## Issue Linking

**tst-pull-requests-005** [WARN]  To auto-close an issue on merge, add `Closes #[issue-number]` anywhere in the PR description.
**tst-pull-requests-006** [WARN]  To close multiple issues, list each separately: `Closes #10, closes #123`.
**tst-pull-requests-007** [WARN]  To link an issue without auto-closing, use `#[issue-number]` without a keyword.

## Approved Categories

**tst-pull-requests-004** [ERROR] Approved categories are: FOUNDATIONS, PROCESSINGS, ORCHESTRATIONS, COORDINATIONS, MANAGEMENTS, AGGREGATIONS, VIEWS, COMPONENTS, PAGES, ACCEPTANCE, INTEGRATION, DATA, BROKERS, CONTROLLERS, INFRA, CONFIG, DOCUMENTATION, DESIGN, IMPORT, STATUS, PROVISION, RELEASE.
