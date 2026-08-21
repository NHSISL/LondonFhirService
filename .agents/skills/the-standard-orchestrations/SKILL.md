---
name: the-standard-orchestrations
version: 0.1.0
standard-version: v2.50.0
applies-to: ["*OrchestrationService*.cs"]
depends-on: ["the-standard-core", "the-standard-processings"]
---

# The Standard — Orchestration Services

## 0/ Context (Purpose — WHY this skill exists)

0.0/ Where: The third-level service layer of any Standard-compliant system.
0.1/ Who: Engineers implementing or reviewing orchestration services.
0.2/ What: Enforces multi-entity coordination, dependency on processing services, and exception management rules for orchestration services.
0.3/ Applies to: *OrchestrationService*.cs
0.4/ Version: v2.50.0
0.5/ Depends on: the-standard-core, the-standard-processings

## 1/ Actual (Dependency — WHAT the rules are and what they depend on)

1.0/ Dos:
  1. Orchestration services must depend only on processing services — never on foundation services or brokers directly → see rules/rules.md#ts-orchestrations-001
  2. Orchestration services may coordinate multiple entity types → see rules/rules.md#ts-orchestrations-002
  3. Orchestration services must wrap all downstream exceptions in orchestration-level exceptions → see rules/rules.md#ts-orchestrations-003
  4. Orchestration service interfaces must follow the naming pattern `I{Purpose}OrchestrationService` → see rules/rules.md#ts-orchestrations-004
  5. Orchestration services must be partial classes split by operation category → see rules/rules.md#ts-orchestrations-005

1.1/ Don'ts:
  1. Must not call foundation services or brokers directly → see validations/anti-patterns.md#orchestration-calls-foundation
  2. Must not implement business logic that belongs in a processing service → see validations/anti-patterns.md#business-logic-in-orchestration
  3. Must not expose internal processing or foundation exception types to higher layers → see validations/anti-patterns.md#leaking-exceptions

1.2/ Ask:
  - Ask when an orchestration operation needs more than three processing services — consider splitting.
  - Ask when a single orchestration operation is becoming a multi-hundred-line method.

1.3/ Defaults:
  - Default naming: `{Purpose}OrchestrationService` / `I{Purpose}OrchestrationService`.
  - Exception wrapping: catch processing-level exceptions, rethrow as orchestration-level exceptions.

1.4/ Examples:
  - ✅ see examples/good/example_good_orchestration_service.cs
  - ❌ see examples/bad/example_bad_orchestration_service.cs

1.5/ Templates:
  - Scaffold a new orchestration service: see templates/

1.6/ Checklists:
  - Pre-review checklist: see validations/checklist.md

1.7/ Contracts:
  - Naming and dependency contracts: see contracts/contracts.json

## 2/ Expected (Exposure — WHAT comes out)

2.0/ Format: C# source code.
2.1/ Outcome: Orchestration services that coordinate multiple entity processing services, wrap exceptions, and follow naming conventions.
2.2/ Tone: Direct. Cite rule IDs. Violations must be fixed, not suggested.
