---
name: the-standard-team-pull-requests
version: 0.1.0
standard-team-version: v0.1.0
applies-to: ["*", ".git/*"]
depends-on: ["the-standard-team-commits", "the-standard-team-branching"]
---

# The Standard Team — Pull Requests

## 0/ Context (Purpose — WHY this skill exists)

0.0/ Where: Any Standard Team-compliant repository — pull request titles, descriptions, and linking.
0.1/ Who: Engineers creating or reviewing pull requests on any project.
0.2/ What: Enforces PR title format, category rules, description conventions, and issue linking syntax.
0.3/ Applies to: All pull requests.
0.4/ Version: v0.1.0
0.5/ Depends on: the-standard-team-commits, the-standard-team-branching

## 1/ Actual (Dependency — WHAT the rules are and what they depend on)

1.0/ Dos:
  1. PR title must follow the format: `[CATEGORY]: [Description Of Work Completed]` → see rules/rules.md#tst-pull-requests-001
  2. `[CATEGORY]` must be in CAPS and taken from the approved category list → see rules/rules.md#tst-pull-requests-002
  3. `[Description Of Work Completed]` must be in Pascal Case → see rules/rules.md#tst-pull-requests-003
  4. To auto-close an issue on merge, add `Closes #[issue-number]` in the PR description → see rules/rules.md#tst-pull-requests-005
  5. To close multiple issues: `Closes #10, closes #123` → see rules/rules.md#tst-pull-requests-006
  6. To link without auto-closing, use `#[issue-number]` without a keyword → see rules/rules.md#tst-pull-requests-007

1.1/ Don'ts:
  1. Must not use lowercase or mixed-case categories — `foundations:` and `Foundations:` are both wrong → see validations/anti-patterns.md#wrong-case-category
  2. Must not use vague PR title descriptions: `fix`, `update`, `changes`, `wip` without specifics → see validations/anti-patterns.md#vague-pr-title
  3. Must not use a category not in the approved category list → see validations/anti-patterns.md#invalid-category

1.2/ Ask:
  - Ask when the correct category for the PR is ambiguous.
  - Ask when the work spans multiple categories — determine the primary one.

1.3/ Defaults:
  - Approved categories: FOUNDATIONS, PROCESSINGS, ORCHESTRATIONS, COORDINATIONS, MANAGEMENTS, AGGREGATIONS, VIEWS, COMPONENTS, PAGES, ACCEPTANCE, INTEGRATION, DATA, BROKERS, CONTROLLERS, INFRA, CONFIG, DOCUMENTATION, DESIGN, IMPORT, STATUS, PROVISION, RELEASE.
  - Default auto-close keyword: `Closes #[issue-number]`.

1.4/ Examples:
  - ✅ `FOUNDATIONS: Add Student`
  - ✅ `BROKERS: Insert Student`
  - ✅ `CONTROLLERS: POST Student`
  - ❌ `foundations: Add Student`
  - ❌ `Foundations: Add Student`
  - ❌ `fix`
  - ❌ see examples/bad/example_bad_pull_requests.md

1.5/ Templates:
  - PR title and description templates: see templates/

1.6/ Checklists:
  - Pre-review checklist: see validations/checklist.md

1.7/ Contracts:
  - PR format contracts and category list: see contracts/contracts.json

## 2/ Expected (Exposure — WHAT comes out)

2.0/ Format: PR title in `[CATEGORY]: [Pascal Case Description]` format; description with optional issue links.
2.1/ Outcome: All PRs follow a consistent, reviewable title format with correct category, casing, and issue linkage.
2.2/ Tone: Direct. Cite rule IDs. Non-compliant PR titles must be corrected before merge.
