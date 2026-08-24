# The Standard React TypeScript — Brokers — Rules

## Broker Responsibility

**tsr-brokers-001** [ERROR] Brokers MUST be the only layer that talks to external dependencies (APIs, storage, logging, external services).

**tsr-brokers-002** [ERROR] Broker methods MUST use async/await for all I/O operations.

**tsr-brokers-003** [ERROR] Brokers MUST map external exceptions to broker-layer exceptions with meaningful messages.

**tsr-brokers-004** [ERROR] API broker methods MUST use standard HTTP verbs: Get, Post, Put, Delete.

**tsr-brokers-005** [ERROR] Broker method names MUST follow pattern: `{verb}{Entity}Async` (e.g., `getPatientAsync`, `postPatientAsync`).

**tsr-brokers-006** [ERROR] Storage brokers MUST abstract localStorage/sessionStorage/IndexedDB behind interface.

**tsr-brokers-007** [ERROR] Logging brokers MUST abstract console or external logging services.

**tsr-brokers-008** [ERROR] Brokers MUST NOT contain business logic (filtering, validation, transformation beyond format conversion).

**tsr-brokers-009** [ERROR] Brokers MUST be injected via dependency injection, not instantiated directly in services.

**tsr-brokers-010** [ERROR] DateTime brokers MUST abstract Date object creation and manipulation.

## Broker Organization

**tsr-brokers-011** [ERROR] Brokers MUST be organized by external dependency type: apis/, storages/, loggings/, datetimes/.

**tsr-brokers-012** [ERROR] Broker class names MUST follow pattern: `{Entity}{Type}Broker` (e.g., `PatientApiBroker`, `LocalStorageBroker`).

**tsr-brokers-013** [ERROR] Each broker MUST serve one and only one external dependency.

**tsr-brokers-014** [WARN] Brokers SHOULD expose interfaces for testing and mocking.

## Exception Handling

**tsr-brokers-015** [ERROR] Brokers MUST catch external exceptions and wrap them in broker-specific exceptions.

**tsr-brokers-016** [ERROR] Broker exceptions MUST preserve the original exception as inner exception.

**tsr-brokers-017** [ERROR] Brokers MUST NOT catch and swallow exceptions without rethrowing.

**tsr-brokers-018** [ERROR] Brokers MUST NOT return error codes — use exceptions for error signaling.

## Broker Interactions

**tsr-brokers-019** [ERROR] Brokers MUST NOT call other brokers — composition happens in services.

**tsr-brokers-020** [ERROR] Brokers MUST NOT perform retries or circuit breaking — that belongs in services.

**tsr-brokers-021** [WARN] API brokers SHOULD accept CancellationToken-equivalent (AbortSignal) for request cancellation.
