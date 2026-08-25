# The Standard — Testing — Anti-Patterns

## impl-before-test

**Violates:** ts-testing-001
**What happens:** A developer writes the service implementation, then writes tests to match it.
**Why it's wrong:** Tests written after implementation describe existing behavior rather than specified behavior. This defeats the purpose of TDD as a design tool.
**Fix:** Delete the implementation, write the failing test first, commit it as `{TestName} -> FAIL`, then implement to make it pass.

## real-infrastructure

**Violates:** ts-testing-004
**What happens:** A unit test creates a real `DbContext`, calls a real HTTP endpoint, or reads from the file system.
**Why it's wrong:** Unit tests must be fast, isolated, and deterministic. Real infrastructure makes tests slow, flaky, and environment-dependent.
**Fix:** Mock all dependencies using Moq or equivalent. Reserve real infrastructure for integration or acceptance tests.

## multi-assert

**Violates:** ts-testing-003
**What happens:** A single test method asserts on the result, the log call, the broker call count, and the exception type.
**Why it's wrong:** Multiple assertions obscure which behavior failed. A single failing assertion should pinpoint the exact broken behavior.
**Fix:** Split into separate tests, each asserting one behavior. One test for the return value, one for the broker call, etc.

## vague-test-names

**Violates:** ts-testing-002
**What happens:** Tests are named `TestAdd`, `Test1`, or `VerifyStudentService`.
**Why it's wrong:** Vague names make it impossible to understand what behavior failed from the test report alone.
**Fix:** Rename to `ShouldAddStudentAsync`, `ShouldThrowValidationExceptionOnAddIfStudentIsNullAsync`, etc.
