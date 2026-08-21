---
name: the-standard-cancellation-patterns
version: 0.1.0
csharp-standard-version: v0.8
applies-to: ["*.cs"]
depends-on: ["the-standard-csharp-methods", "the-standard-csharp-classes"]
---

# The Standard C# — Cancellation Patterns

## 0/ Context (Purpose — WHY this skill exists)

0.0/ Where: Any C# file where CancellationToken, timeout logic, or TryCatch exception handling is used.
0.1/ Who: Any engineer writing or reviewing async C# methods that accept CancellationToken or implement timeout behavior.
0.2/ What: Governs CancellationToken method signatures, token propagation, TryCatch exception handling for OperationCanceledException and TimeoutException, parallel orchestration, and testing of cancellation behavior.
0.3/ Applies to: *.cs — specifically service, broker, and orchestration files using TryCatch and async methods.
0.4/ Version: The Standard C# Cancellation Patterns v1.0
0.5/ Depends on: the-standard-csharp-methods, the-standard-csharp-classes

## 1/ Actual (Dependency — WHAT the rules are and what they depend on)

1.0/ Dos:

1. CancellationToken MUST be the last parameter in method signatures → see rules/rules.md#tsc-csharp-cp-001
2. CancellationToken MUST default to `default` for backwards compatibility → see rules/rules.md#tsc-csharp-cp-002
3. Use the canonical parameter name `cancellationToken` → see rules/rules.md#tsc-csharp-cp-004
4. Methods accepting CancellationToken MUST call `ThrowIfCancellationRequested()` → see rules/rules.md#tsc-csharp-cp-005
5. The same token MUST flow through all layers (Controller → Service → Broker) → see rules/rules.md#tsc-csharp-cp-006
6. Link tokens when timeout support is required using `CreateLinkedTokenSource` → see rules/rules.md#tsc-csharp-cp-008
7. When timeout logic exists, catch `OperationCanceledException when (timeoutSource.IsCancellationRequested)` BEFORE plain `OperationCanceledException` → see rules/rules.md#tsc-csharp-cp-011
8. TimeoutException MAY be wrapped as a dependency failure → see rules/rules.md#tsc-csharp-cp-012
9. Include BOTH catch blocks when CancellationToken is present → see rules/rules.md#tsc-csharp-cp-014
10. Pass the same token to all parallel operations → see rules/rules.md#tsc-csharp-cp-015

1.1/ Don'ts:

1. Never use nullable CancellationToken (`CancellationToken?`) → see validations/anti-patterns.md#nullable-cancellation-token
2. Never use abbreviated parameter names (token, ct, cancelToken) → see validations/anti-patterns.md#abbreviated-parameter-names
3. Never drop the token silently when calling dependencies → see validations/anti-patterns.md#dropped-token
4. Never wrap OperationCanceledException in service or dependency exceptions → see validations/anti-patterns.md#wrapped-cancellation
5. Never create new CancellationTokenSource unnecessarily → see validations/anti-patterns.md#unnecessary-token-source
6. Never place plain `catch (OperationCanceledException)` before timeout-guarded catch → see validations/anti-patterns.md#incorrect-catch-order
7. Never add CancellationToken to trivial in-memory operations → see validations/anti-patterns.md#trivial-operations
8. Never omit `catch (OperationCanceledException) { throw; }` when CancellationToken is present → see validations/anti-patterns.md#missing-cancellation-catch

1.2/ Ask:

- When uncertain whether an operation warrants cancellation support (e.g., borderline I/O vs in-memory).
- When timeout duration is not specified by the consumer.
- When parallel orchestration requires different cancellation strategies per task.

1.3/ Defaults:

- If no timeout is specified, omit the timeout-guarded catch block but ALWAYS include `catch (OperationCanceledException) { throw; }`.
- If timeout logic exists, BOTH catch blocks MUST be present in the correct order.
- Parameter name defaults to `cancellationToken` unless explicitly overridden by framework convention.
- Cancellation validation (`ThrowIfCancellationRequested()`) should occur near the start of the method before dependency calls.

1.4/ Examples:

**✅ Correct method signature:**
```csharp
public ValueTask<Student> RetrieveStudentByIdAsync(
    Guid studentId,
    CancellationToken cancellationToken = default)
```

**❌ Nullable CancellationToken:**
```csharp
public ValueTask<Student> RetrieveStudentByIdAsync(
    Guid studentId,
    CancellationToken? cancellationToken)
```

**✅ Correct TryCatch with timeout:**
```csharp
catch (OperationCanceledException)
when (timeoutSource.IsCancellationRequested)
{
    var timeoutException = new TimeoutException("Operation timed out.");
    throw CreateAndLogDependencyException(timeoutException);
}
catch (OperationCanceledException)
{
    throw;
}
```

**❌ Wrapped cancellation:**
```csharp
catch (OperationCanceledException exception)
{
    throw new FailedServiceException(exception);
}
```

For complete examples, see:
- ✅ examples/good/example_good_method_signature.cs
- ✅ examples/good/example_good_trycatch_with_timeout.cs
- ✅ examples/good/example_good_trycatch_no_timeout.cs
- ✅ examples/good/example_good_token_propagation.cs
- ✅ examples/good/example_good_parallel_orchestration.cs
- ✅ examples/good/example_good_test_cancellation.cs
- ❌ examples/bad/example_bad_nullable_token.cs
- ❌ examples/bad/example_bad_abbreviated_name.cs
- ❌ examples/bad/example_bad_dropped_token.cs
- ❌ examples/bad/example_bad_wrapped_cancellation.cs
- ❌ examples/bad/example_bad_catch_order.cs
- ❌ examples/bad/example_bad_unnecessary_catch.cs

1.5/ Templates:

- Scaffold a service method with timeout: see templates/
- Scaffold a service method without timeout: see templates/

1.6/ Checklists:

- Pre-review checklist: see validations/checklist.md

1.7/ Contracts:

- Method signature contracts: see contracts/contracts.json

## 2/ Expected (Exposure — WHAT comes out)

2.0/ Format: C# methods with correct CancellationToken signatures and TryCatch exception handling patterns.
2.1/ Outcome: All cancellation and timeout handling is correct, propagated through all layers, and testable. OperationCanceledException bubbles naturally; TimeoutException is localized as a dependency failure.
2.2/ Tone: Direct. Cite rule IDs. No personal preference. Violations must be blocked with clear references to the violated rule.
