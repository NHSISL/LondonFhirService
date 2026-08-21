---
name: the-standard-exceptions
version: 0.1.0
standard-version: v2.50.0
applies-to: ["*Exception*.cs"]
depends-on: ["the-standard-core"]
---

# The Standard — Exceptions

## 0/ Context (Purpose — WHY this skill exists)

0.0/ Where: All Standard-compliant systems — exception models, exception mapping, and wrapping across all layers.
0.1/ Who: Engineers implementing or reviewing exception hierarchies, service exception wrapping, and controller exception mapping.
0.2/ What: Enforces the three-category exception model (Validation, Dependency, Service), correct wrapping and unwrapping, and exception visibility rules.
0.3/ Applies to: *Exception*.cs
0.4/ Version: v2.50.0
0.5/ Depends on: the-standard-core

## 1/ Actual (Dependency — WHAT the rules are and what they depend on)

1.0/ Dos:
  1. Every service must expose exactly three outer exception categories: Validation, Dependency, and Service → see rules/rules.md#ts-exceptions-001
  2. ValidationException must wrap all input-related errors (null, empty, invalid) → see rules/rules.md#ts-exceptions-002
  3. DependencyException must wrap all infrastructure-related errors (SQL, HTTP, queue) → see rules/rules.md#ts-exceptions-003
  4. DependencyValidationException must wrap dependency-detected business rule violations (conflict, not found) → see rules/rules.md#ts-exceptions-004
  5. ServiceException must wrap any unexpected exception not covered by the above categories → see rules/rules.md#ts-exceptions-005
  6. Each layer must define its own exception types — never re-use exception types from a lower layer → see rules/rules.md#ts-exceptions-006
  7. Inner exceptions must always be preserved when wrapping → see rules/rules.md#ts-exceptions-007
  8. Exception messages must be human-readable and must not expose infrastructure details → see rules/rules.md#ts-exceptions-008

1.1/ Don'ts:
  1. Must not let raw infrastructure exceptions (SqlException, HttpRequestException) cross service boundaries → see validations/anti-patterns.md#leaking-raw-exceptions
  2. Must not use a single catch-all exception type for all failures → see validations/anti-patterns.md#catch-all-exception
  3. Must not swallow exceptions silently — every caught exception must be re-thrown or logged → see validations/anti-patterns.md#swallowed-exception
  4. Must not share exception types between layers → see validations/anti-patterns.md#shared-exception-types

1.2/ Ask:
  - Ask when an exception does not clearly fit Validation, Dependency, or Service — determine the correct category before creating a new type.

1.3/ Defaults:
  - Exception hierarchy per entity: `{Entity}ValidationException`, `{Entity}DependencyValidationException`, `{Entity}DependencyException`, `{Entity}ServiceException`.
  - Inner exception: always pass the caught exception as the inner exception parameter.
  - Exception message: `{description} error occurred, contact support.`

1.4/ Examples:
  - ✅ see examples/good/example_good_exception_hierarchy.cs
  - ❌ see examples/bad/example_bad_exception_handling.cs

1.5/ Templates:
  - Exception hierarchy scaffold: see templates/

1.6/ Checklists:
  - Pre-review checklist: see validations/checklist.md

1.7/ Contracts:
  - Exception hierarchy and category contracts: see contracts/contracts.json

## 2/ Expected (Exposure — WHAT comes out)

2.0/ Format: C# source code.
2.1/ Outcome: Consistent, layered exception hierarchies with correct wrapping, preserved inner exceptions, and no infrastructure leakage.
2.2/ Tone: Direct. Cite rule IDs. Violations must be fixed, not suggested.
