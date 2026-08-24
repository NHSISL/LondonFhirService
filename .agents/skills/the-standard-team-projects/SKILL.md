---
name: the-standard-team-projects
version: 0.1.0
standard-team-version: v0.1.0
applies-to: ["*.sln", "*.csproj", "**/*.cs"]
depends-on: []
---

# The Standard Team — Projects

## 0/ Context (Purpose — WHY this skill exists)

0.0/ Where: Any Standard Team-compliant .NET solution — solution and project structure.
0.1/ Who: Engineers creating or reviewing .NET solutions and project folder structures.
0.2/ What: Enforces the Standard solution layout including API, Build, Provision, Acceptance, and Build test projects; and the folder structure within each project.
0.3/ Applies to: *.sln, *.csproj, solution and project folder structures.
0.4/ Version: v0.1.0
0.5/ Depends on: none

## 1/ Actual (Dependency — WHAT the rules are and what they depend on)

1.0/ Dos:
  1. Solutions must include an API project, a Build infrastructure project, a Provision project, an Acceptance test project, and a Build test project → see rules/rules.md#tst-projects-001
  2. Follow the Standard project naming pattern: `{Product}.{Layer}` → see rules/rules.md#tst-projects-002
  3. Organise source folders by layer: Brokers, Models, Services, Controllers → see rules/rules.md#tst-projects-003
  4. Organise Models by domain entity with an Exceptions subfolder per entity → see rules/rules.md#tst-projects-004
  5. Organise Services by type: Foundations, Processings, Orchestrations → see rules/rules.md#tst-projects-005
  6. Mirror the source folder structure in test projects → see rules/rules.md#tst-projects-006

1.1/ Don'ts:
  1. Must not mix layers in a single folder (e.g., brokers and services in the same folder) → see validations/anti-patterns.md#mixed-layers
  2. Must not omit the Build or Provision infrastructure projects → see validations/anti-patterns.md#missing-infra-projects
  3. Must not place model exception classes outside the entity-specific Exceptions subfolder → see validations/anti-patterns.md#exceptions-outside-folder

1.2/ Ask:
  - Ask when the product name or solution namespace is ambiguous.
  - Ask when the domain entity hierarchy is unclear.

1.3/ Defaults:
  - Default solution structure: API + Build + Provision + Acceptance + Build test projects.
  - Default source folder order: Brokers → Models → Migrations → Services → Controllers.
  - Test projects mirror the source structure under Services/{ServiceType}/{ServiceName}.

1.4/ Examples:
  - ✅ see examples/good/example_good_solution_structure.md
  - ❌ see examples/bad/example_bad_solution_structure.md

1.5/ Templates:
  - Scaffold a Standard solution structure: see templates/

1.6/ Checklists:
  - Pre-review checklist: see validations/checklist.md

1.7/ Contracts:
  - Solution and project structure contracts: see contracts/contracts.json

## 2/ Expected (Exposure — WHAT comes out)

2.0/ Format: Solution and folder structure documentation or scaffolding.
2.1/ Outcome: Every solution follows the Standard project layout with correct naming, layering, and test mirroring.
2.2/ Tone: Direct. Cite rule IDs. Violations must be corrected.
