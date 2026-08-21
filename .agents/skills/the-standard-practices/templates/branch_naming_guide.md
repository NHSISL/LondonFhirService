# Branch Naming Guide

## Pattern

`users/{github-handle}/{type}/{short-description}`

All parts must be lowercase kebab-case.

## Type values

| Type | When to use |
|---|---|
| `feature` | New functionality |
| `bugfix` | Fixing a defect |
| `refactor` | Code restructuring without behavior change |
| `docs` | Documentation only |
| `config` | CI, build, or configuration changes |
| `release` | Release preparation |

## Examples

| ✅ Correct | ❌ Incorrect |
|---|---|
| `users/jsmith/feature/add-student-broker` | `feature/add-student-broker` |
| `users/jdoe/bugfix/fix-null-enrollment` | `users/jdoe/Fix-Null-Enrollment` |
| `users/aali/docs/update-readme` | `wip` |
| `users/mchen/config/add-ci-pipeline` | `temp-fix` |

## Rules

- Must start with `users/{github-handle}/`
- Must use lowercase kebab-case throughout
- Must be descriptive — no `wip`, `temp`, `fix`, `test`
- Must be short-lived — delete after merging
- Must never be pushed directly to `main`
