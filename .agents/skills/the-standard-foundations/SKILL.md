---
name: the-standard-foundations
version: 0.1.0
standard-version: v2.50.0
applies-to: ["*Service*.cs"]
depends-on: ["the-standard-core", "the-standard-brokers"]
---

# The Standard — Foundation Services

## 0/ Context (Purpose — WHY this skill exists)

0.0/ Where: The foundation (first-level) service layer of any Standard-compliant system.
0.1/ Who: Engineers implementing or reviewing foundation services and their tests.
0.2/ What: Enforces structural validation, pass-through operations, single-entity responsibility, and correct broker dependency rules for foundation services.
0.3/ Applies to: *Service*.cs
0.4/ Version: v2.50.0
0.5/ Depends on: the-standard-core, the-standard-brokers

## 1/ Actual (Dependency — WHAT the rules are and what they depend on)

1.0/ Dos:
  1. Foundation services must serve only one entity type (e.g., StudentService for Student) → see rules/rules.md#ts-foundations-001
  2. Foundation services must perform structural and logical validation on all incoming models → see rules/rules.md#ts-foundations-002
  3. Foundation services must call only one broker per operation → see rules/rules.md#ts-foundations-003
  4. Foundation services must be a pass-through to the broker — no transformation or enrichment → see rules/rules.md#ts-foundations-004
  5. Foundation services must map broker exceptions to local exceptions using dependency validation → see rules/rules.md#ts-foundations-005
  6. Foundation services must implement try-catch wrapping for broker calls → see rules/rules.md#ts-foundations-006
  7. Foundation services must be partial classes, split by entity operation category → see rules/rules.md#ts-foundations-007
  8. Foundation service interfaces must follow the naming pattern `I{Entity}Service` → see rules/rules.md#ts-foundations-008

1.1/ Don'ts:
  1. Must not call another foundation service — only brokers → see validations/anti-patterns.md#service-calling-service
  2. Must not transform the model before passing it to the broker → see validations/anti-patterns.md#model-transformation
  3. Must not contain business rules combining multiple entities → see validations/anti-patterns.md#cross-entity-logic
  4. Must not suppress exceptions from brokers without wrapping → see validations/anti-patterns.md#unhandled-broker-exception

1.2/ Ask:
  - Ask when a foundation service appears to need data from more than one entity.
  - Ask when validation logic resembles a business rule rather than a structural check.

1.3/ Defaults:
  - When a service supports one entity, name it `{Entity}Service` and its interface `I{Entity}Service`.
  - Default exception wrapping: wrap `DbUpdateConcurrencyException` as `LockedStudentException`, storage exceptions as `FailedStudentStorageException`.
  - Structural validation checks: null, empty strings, default Guid, default DateTime.

1.4/ Examples:
  - ✅ see examples/good/example_good_foundation_service.cs
  - ❌ see examples/bad/example_bad_foundation_service.cs

1.5/ Templates:
  - Scaffold a new foundation service: see templates/

1.6/ Checklists:
  - Pre-review checklist: see validations/checklist.md

1.7/ Contracts:
  - Naming and validation contracts: see contracts/contracts.json

## 2/ Expected (Exposure — WHAT comes out)

2.0/ Format: C# source code.
2.1/ Outcome: Foundation services that validate structurally, call a single broker, wrap exceptions, and serve exactly one entity type.
2.2/ Tone: Direct. Cite rule IDs. Violations must be fixed, not suggested.
