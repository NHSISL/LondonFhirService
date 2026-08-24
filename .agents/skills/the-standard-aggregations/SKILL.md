---
name: the-standard-aggregations
version: 0.1.0
standard-version: v2.50.0
applies-to: ["*AggregationService*.cs"]
depends-on: ["the-standard-core", "the-standard-orchestrations"]
---

# The Standard — Aggregation Services

## 0/ Context (Purpose — WHY this skill exists)

0.0/ Where: The fourth-level (top business logic) service layer of a Standard-compliant system.
0.1/ Who: Engineers implementing or reviewing aggregation services.
0.2/ What: Enforces single-call exposure, fan-out orchestration coordination, and exception wrapping for aggregation services.
0.3/ Applies to: *AggregationService*.cs
0.4/ Version: v2.50.0
0.5/ Depends on: the-standard-core, the-standard-orchestrations

## 1/ Actual (Dependency — WHAT the rules are and what they depend on)

1.0/ Dos:
  1. Aggregation services must depend only on orchestration services → see rules/rules.md#ts-aggregations-001
  2. Aggregation services must expose a single operation that fans out to multiple orchestration services → see rules/rules.md#ts-aggregations-002
  3. Aggregation services must wrap all downstream exceptions in aggregation-level exceptions → see rules/rules.md#ts-aggregations-003
  4. Aggregation service interfaces must follow the naming pattern `I{Purpose}AggregationService` → see rules/rules.md#ts-aggregations-004
  5. Aggregation services must be partial classes split by operation category → see rules/rules.md#ts-aggregations-005

1.1/ Don'ts:
  1. Must not call processing or foundation services directly → see validations/anti-patterns.md#aggregation-skips-layer
  2. Must not implement business logic — only call and return → see validations/anti-patterns.md#business-logic-in-aggregation
  3. Must not expose internal orchestration exception types to callers → see validations/anti-patterns.md#leaking-orchestration-exceptions

1.2/ Ask:
  - Ask when an aggregation operation depends on more than five orchestration services — consider whether the design should be split.

1.3/ Defaults:
  - Default naming: `{Purpose}AggregationService` / `I{Purpose}AggregationService`.
  - Exception wrapping: catch orchestration-level exceptions, rethrow as aggregation-level exceptions.

1.4/ Examples:
  - ✅ see examples/good/example_good_aggregation_service.cs
  - ❌ see examples/bad/example_bad_aggregation_service.cs

1.5/ Templates:
  - Scaffold a new aggregation service: see templates/

1.6/ Checklists:
  - Pre-review checklist: see validations/checklist.md

1.7/ Contracts:
  - Naming and dependency contracts: see contracts/contracts.json

## 2/ Expected (Exposure — WHAT comes out)

2.0/ Format: C# source code.
2.1/ Outcome: Aggregation services that fan out to orchestration services through a single exposed operation.
2.2/ Tone: Direct. Cite rule IDs. Violations must be fixed, not suggested.
