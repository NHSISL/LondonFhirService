# The Standard — Aggregation Services — Anti-Patterns

## aggregation-skips-layer

**Violates:** ts-aggregations-001
**What happens:** An aggregation service injects `IStudentProcessingService` or `IStorageBroker` directly.
**Why it's wrong:** Aggregation services must only consume orchestration services. Skipping a layer breaks testability and separation of concerns.
**Fix:** Route all calls through the appropriate orchestration service.

## business-logic-in-aggregation

**Violates:** ts-aggregations-002
**What happens:** An aggregation operation contains conditional logic that determines which orchestration services to call based on model state.
**Why it's wrong:** Aggregation is a fan-out point, not a decision point. Business decisions belong in orchestration or processing services.
**Fix:** Move conditional logic to the appropriate orchestration service and have the aggregation call it unconditionally.

## leaking-orchestration-exceptions

**Violates:** ts-aggregations-003
**What happens:** A `StudentEnrollmentOrchestrationDependencyException` propagates out of the aggregation service without being wrapped.
**Why it's wrong:** Callers (controllers) must not handle orchestration-level exception types. Every layer must wrap and re-throw.
**Fix:** Catch all orchestration exceptions and rethrow as `{Purpose}AggregationDependencyException`.
