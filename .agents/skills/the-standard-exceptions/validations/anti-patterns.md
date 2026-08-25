# The Standard — Exceptions — Anti-Patterns

## leaking-raw-exceptions

**Violates:** ts-exceptions-003
**What happens:** A `SqlException` thrown in the broker propagates uncaught through the service and into the controller.
**Why it's wrong:** Raw infrastructure exceptions expose implementation details, break the contract between layers, and confuse callers who receive infrastructure-specific error messages.
**Fix:** Catch `SqlException` in the service and rethrow as `FailedStudentStorageException` wrapped in `StudentDependencyException`.

## catch-all-exception

**Violates:** ts-exceptions-001
**What happens:** A service has a single `catch (Exception ex)` block that wraps everything in one generic exception type.
**Why it's wrong:** Callers (controllers, higher services) cannot distinguish validation failures from infrastructure failures. Each category requires a different HTTP response code.
**Fix:** Use separate catch blocks for `ValidationException`, `DbUpdateConcurrencyException`, `SqlException`, and `Exception` (catch-all last), wrapping each in the correct category.

## swallowed-exception

**Violates:** ts-exceptions-007
**What happens:** A catch block logs the exception and returns a default value without re-throwing.
**Why it's wrong:** Silent failure hides errors from callers. The exception contract is broken — callers receive no indication that the operation failed.
**Fix:** Either re-throw the wrapped exception or propagate a meaningful failure result. Never silently swallow.

## shared-exception-types

**Violates:** ts-exceptions-006
**What happens:** `StudentProcessingService` catches and re-throws `StudentDependencyException` (a foundation-layer type) without wrapping it in a processing-layer exception.
**Why it's wrong:** Higher layers become coupled to lower-layer exception types, breaking encapsulation.
**Fix:** Wrap `StudentDependencyException` in `StudentProcessingDependencyException` before propagating.
