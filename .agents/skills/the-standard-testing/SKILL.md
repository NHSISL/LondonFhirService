---
name: the-standard-testing
version: 0.1.0
standard-version: v2.50.0
applies-to: ["*Tests*.cs"]
depends-on: ["the-standard-core", "the-standard-foundations"]
---

# The Standard — Testing

## 0/ Context (Purpose — WHY this skill exists)

0.0/ Where: Test projects in any Standard-compliant system.
0.1/ Who: Engineers writing or reviewing unit, acceptance, or integration tests.
0.2/ What: Enforces TDD discipline, test naming, FAIL/PASS commit patterns, mocking rules, and test structure.
0.3/ Applies to: *Tests*.cs
0.4/ Version: v2.50.0
0.5/ Depends on: the-standard-core, the-standard-foundations

## 1/ Actual (Dependency — WHAT the rules are and what they depend on)

1.0/ Dos:
  1. Tests must be written before the implementation (TDD — failing test first) → see rules/rules.md#ts-testing-001
  2. Test methods must follow the naming pattern `Should{Outcome}_{Condition}Async` → see rules/rules.md#ts-testing-002
  3. Each test must have a single assertion or a logically grouped set of assertions for one behavior → see rules/rules.md#ts-testing-003
  4. Tests must use mocks/fakes for all dependencies — never real infrastructure → see rules/rules.md#ts-testing-004
  5. Commit a failing test with message `{TestName} -> FAIL` before implementing → see rules/rules.md#ts-testing-005
  6. Commit the passing implementation with message `{TestName} -> PASS` → see rules/rules.md#ts-testing-006
  7. Test classes must follow the naming pattern `{SubjectUnderTest}Tests` → see rules/rules.md#ts-testing-007
  8. Tests must cover the happy path, validation failures, dependency failures, and service failures → see rules/rules.md#ts-testing-008

1.1/ Don'ts:
  1. Must not write implementation code before a failing test exists → see validations/anti-patterns.md#impl-before-test
  2. Must not use real databases, file systems, or HTTP in unit tests → see validations/anti-patterns.md#real-infrastructure
  3. Must not assert on multiple unrelated behaviors in a single test → see validations/anti-patterns.md#multi-assert
  4. Must not name tests with vague names like `Test1`, `TestAdd`, or `VerifyMethod` → see validations/anti-patterns.md#vague-test-names

1.2/ Ask:
  - Ask when a test requires a real database — that is an integration test and must be isolated.
  - Ask when a test is too large to have a single clear assertion.

1.3/ Defaults:
  - Test naming: `ShouldAddStudentAsync`, `ShouldThrowValidationExceptionOnAddIfStudentIsNullAsync`.
  - Commit on failing test: `ShouldAddStudentAsync -> FAIL`
  - Commit on passing impl: `ShouldAddStudentAsync -> PASS`
  - Mock library: Moq (or equivalent).
  - Test framework: xUnit (or equivalent).

1.4/ Examples:
  - ✅ see examples/good/example_good_unit_test.cs
  - ❌ see examples/bad/example_bad_unit_test.cs

1.5/ Templates:
  - Test class scaffold: see templates/

1.6/ Checklists:
  - Pre-review checklist: see validations/checklist.md

1.7/ Contracts:
  - Test naming and coverage contracts: see contracts/contracts.json

## 2/ Expected (Exposure — WHAT comes out)

2.0/ Format: C# xUnit test source code.
2.1/ Outcome: Tests that are isolated, named correctly, cover all standard paths, and follow TDD commit discipline.
2.2/ Tone: Direct. Cite rule IDs. Violations must be fixed, not suggested.
