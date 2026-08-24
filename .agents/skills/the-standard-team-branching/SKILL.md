---
name: the-standard-team-branching
version: 0.1.0
standard-team-version: v0.1.0
applies-to: ["*", ".git/*"]
depends-on: []
---

# The Standard Team — Branching

## 0/ Context (Purpose — WHY this skill exists)

0.0/ Where: Any Standard Team-compliant repository — branch naming and forking workflow.
0.1/ Who: Engineers creating branches, forks, or pull requests on any project.
0.2/ What: Enforces forking strategy, branch naming conventions, and the category-based branch naming format.
0.3/ Applies to: All branches in all repositories.
0.4/ Version: v0.1.0
0.5/ Depends on: none

## 1/ Actual (Dependency — WHAT the rules are and what they depend on)

1.0/ Dos:
  1. Fork the target repository before contributing — never push feature branches to upstream → see rules/rules.md#tst-branching-001
  2. Name branches using the pattern `users/[username]/[category]-[entity]-[action]` → see rules/rules.md#tst-branching-002
  3. Use lowercase for the category, entity, and action segments of the branch name → see rules/rules.md#tst-branching-003
  4. Choose a category from the approved category list → see contracts/contracts.json
  5. Keep branches short-lived and focused on a single concern → see rules/rules.md#tst-branching-005
  6. Delete remote branches after merging → see rules/rules.md#tst-branching-006

1.1/ Don'ts:
  1. Must not commit directly to `main` → see validations/anti-patterns.md#direct-commit-to-main
  2. Must not use generic branch names like `fix`, `test`, `temp`, or `wip` → see validations/anti-patterns.md#generic-branch-names
  3. Must not reuse old branch names for new work → see validations/anti-patterns.md#reused-branch-names
  4. Must not push feature branches to the upstream repository — only to the fork → see validations/anti-patterns.md#push-to-upstream

1.2/ Ask:
  - Ask when the correct category for a branch is ambiguous.
  - Ask when the entity or action is unclear from the work description.

1.3/ Defaults:
  - Default branch pattern: `users/{github-handle}/{category}-{entity}-{action}`
  - Default contribution flow: fork → clone → branch → commit → PR to upstream.
  - Categories are taken from the approved list in contracts/contracts.json.

1.4/ Examples:
  - ✅ `users/jsmith/foundations-student-add`
  - ✅ `users/jsmith/brokers-student-storage`
  - ❌ `fix-bug`
  - ❌ `feature`
  - ❌ see examples/bad/example_bad_branch_names.md

1.5/ Templates:
  - Branch naming guide: see templates/

1.6/ Checklists:
  - Pre-review checklist: see validations/checklist.md

1.7/ Contracts:
  - Branch naming contracts and category list: see contracts/contracts.json

## 2/ Expected (Exposure — WHAT comes out)

2.0/ Format: Branch name string following `users/{username}/{category}-{entity}-{action}`.
2.1/ Outcome: All branches follow a consistent, reviewable naming convention derived from the approved category list.
2.2/ Tone: Direct. Cite rule IDs. Non-compliant branch names must be corrected.
