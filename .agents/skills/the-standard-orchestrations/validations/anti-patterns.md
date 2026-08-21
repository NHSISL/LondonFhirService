# The Standard — Orchestration Services — Anti-Patterns

## orchestration-calls-foundation

**Violates:** ts-orchestrations-001
**What happens:** An orchestration service injects `IStudentService` (a foundation service) directly.
**Why it's wrong:** Orchestration must depend on processing services. Calling a foundation service skips a layer and defeats the purpose of processing services.
**Fix:** Inject `IStudentProcessingService` instead. Move the logic into the processing service if it does not exist yet.

## business-logic-in-orchestration

**Violates:** ts-orchestrations-001
**What happens:** An orchestration operation validates the model or performs transformation before calling processing services.
**Why it's wrong:** Validation and transformation are concerns of foundation and processing services. Orchestration must only coordinate.
**Fix:** Move validation into the foundation service and any enrichment into the processing service.

## leaking-exceptions

**Violates:** ts-orchestrations-003
**What happens:** A `StudentProcessingDependencyException` propagates out of the orchestration service without being wrapped.
**Why it's wrong:** Higher layers (controllers, exposers) must not know about lower-level exception types. Every layer wraps and re-throws with its own exception type.
**Fix:** Catch all processing-level exceptions and rethrow as `{Purpose}OrchestrationDependencyException` or `{Purpose}OrchestrationServiceException`.
