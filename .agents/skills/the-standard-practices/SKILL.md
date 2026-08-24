---
name: the-standard-practices
version: 0.1.0
standard-version: v2.50.0
applies-to: ["*"]
depends-on: ["the-standard-core"]
---

# The Standard — Practices

## 0/ Context (Purpose — WHY this skill exists)

0.0/ Where: All repositories following The Standard — source control, CI/CD, and configuration.
0.1/ Who: Engineers contributing to any Standard-compliant project.
0.2/ What: Enforces branching, commit, pull request, and configuration practices.
0.3/ Applies to: * (all files)
0.4/ Version: v2.50.0
0.5/ Depends on: the-standard-core

## 1/ Actual (Dependency — WHAT the rules are and what they depend on)

1.0/ Dos:
  1. Fork the target repository before contributing; never push feature branches to upstream → see rules/rules.md#ts-practices-001
  2. Name branches using the pattern `users/{handle}/{type}/{short-description}` → see rules/rules.md#ts-practices-002
  3. Write commit messages in the imperative mood with a category prefix in ALL CAPS → see rules/rules.md#ts-practices-003
  4. Keep pull requests focused on a single concern — one PR per feature/fix → see rules/rules.md#ts-practices-004
  5. Include a passing build and all tests green before requesting a PR review → see rules/rules.md#ts-practices-005
  6. Store all secrets and environment-specific values in environment variables or secret stores, never in source code → see rules/rules.md#ts-practices-006
  7. Use ADotNet or an equivalent YAML-based CI pipeline configuration → see rules/rules.md#ts-practices-007

1.1/ Don'ts:
  1. Must not commit secrets, connection strings, or API keys to source control → see validations/anti-patterns.md#secrets-in-source
  2. Must not merge a PR with failing tests or a failing build → see validations/anti-patterns.md#failing-pr
  3. Must not use vague commit messages such as "fix", "update", or "wip" → see validations/anti-patterns.md#vague-commits
  4. Must not submit PRs that mix multiple unrelated concerns → see validations/anti-patterns.md#mixed-pr

1.2/ Ask:
  - Ask when a change touches more than one domain — it likely needs separate PRs.
  - Ask when secrets management approach is unclear.

1.3/ Defaults:
  - Default branch name format: `users/{github-handle}/{type}/{short-description}`
  - Default commit format: `{CATEGORY}: {imperative sentence}.`
  - Default PR target: `main` or the project's primary integration branch.

1.4/ Examples:
  - ✅ see examples/good/example_good_branch_and_commit.md
  - ❌ see examples/bad/example_bad_branch_and_commit.md

1.5/ Templates:
  - GitHub Actions CI workflow: see templates/github-actions-workflow.yml
  - Pull request template: see templates/pull-request-template.md
  - Branch naming guide: see templates/branch_naming_guide.md

1.6/ Checklists:
  - Pre-review checklist: see validations/checklist.md

1.7/ Contracts:
  - Naming and CI contracts: see contracts/contracts.json

## 2/ Expected (Exposure — WHAT comes out)

2.0/ Format: Markdown, YAML, branch names, and commit messages.
2.1/ Outcome: Consistent, traceable, reviewable contributions that follow Standard practices.
2.2/ Tone: Direct. Cite rule IDs. Violations must be fixed, not suggested.
