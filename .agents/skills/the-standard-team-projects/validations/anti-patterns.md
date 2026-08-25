---
skill: the-standard-team-projects
type: anti-patterns
source-section: "4.1.2 Projects"
---

# The Standard Team — Projects — Anti-Patterns

## Mixed Layers

**Violates:** tst-projects-009
**What happens:** Brokers and Services are placed in the same folder, or Models are mixed with Controllers.
**Why it's wrong:** Each layer has distinct responsibilities. Mixing them breaks discoverability, makes the architecture ambiguous, and prevents agents from applying layer-specific rules.
**Fix:** Separate into dedicated folders: `Brokers/`, `Models/`, `Services/`, `Controllers/`.

## Missing Infra Projects

**Violates:** tst-projects-001, tst-projects-003
**What happens:** The solution contains only the API project and test project — no Build or Provision infrastructure projects.
**Why it's wrong:** The Standard requires infrastructure projects for build automation and environment provisioning. Without them, CI/CD pipelines cannot be set up consistently.
**Fix:** Add `{Product}.Infrastructure.Build` and `{Product}.Infrastructure.Provision` console app projects to the solution.

## Exceptions Outside Entity Folder

**Violates:** tst-projects-006
**What happens:** Exception classes for a domain entity are placed directly under `Models/` or in a shared `Exceptions/` folder instead of `Models/Foundations/{Entity}/Exceptions/`.
**Why it's wrong:** Exceptions belong to the entity they represent. Placing them elsewhere hides the relationship and makes the model structure inconsistent.
**Fix:** Move exception classes to `Models/Foundations/{Entity}/Exceptions/`.

## Test Project Not Mirroring Source

**Violates:** tst-projects-010, tst-projects-011
**What happens:** The test project uses a flat folder structure or arbitrary grouping that does not reflect the source project's layer hierarchy.
**Why it's wrong:** Test discoverability depends on a predictable folder structure. When tests are not mirrored, it becomes hard to find, run, or review tests for specific services.
**Fix:** Organise tests under `Services/{ServiceType}/{ServiceName}/` to mirror the source.
