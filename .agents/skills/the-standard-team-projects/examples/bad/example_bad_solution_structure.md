---
skill: the-standard-team-projects
type: example
source-section: "4.1.2 Projects"
demonstrates: "tst-projects-001, tst-projects-005, tst-projects-006, tst-projects-009, tst-projects-010"
---

# ❌ Bad Example — Non-Standard Solution Structure

## Missing Infrastructure Projects

```
Taarafo.Core            (API)
Taarafo.Core.Tests      (Test Project)
```

**Violation:** tst-projects-001 — Build and Provision infrastructure projects are missing.

## Mixed Layers in Source Folder

```
Taarafo.Core/
  Helpers/                  ← violates tst-projects-005 — not a recognised layer folder
  Services/
    PostService.cs           ← violates tst-projects-007 — missing Foundations/ subfolder
    PostBroker.cs            ← violates tst-projects-009 — broker mixed into Services folder
  Models/
    Post.cs                  ← violates tst-projects-006 — missing Foundations/{Entity}/Exceptions/ structure
    PostException.cs         ← violates tst-projects-006 — exception not in Exceptions/ subfolder
```

## Flat Test Structure

```
Taarafo.Core.Tests/
  PostServiceTests.cs        ← violates tst-projects-010, tst-projects-011 — not mirroring source
  PostBrokerTests.cs
```
