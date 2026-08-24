---
name: the-standard-processings
version: 0.1.0
standard-version: v2.50.0
applies-to: ["*ProcessingService*.cs"]
depends-on: ["the-standard-core", "the-standard-foundations"]
---

# The Standard — Processing Services

## 0/ Context (Purpose — WHY this skill exists)

0.0/ Where: The second-level service layer of any Standard-compliant system.
0.1/ Who: Engineers implementing or reviewing processing services.
0.2/ What: Enforces higher-order business logic, combining foundation service calls, and cross-cutting business rules for a single entity type.
0.3/ Applies to: *ProcessingService*.cs
0.4/ Version: v2.50.0
0.5/ Depends on: the-standard-core, the-standard-foundations

## 1/ Actual (Dependency — WHAT the rules are and what they depend on)

1.0/ Dos:
  1. Processing services must call only foundation services — never brokers directly → see rules/rules.md#ts-processings-001
  2. Processing services must serve only one entity type → see rules/rules.md#ts-processings-002
  3. Processing services must implement higher-order operations that combine or extend foundation calls (e.g., EnsureStudentExistsAsync) → see rules/rules.md#ts-processings-003
  4. Processing services must map and wrap all downstream exceptions → see rules/rules.md#ts-processings-004
  5. Processing service interfaces must follow the naming pattern `I{Entity}ProcessingService` → see rules/rules.md#ts-processings-005
  6. Processing services must be partial classes split by operation category → see rules/rules.md#ts-processings-006

1.1/ Don'ts:
  1. Must not call a broker directly — only foundation services → see validations/anti-patterns.md#processing-calls-broker
  2. Must not call another processing service → see validations/anti-patterns.md#processing-calls-processing
  3. Must not duplicate validation already performed by the foundation service → see validations/anti-patterns.md#duplicate-validation
  4. Must not manage more than one entity type → see validations/anti-patterns.md#cross-entity-processing

1.2/ Ask:
  - Ask when a processing operation needs data from more than one entity — that likely belongs in orchestration.
  - Ask when an operation appears to bypass the foundation service.

1.3/ Defaults:
  - Default naming: `{Entity}ProcessingService` / `I{Entity}ProcessingService`.
  - Higher-order methods: `EnsureAsync`, `UpsertAsync`, `TryRemoveAsync`, `TryAddAsync`.
  - Exception wrapping: catch foundation exceptions and rethrow as processing-level exceptions.

1.4/ Examples:
  - ✅ see examples/good/example_good_processing_service.cs
  - ❌ see examples/bad/example_bad_processing_service.cs

1.5/ Templates:
  - Scaffold a new processing service: see templates/

1.6/ Checklists:
  - Pre-review checklist: see validations/checklist.md

1.7/ Contracts:
  - Naming and operation contracts: see contracts/contracts.json

## 2/ Expected (Exposure — WHAT comes out)

2.0/ Format: C# source code.
2.1/ Outcome: Processing services that compose foundation calls into higher-order operations for one entity type.
2.2/ Tone: Direct. Cite rule IDs. Violations must be fixed, not suggested.
