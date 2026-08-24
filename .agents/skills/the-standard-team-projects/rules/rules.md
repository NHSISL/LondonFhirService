---
skill: the-standard-team-projects
type: rules
source-section: "4.1.2 Projects"
---

# The Standard Team — Projects — Rules

## Solution Structure

**tst-projects-001** [ERROR] A Standard solution must contain: an API project, a Build infrastructure project, a Provision project, an Acceptance test project, and a Build test project.
**tst-projects-002** [ERROR] Projects must be named using the pattern `{Product}.{Layer}` (e.g., `Taarafo.Core`, `Taarafo.Core.Infrastructure.Build`).
**tst-projects-003** [ERROR] Infrastructure projects must use the suffix `.Infrastructure.Build` for build and `.Infrastructure.Provision` for provisioning.
**tst-projects-004** [ERROR] Test projects must use the suffix `.Tests.Acceptance` for acceptance tests and `.Tests.Build` for build tests.

## Source Project Folder Structure

**tst-projects-005** [ERROR] Source project folders must be organised by architectural layer: Brokers, Models, Migrations, Services, Controllers.
**tst-projects-006** [ERROR] Models must be organised by domain under `Models/Foundations/{Entity}/` with an `Exceptions/` subfolder per entity.
**tst-projects-007** [ERROR] Services must be organised by type: `Services/Foundations/`, `Services/Processings/`, `Services/Orchestrations/`.
**tst-projects-008** [ERROR] Brokers must be organised by concern: `Brokers/DateTimes/`, `Brokers/Loggings/`, `Brokers/Storages/`.
**tst-projects-009** [ERROR] Layers must not be mixed in the same folder.

## Test Project Folder Structure

**tst-projects-010** [ERROR] Test projects must mirror the source project folder structure.
**tst-projects-011** [ERROR] Tests must be organised under `Services/{ServiceType}/{ServiceName}/` within the test project.
