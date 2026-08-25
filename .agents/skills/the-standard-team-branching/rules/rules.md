---
skill: the-standard-team-branching
type: rules
source-section: "4.1.1 Forking and Branching Strategies, 4.1.1.2 Branch Name Conventions"
---

# The Standard Team — Branching — Rules

## Forking and Contribution Flow

**tst-branching-001** [ERROR] Contributors must fork the target repository before contributing — never push feature branches directly to upstream.
**tst-branching-002** [ERROR] After forking, clone the personal fork to a local machine before creating branches.
**tst-branching-003** [ERROR] Branches must be pushed to the personal fork — never to the official/upstream repository.

## Branch Naming

**tst-branching-004** [ERROR] Branch names must follow the pattern: `users/[username]/[category]-[entity]-[action]`.
**tst-branching-005** [ERROR] The `[username]` segment must be the contributor's GitHub username.
**tst-branching-006** [ERROR] The `[category]` segment must be taken from the approved category list (see contracts/contracts.json).
**tst-branching-007** [ERROR] The `[entity]` segment must identify the domain entity the work concerns.
**tst-branching-008** [ERROR] The `[action]` segment must describe the action being performed.
**tst-branching-009** [ERROR] All segments must be lowercase and hyphen-separated.

## Branch Lifecycle

**tst-branching-010** [ERROR] Each branch must be focused on a single concern.
**tst-branching-011** [ERROR] Must not commit directly to `main`.
**tst-branching-012** [ERROR] Must not use generic branch names such as `fix`, `test`, `temp`, or `wip`.
**tst-branching-013** [ERROR] Must not reuse old branch names for new work.
**tst-branching-014** [WARN]  Remote branches should be deleted after merging.
