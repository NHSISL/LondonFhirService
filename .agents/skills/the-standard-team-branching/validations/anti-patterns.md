---
skill: the-standard-team-branching
type: anti-patterns
source-section: "4.1.1 Forking and Branching Strategies"
---

# The Standard Team — Branching — Anti-Patterns

## Direct Commit to Main

**Violates:** tst-branching-011
**What happens:** A developer commits directly to the `main` branch without creating a feature branch.
**Why it's wrong:** `main` is the integration branch. Direct commits bypass code review, break the contribution workflow, and risk destabilising the codebase.
**Fix:** Create a branch following `users/[username]/[category]-[entity]-[action]`, commit there, and open a pull request.

## Generic Branch Names

**Violates:** tst-branching-012
**What happens:** A branch is named `fix`, `test`, `wip`, `feature`, or another vague term.
**Why it's wrong:** Generic names provide no information about what the branch contains. They cannot be reviewed, tracked, or correlated to a commit or PR.
**Fix:** Use the full pattern: `users/[username]/[category]-[entity]-[action]`, e.g., `users/jsmith/foundations-student-add`.

## Push to Upstream

**Violates:** tst-branching-003
**What happens:** A contributor pushes a feature branch directly to the upstream/official repository instead of their fork.
**Why it's wrong:** Contributors must not have write access to the official repository. Pushing to upstream bypasses the fork-and-PR workflow and pollutes the official repository with in-progress branches.
**Fix:** Push the branch to the personal fork and open a pull request from there.

## Reused Branch Names

**Violates:** tst-branching-013
**What happens:** A contributor reuses an old merged or deleted branch name for new work.
**Why it's wrong:** Reusing branch names creates confusion in history, PRs, and CI systems. Each piece of work deserves a distinct, traceable branch name.
**Fix:** Create a new branch with a new name that reflects the new work being done.

## Wrong Case in Branch Name

**Violates:** tst-branching-009
**What happens:** A branch is named with uppercase letters, e.g., `users/jsmith/FOUNDATIONS-Student-Add`.
**Why it's wrong:** The branch naming convention requires all segments to be lowercase and hyphen-separated. Mixed case creates inconsistency and potential case-sensitivity issues across tools.
**Fix:** Rename to lowercase: `users/jsmith/foundations-student-add`.
