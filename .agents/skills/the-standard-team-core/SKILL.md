---
name: the-standard-team-core
version: 0.1.0
standard-team-version: v0.1.0
applies-to: ["*.md", "**/*.md"]
depends-on: []
---

# The Standard Team — Core

## 0/ Context (Purpose — WHY this skill exists)

0.0/ Where: Any Standard Team-compliant project or team — project documentation and purpose-defining artifacts.
0.1/ Who: Engineers, team leads, and architects defining or reviewing team purpose, goals, and scenarios.
0.2/ What: Enforces the purposing model — business nature, business goals, future considerations, and scenario definitions.
0.3/ Applies to: *.md, project documentation files.
0.4/ Version: v0.1.0
0.5/ Depends on: none

## 1/ Actual (Dependency — WHAT the rules are and what they depend on)

1.0/ Dos:
  1. Every team and project must define its business nature → see rules/rules.md#tst-core-001
  2. Every team and project must define its business goals → see rules/rules.md#tst-core-002
  3. Every team and project must define future considerations → see rules/rules.md#tst-core-003
  4. Scenarios must be written with actors (Given), interactions (When), and outcomes (Then) → see rules/rules.md#tst-core-004
  5. Scenarios must remain non-technical unless the business itself is technical → see rules/rules.md#tst-core-005
  6. Scenario outcomes must be expressible as advertising copy for end users → see rules/rules.md#tst-core-006
  7. Scenarios may chain — each building on the outcome of a prior scenario → see rules/rules.md#tst-core-007

1.1/ Don'ts:
  1. Must not skip defining business nature, goals, or future considerations → see validations/anti-patterns.md#missing-purpose-elements
  2. Must not write scenarios with technical implementation details as the primary language → see validations/anti-patterns.md#technical-scenarios
  3. Must not conflate business scenarios with engineering tasks or stories → see validations/anti-patterns.md#scenario-as-task

1.2/ Ask:
  - Ask when the business nature of the system is ambiguous before proceeding.
  - Ask when a scenario actor is not identifiable from the documentation.
  - Ask when outcomes appear to be implementation steps rather than user-visible results.

1.3/ Defaults:
  - When no scenarios exist, request scenario documentation before generating technical work.
  - Default scenario format: GIVEN / WHEN / THEN.
  - Default actors: end users of the system (not developers or system components).

1.4/ Examples:
  - ✅ see examples/good/example_good_purpose_document.md
  - ❌ see examples/bad/example_bad_purpose_document.md

1.5/ Templates:
  - Scaffold a purpose document: see templates/
  - Scaffold a scenario: see templates/

1.6/ Checklists:
  - Pre-review checklist: see validations/checklist.md

1.7/ Contracts:
  - Formal contracts: see contracts/contracts.json

## 2/ Expected (Exposure — WHAT comes out)

2.0/ Format: Markdown documentation files describing team purpose, business goals, and scenarios.
2.1/ Outcome: Every project has a documented purpose with nature, goals, future considerations, and user-level scenarios that engineers can act on.
2.2/ Tone: Direct. Cite rule IDs. Non-compliance must be corrected, not suggested.
