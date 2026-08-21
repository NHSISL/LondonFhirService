---
skill: the-standard-team-projects
type: example
source-section: "4.1.2 Projects"
demonstrates: "tst-projects-001, tst-projects-002, tst-projects-003, tst-projects-004, tst-projects-005, tst-projects-006, tst-projects-007"
---

# ✅ Good Example — Standard Solution Structure

```
Taarafo.Core                              (API)
Taarafo.Core.Infrastructure.Build         (Console App)
Taarafo.Core.Infrastructure.Provision     (Console App)
Taarafo.Core.Tests.Acceptance             (xUnit Test Project)
Taarafo.Core.Tests.Build                  (xUnit Test Project)
```

## Source Project Folder Structure

```
Taarafo.Core/
  Brokers/
    DateTimes/
    Loggings/
    Storages/
  Models/
    Foundations/
      Posts/
        Exceptions/
      Comments/
        Exceptions/
  Migrations/
  Services/
    Foundations/
      Posts/
      Comments/
    Processings/
      Posts/
    Orchestrations/
      Posts/
  Controllers/
```

## Test Project Folder Structure

```
Taarafo.Core.Tests.Build/
  Services/
    Foundations/
      Posts/
      Comments/
    Processings/
      Posts/
    Orchestrations/
      Posts/
```
