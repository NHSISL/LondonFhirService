# The Standard C# — Cancellation Patterns — Anti-Patterns

## Nullable Cancellation Token

**Violates:** tsc-csharp-cp-003

**What happens:** The method signature uses `CancellationToken?` instead of `CancellationToken`.

**Why it's wrong:** Nullable CancellationToken creates unnecessary null-checking complexity and breaks the standard contract. CancellationToken is a struct with a well-defined default value (`default`).

**Fix:** Change `CancellationToken? cancellationToken` to `CancellationToken cancellationToken = default`.

---

## Abbreviated Parameter Names

**Violates:** tsc-csharp-cp-004

**What happens:** The parameter is named `ct`, `token`, or `cancelToken` instead of `cancellationToken`.

**Why it's wrong:** Abbreviated names reduce code readability and violate The Standard's naming conventions. The canonical name is `cancellationToken`.

**Fix:** Rename the parameter to `cancellationToken`.

---

## Dropped Token

**Violates:** tsc-csharp-cp-006, tsc-csharp-cp-007

**What happens:** A method accepts CancellationToken but does not pass it to the dependency call.

Example:
```csharp
public async ValueTask<Student> RetrieveStudentAsync(
    Guid studentId,
    CancellationToken cancellationToken = default)
{
    return await storageBroker.SelectStudentByIdAsync(studentId);
}
```

**Why it's wrong:** The token is silently dropped, preventing cancellation from propagating to the dependency. The consumer's cancellation intent is lost.

**Fix:** Pass the token to all dependency calls:
```csharp
return await storageBroker.SelectStudentByIdAsync(studentId, cancellationToken);
```

---

## Wrapped Cancellation

**Violates:** tsc-csharp-cp-013

**What happens:** OperationCanceledException is caught and wrapped in a service or dependency exception.

Example:
```csharp
catch (OperationCanceledException exception)
{
    throw new FailedServiceException(exception);
}
```

**Why it's wrong:** Cancellation is not a failure. Wrapping OperationCanceledException prevents it from bubbling to the caller and breaks the cancellation contract.

**Fix:** Rethrow OperationCanceledException unchanged:
```csharp
catch (OperationCanceledException)
{
    throw;
}
```

---

## Incorrect Catch Order

**Violates:** tsc-csharp-cp-011

**What happens:** The plain `catch (OperationCanceledException)` block is placed before the timeout-guarded catch block.

Example:
```csharp
catch (OperationCanceledException)
{
    throw;
}
catch (OperationCanceledException)
when (timeoutSource.IsCancellationRequested)
{
    var timeoutException = new TimeoutException("Operation timed out.");
    throw CreateAndLogDependencyException(timeoutException);
}
```

**Why it's wrong:** The first catch block will always execute, preventing the timeout-guarded block from ever running. Timeout exceptions will not be localized.

**Fix:** Place the timeout-guarded catch block first:
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

---

## Unnecessary Token Source

**Violates:** tsc-csharp-cp-009

**What happens:** A new CancellationTokenSource is created instead of propagating the upstream token.

Example:
```csharp
public async ValueTask<Student> RetrieveStudentAsync(
    Guid studentId,
    CancellationToken cancellationToken = default)
{
    var source = new CancellationTokenSource();
    return await dependency.CallAsync(source.Token);
}
```

**Why it's wrong:** The upstream cancellation is replaced, breaking the cancellation chain. The consumer's cancellation intent is lost.

**Fix:** Propagate the upstream token directly, or link it if timeout is needed:
```csharp
return await dependency.CallAsync(cancellationToken);
```

Or for timeout:
```csharp
using var timeoutSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
using var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(
    cancellationToken,
    timeoutSource.Token);

return await dependency.CallAsync(linkedSource.Token);
```

---

## Missing Cancellation Catch

**Violates:** tsc-csharp-cp-014

**What happens:** A method accepts CancellationToken but does not include `catch (OperationCanceledException) { throw; }`.

**Why it's wrong:** Without this catch block, OperationCanceledException will be caught by the catch-all exception handler (e.g., `catch (Exception exception)`) and wrapped in a `ServiceException` or similar, preventing it from bubbling correctly to the caller.

**Fix:** Always include the plain catch block when CancellationToken is present:
```csharp
catch (OperationCanceledException)
{
    throw;
}
```

---

## Trivial Operations

**Violates:** tsc-csharp-cp-017

**What happens:** CancellationToken is added to a trivial in-memory operation.

Example:
```csharp
public int Add(int x, int y, CancellationToken cancellationToken = default)
{
    return x + y;
}
```

**Why it's wrong:** Cancellation support adds complexity and testing overhead for operations that complete instantly. CancellationToken is intended for I/O-bound or long-running operations.

**Fix:** Remove CancellationToken from trivial operations:
```csharp
public int Add(int x, int y)
{
    return x + y;
}
```
