# The Standard — Exposers — Anti-Patterns

## controller-with-business-logic

**Violates:** ts-exposers-001
**What happens:** A controller action contains `if` statements, data transformation, or validation before calling the service.
**Why it's wrong:** Controllers must be pure HTTP adapters. Any logic in a controller is untestable at the business layer and violates separation of concerns.
**Fix:** Move all logic into the appropriate service layer. The controller must only call the service and map the result or exception to an HTTP response.

## multiple-service-injection

**Violates:** ts-exposers-001
**What happens:** A controller injects two or more services (e.g., `IStudentService` and `ICourseService`).
**Why it's wrong:** Controllers must depend on exactly one service. Coordinating multiple services is the responsibility of aggregation or orchestration services.
**Fix:** Create an aggregation or orchestration service and inject that single service into the controller.

## leaking-exceptions

**Violates:** ts-exposers-002
**What happens:** An unhandled exception propagates out of the controller and the framework returns a 500 with the raw exception message.
**Why it's wrong:** Internal exception details (stack traces, infrastructure types) must never reach clients. Every exception type must be caught and mapped to an appropriate HTTP response.
**Fix:** Add a catch block for every exception the service can throw and return the correct `IActionResult`.

## wrong-http-verb

**Violates:** ts-exposers-003, ts-exposers-004, ts-exposers-005, ts-exposers-006
**What happens:** A `[HttpGet]` endpoint modifies data, or a `[HttpPost]` endpoint retrieves a list.
**Why it's wrong:** HTTP verbs carry semantic meaning that clients and infrastructure (caches, proxies) rely on. Misuse causes correctness and caching bugs.
**Fix:** Use `[HttpPost]` for creation, `[HttpGet]` for retrieval, `[HttpPut]` for update, `[HttpDelete]` for deletion.
