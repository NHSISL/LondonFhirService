# The Standard — Processing Services — Anti-Patterns

## processing-calls-broker

**Violates:** ts-processings-001
**What happens:** A processing service injects `IStorageBroker` and calls it directly.
**Why it's wrong:** Processing services must depend only on foundation services. Direct broker calls skip a layer and break testability.
**Fix:** Route the call through the appropriate foundation service.

## processing-calls-processing

**Violates:** ts-processings-001
**What happens:** `StudentProcessingService` injects `ICourseProcessingService`.
**Why it's wrong:** Processing services must not depend on peer processing services. Cross-entity coordination belongs in orchestration.
**Fix:** Create an orchestration service that coordinates both processing services.

## duplicate-validation

**Violates:** ts-processings-002
**What happens:** A processing service re-validates null/empty fields that the foundation service already validates.
**Why it's wrong:** Duplicated validation creates noise, diverges over time, and adds false complexity.
**Fix:** Remove structural validation from the processing service — trust the foundation layer.

## cross-entity-processing

**Violates:** ts-processings-002
**What happens:** `StudentProcessingService` also manages `Enrollment` records.
**Why it's wrong:** Processing services must serve exactly one entity. Cross-entity logic belongs in orchestration.
**Fix:** Create a dedicated `EnrollmentProcessingService` and coordinate in an orchestration service.
