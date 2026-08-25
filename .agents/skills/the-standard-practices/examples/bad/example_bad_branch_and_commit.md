# The Standard — Practices — Bad Examples

## ❌ Branch Naming Violations

```
feature/student          ← missing users/{handle}/ prefix
users/jsmith/AddStudent  ← PascalCase not kebab-case
fix-stuff                ← no user prefix, vague name
```

## ❌ Commit Message Violations

```
fix                      ← vague, no category
update broker            ← lowercase category, vague description
WIP: student stuff       ← WIP commit, vague
Added student service    ← not imperative mood, no category
```

## ❌ Pull Request Title Violations

```
fix student bug          ← no category, not Pascal Case
Foundations: add stuff   ← category not ALL CAPS, description vague
update                   ← no category, vague
```

## ❌ Secrets in Source

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=prod-sql;Database=School;User Id=admin;Password=SuperSecret123!"
  }
}
```
This connection string committed to `appsettings.json` exposes credentials permanently in git history.
