---
skill: the-standard-team-projects
type: checklist
source-section: "4.1.2 Projects"
---

# The Standard Team — Projects — Checklist

- [ ] Solution contains an API project (tst-projects-001)
- [ ] Solution contains a Build infrastructure project using `.Infrastructure.Build` suffix (tst-projects-001, tst-projects-003)
- [ ] Solution contains a Provision infrastructure project using `.Infrastructure.Provision` suffix (tst-projects-001, tst-projects-003)
- [ ] Solution contains an Acceptance test project using `.Tests.Acceptance` suffix (tst-projects-001, tst-projects-004)
- [ ] Solution contains a Build test project using `.Tests.Build` suffix (tst-projects-001, tst-projects-004)
- [ ] All projects follow the `{Product}.{Layer}` naming pattern (tst-projects-002)
- [ ] Source folders are organised by layer: Brokers, Models, Migrations, Services, Controllers (tst-projects-005)
- [ ] Models are organised under `Models/Foundations/{Entity}/Exceptions/` (tst-projects-006)
- [ ] Services are organised under `Services/Foundations/`, `Services/Processings/`, `Services/Orchestrations/` (tst-projects-007)
- [ ] No layers are mixed in the same folder (tst-projects-009)
- [ ] Test project mirrors the source folder structure (tst-projects-010)
