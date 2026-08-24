# The Standard — Foundation Services — Anti-Patterns

## service-calling-service

**Violates:** ts-foundations-003
**What happens:** A foundation service injects and calls another foundation service (e.g., `StudentService` calls `CourseService`).
**Why it's wrong:** Foundation services must depend only on brokers. Cross-service calls belong in higher-order processing or orchestration services.
**Fix:** Create a processing or orchestration service to coordinate multiple foundation services.

## model-transformation

**Violates:** ts-foundations-004
**What happens:** A foundation service modifies the model (adds timestamps, enriches data) before passing it to the broker.
**Why it's wrong:** Transformation is a business concern. Foundation services must be pure pass-throughs.
**Fix:** Move enrichment logic to a processing service above the foundation.

## cross-entity-logic

**Violates:** ts-foundations-001
**What happens:** `StudentService` also manages `Course` records or checks `Enrollment` constraints.
**Why it's wrong:** Each foundation service must own exactly one entity. Cross-entity rules belong in higher-order services.
**Fix:** Create a dedicated `CourseService` and coordinate in an orchestration service.

## unhandled-broker-exception

**Violates:** ts-foundations-005, ts-foundations-006
**What happens:** A broker call throws a `SqlException` that propagates uncaught to the controller.
**Why it's wrong:** Raw infrastructure exceptions must not cross service boundaries. They reveal implementation details and break the error contract.
**Fix:** Wrap in try-catch and rethrow as a local exception (e.g., `FailedStudentStorageException`).
