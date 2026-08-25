# The Standard — Testing — Rules

## TDD Discipline

**ts-testing-001** [ERROR] Tests must be written before the implementation — a failing test must exist before any implementation code is written.
**ts-testing-005** [ERROR] A failing test must be committed with the message `{TestName} -> FAIL` before writing implementation.
**ts-testing-006** [ERROR] The passing implementation must be committed with the message `{TestName} -> PASS`.

## Naming

**ts-testing-002** [ERROR] Test methods must follow the naming pattern `Should{Outcome}_{Condition}Async` (e.g., `ShouldAddStudentAsync`, `ShouldThrowValidationExceptionOnAddIfStudentIsNullAsync`).
**ts-testing-007** [ERROR] Test classes must follow the naming pattern `{SubjectUnderTest}Tests` (e.g., `StudentServiceTests`).

## Structure

**ts-testing-003** [ERROR] Each test must have a single assertion or assertions grouped for one specific behavior.
**ts-testing-004** [ERROR] Tests must use mocks or fakes for all dependencies — never real infrastructure (databases, file systems, HTTP).

## Coverage

**ts-testing-008** [ERROR] Tests must cover the happy path, validation failures, dependency failures, and service failures for every operation.
