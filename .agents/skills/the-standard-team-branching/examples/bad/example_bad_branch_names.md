---
skill: the-standard-team-branching
type: example
source-section: "4.1.1.2 Branch Name Conventions"
demonstrates: "tst-branching-004, tst-branching-009, tst-branching-012, tst-branching-013"
---

# ❌ Bad Examples — Branch Names

```
fix-bug                          # generic, no username prefix, no category
feature                          # vague, no pattern
wip                              # forbidden generic name
student-add                      # missing users/ prefix
users/jsmith/FOUNDATIONS-Student  # uppercase — must be lowercase
users/jsmith/foundations          # missing entity and action
jsmith/foundations-student-add   # missing users/ prefix
```

## Why These Are Wrong

| Branch | Violation |
|---|---|
| `fix-bug` | tst-branching-004, tst-branching-012 — generic, wrong format |
| `feature` | tst-branching-004, tst-branching-012 — generic, wrong format |
| `wip` | tst-branching-012 — forbidden generic name |
| `student-add` | tst-branching-004 — missing `users/[username]/` prefix |
| `users/jsmith/FOUNDATIONS-Student` | tst-branching-009 — not lowercase |
| `users/jsmith/foundations` | tst-branching-007, tst-branching-008 — missing entity and action |
| `jsmith/foundations-student-add` | tst-branching-004 — missing `users/` prefix |
