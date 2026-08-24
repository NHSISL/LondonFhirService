# The Standard C# — Cancellation Patterns — Rules

## Method Signature Conventions

**tsc-csharp-cp-001** [ERROR] CancellationToken MUST be the last parameter in every method signature.

**tsc-csharp-cp-002** [ERROR] CancellationToken MUST default to `default` for backwards compatibility.

**tsc-csharp-cp-003** [ERROR] CancellationToken MUST NOT be nullable (`CancellationToken?` is forbidden).

**tsc-csharp-cp-004** [ERROR] The parameter MUST be named `cancellationToken` — never `token`, `ct`, or `cancelToken`.

**tsc-csharp-cp-005** [ERROR] Methods accepting CancellationToken MUST call `cancellationToken.ThrowIfCancellationRequested()` near the start of the method before dependency operations.

## Token Propagation

**tsc-csharp-cp-006** [ERROR] The same CancellationToken MUST flow through all layers (Controller → Coordination → Orchestration → Foundation → Broker → Dependency).

**tsc-csharp-cp-007** [ERROR] Never silently drop the token when calling dependencies.

**tsc-csharp-cp-008** [ERROR] When timeout support is required, use `CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutSource.Token)`.

**tsc-csharp-cp-009** [WARN] Do not create a new `CancellationTokenSource` unnecessarily — link to the upstream token instead.

## TryCatch Exception Handling

**tsc-csharp-cp-010** [ERROR] When timeout logic exists, BOTH catch blocks MUST be present in the correct order.

**tsc-csharp-cp-011** [ERROR] The timeout-guarded catch block `catch (OperationCanceledException) when (timeoutSource.IsCancellationRequested)` MUST precede the plain `catch (OperationCanceledException)` block.

**tsc-csharp-cp-012** [ERROR] TimeoutException MAY be wrapped as a dependency failure (e.g., `DependencyException` or `FailedServiceDependencyException`).

**tsc-csharp-cp-013** [ERROR] OperationCanceledException MUST NEVER be wrapped in any service or dependency exception — it MUST be rethrown with `throw;`.

**tsc-csharp-cp-014** [ERROR] Whenever a method accepts a CancellationToken, the plain `catch (OperationCanceledException) { throw; }` block MUST exist to prevent it from being caught by the catch-all exception handler.

**tsc-csharp-cp-015** [ERROR] In parallel orchestration, pass the same CancellationToken to all parallel operations.

## Applicability

**tsc-csharp-cp-016** [ERROR] CancellationToken SHOULD be used for database operations, HTTP operations, file operations, queues, messaging, and long-running work.

**tsc-csharp-cp-017** [WARN] CancellationToken SHOULD NOT be added to trivial in-memory operations (e.g., `public int Add(int x, int y, CancellationToken cancellationToken)`).

## Testing

**tsc-csharp-cp-018** [ERROR] Dependency exception tests MUST include `TimeoutException` in Theory MemberData to validate localization and categorization.

**tsc-csharp-cp-019** [ERROR] Exception tests MUST verify true cancellation (`OperationCanceledException`) is never wrapped — it must be rethrown unchanged.
