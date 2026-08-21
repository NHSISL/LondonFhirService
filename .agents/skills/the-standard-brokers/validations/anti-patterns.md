# The Standard — Brokers — Anti-Patterns

## broker-with-validation

**Violates:** ts-brokers-001
**What happens:** A broker contains an `if` check or throws a validation exception before calling the external resource.
**Why it's wrong:** Validation is a business concern. Brokers are infrastructure — they must pass data through, not judge it.
**Fix:** Move the validation into the foundation service that calls this broker.

## broker-calling-broker

**Violates:** ts-brokers-001
**What happens:** A storage broker calls a logging broker or another broker to enrich its response.
**Why it's wrong:** Brokers must be single-purpose wrappers. Cross-broker calls create hidden dependencies and break testability.
**Fix:** Move cross-cutting concerns (logging, enrichment) into the service layer.

## leaking-infrastructure

**Violates:** ts-brokers-001
**What happens:** A broker exposes `DbContext`, `HttpClient`, or `SqlConnection` through a public property or method signature.
**Why it's wrong:** Consumers become coupled to the infrastructure technology, defeating the broker's abstraction purpose.
**Fix:** Return only domain models or primitives. Keep infrastructure types private.

## technology-specific-naming

**Violates:** ts-brokers-002
**What happens:** An interface is named `ISqlStorageBroker` or `IMongoStorageBroker`.
**Why it's wrong:** The technology leaks into the contract. Swapping storage technology now requires changing consumers.
**Fix:** Name the interface `IStorageBroker`. Put the technology name only in the concrete class: `SqlStorageBroker`.
