---
applyTo: "**/*Tests.cs, **/*Tests.*.cs"
description: "Unit testing conventions using xUnit, Moq, and FluentAssertions."
---

# Unit Testing Conventions — The Standard

Enforce all rules below on every test file during code generation and review.
General C# naming and formatting rules are in `csharp.instructions.md`.
Line-length, blank-line, and argument-wrapping rules are in `copilot-instructions.md`.

---

## TDD Discipline

- **test-001** Write the failing test first. Never write implementation before a failing test exists.
- **test-002** Verify the test actually fails (test runner shows red) before committing a FAIL commit.
- **test-003** Write the minimum implementation required to pass. No more.
- **test-004** Verify the full relevant test suite passes before committing a PASS commit.
- **test-005** Refactor without changing behavior. All tests must still pass.

---

## File Structure

- **test-090** Tests MUST mirror the same partial-class split as the service under test.

  | File | Contains |
  |---|---|
  | `{Entity}ServiceTests.cs` | Constructor, mocks, helper methods, filler setup |
  | `{Entity}ServiceTests.Logic.{Method}.cs` | Happy-path / success-case tests |
  | `{Entity}ServiceTests.Validations.{Method}.cs` | Validation failure tests |
  | `{Entity}ServiceTests.Exceptions.{Method}.cs` | Dependency and service exception tests |

- **test-091** The root test file contains ONLY: constructor, mock fields, helper methods
  (`CreateRandom{Entity}`, `GetRandomDateTimeOffset`, `SameExceptionAs`, filler setup).
  No test methods belong in this file.

- **test-092** Logic test files contain happy-path tests only.
- **test-093** Validations test files contain validation failure tests only.
- **test-094** Exceptions test files contain dependency and service exception tests only.

---

## Test Method Naming

- **test-104** Success cases: `Should{Action}Async`
- **test-104** Failure cases: `ShouldThrow{Exception}On{Action}If{Condition}AndLogItAsync`

  ```csharp
  // ✅
  public async Task ShouldAddStudentAsync() { ... }

  public async Task ShouldThrowValidationExceptionOnAddIfStudentIsNullAndLogItAsync() { ... }

  // ❌
  public async Task AddStudent() { ... }
  public async Task TestNullStudent() { ... }
  ```

---

## Test Conventions

- **test-100** Every test MUST use the GWT pattern with `// given`, `// when`, `// then` comments.

  ```csharp
  // given
  Student randomStudent = CreateRandomStudent();
  // ... setup ...

  // when
  Student actualStudent = await this.studentService.AddStudentAsync(inputStudent);

  // then
  actualStudent.Should().BeEquivalentTo(expectedStudent);
  // ... verify ...
  ```

- **test-101** All dependencies MUST be mocked using Moq. Mock fields declared in the root file.

  ```csharp
  private readonly Mock<IStorageBroker> storageBrokerMock;
  private readonly Mock<ILoggingBroker> loggingBrokerMock;
  ```

- **test-102** Assertions MUST use FluentAssertions: `actual.Should().BeEquivalentTo(expected)`.
- **test-103** Single-case tests use `[Fact]`. Parameterized tests use `[Theory] [InlineData]`.

---

## Test Data — Variable Naming (test-035, test-105)

The purpose of this pattern is **readability**. `inputStudent` and `storageStudent` may both hold
the value of `randomStudent`, but the aliases make the test read as a narrative. Each variable
name signals its role at that stage of the flow rather than requiring the reader to track the
state of a single variable across multiple lines.

Rules:
1. Randomized values MUST use a `random` prefix: `randomId`, `randomStudent`.
2. Each stage of the flow aliases the previous value with an intent-revealing name.
3. `DeepClone` is applied at the point where state must be isolated — typically the final
   assertion target. Not every intermediate alias requires a clone.

**Simple Add flow:**

```csharp
Student randomStudent = CreateRandomStudent();
Student inputStudent = randomStudent;
Student storageStudent = inputStudent.DeepClone();
Student expectedStudent = storageStudent.DeepClone();

this.storageBrokerMock.Setup(broker =>
    broker.InsertStudentAsync(inputStudent))
        .ReturnsAsync(storageStudent);

// when
Student actualStudent = await this.studentService.AddStudentAsync(inputStudent);

// then
actualStudent.Should().BeEquivalentTo(expectedStudent);

this.storageBrokerMock.Verify(broker =>
    broker.InsertStudentAsync(inputStudent),
        Times.Once);
```

**Multi-step flow (e.g. retrieve then delete):**

```csharp
Guid randomId = Guid.NewGuid();
Guid inputStudentId = randomId;
Student randomStudent = CreateRandomStudent();
Student storageStudent = randomStudent;
Student deletedStudent = storageStudent;
Student expectedStudent = deletedStudent.DeepClone();
```

Mock setup and verify MUST reference the exact named variable at each step. Never use `randomStudent`
throughout when it plays different roles at different points.

---

## It.IsAny Restriction (test-106)

- `It.IsAny<T>()` MUST NOT appear in Logic or Validations test files.
- It is only permitted in Exceptions test files, where the subject under test is exception behavior
  — not whether the correct input value was passed.

```csharp
// ✅ in Exceptions file — we only care that the exception is thrown
this.storageBrokerMock.Verify(broker =>
    broker.InsertStudentAsync(It.IsAny<Student>()),
        Times.Once);

// ❌ in Logic or Validations file — exact variable must be used
this.storageBrokerMock.Verify(broker =>
    broker.InsertStudentAsync(It.IsAny<Student>()),
        Times.Once);
```

---

## Mock Verification (test-030, test-031, test-032)

- **test-030** Every test MUST verify exact broker calls using `Times.Once` or `Times.Never`.
- **test-031** Every test MUST end with `VerifyNoOtherCalls()` on ALL mocks.
- **test-032** Every error-path test MUST verify the logging broker received the expected exception.

  ```csharp
  // ✅
  this.storageBrokerMock.Verify(broker =>
      broker.InsertStudentAsync(inputStudent),
          Times.Once);

  this.loggingBrokerMock.Verify(broker =>
      broker.LogError(It.Is(SameExceptionAs(
          expectedStudentValidationException))),
              Times.Once);

  this.storageBrokerMock.VerifyNoOtherCalls();
  this.loggingBrokerMock.VerifyNoOtherCalls();
  ```

- **test-033** Validation and exception behaviors MUST be kept local and explicit — no shared
  assertion helpers across tests.
- **test-036** Exception equality MUST use the `SameExceptionAs` expression helper (backed by
  `Xeption.SameExceptionAs()`).

---

## Test Implementation Order

### Foundation Service — Happy Path First (test-010 to test-016)

Tests MUST be written and committed in this order:

1. Happy path — `ShouldAdd{Entity}Async`
2. Structural validations — null entity (`ShouldThrowValidationExceptionOnAddIf{Entity}IsNullAndLogItAsync`)
3. Logical validations — invalid fields (`ShouldThrowValidationExceptionOnAddIf{Entity}IsInvalidAndLogItAsync`)
4. Dependency validation exceptions — `BadRequest` → `Conflict`
5. Critical dependency exceptions — `Unauthorized` → `Forbidden` → `NotFound` → `UrlNotFound`
6. Non-critical dependency exceptions — `InternalServerError` → `ServiceUnavailable`
7. Transport exception — `HttpRequestException`
8. Catch-all service exception — `Exception`

For storage-based services replace steps 4–7 with: `DuplicateKeyException`, `DbUpdateException`,
`SqlException`.

### Exact Foundation Add Order (test-110)

```
0.  ShouldAdd{Entity}Async
1.  ShouldThrowValidationExceptionOnAddIf{Entity}IsNullAndLogItAsync
2.  ShouldThrowValidationExceptionOnAddIf{Entity}IsInvalidAndLogItAsync
3.  ShouldThrowValidationExceptionOnAddIf{Entity}HasInvalidLengthPropertiesAndLogItAsync  (if applicable)
4.  ShouldThrowDependencyValidationExceptionOnAddIfBadRequestErrorOccursAndLogItAsync
5.  ShouldThrowDependencyValidationExceptionOnAddIfConflictErrorOccursAndLogItAsync
6.  ShouldThrowCriticalDependencyExceptionOnAddIfUnauthorizedErrorOccursAndLogItAsync
7.  ShouldThrowCriticalDependencyExceptionOnAddIfForbiddenErrorOccursAndLogItAsync
8.  ShouldThrowCriticalDependencyExceptionOnAddIfNotFoundErrorOccursAndLogItAsync
9.  ShouldThrowCriticalDependencyExceptionOnAddIfUrlNotFoundErrorOccursAndLogItAsync
10. ShouldThrowDependencyExceptionOnAddIfInternalServerErrorOccursAndLogItAsync
11. ShouldThrowDependencyExceptionOnAddIfServiceUnavailableErrorOccursAndLogItAsync
12. ShouldThrowCriticalDependencyExceptionOnAddIfHttpRequestErrorOccursAndLogItAsync
13. ShouldThrowServiceExceptionOnAddIfServiceErrorOccursAndLogItAsync
```

---

## Validation Test Rules

- **test-017** Tests MUST verify input parameters are validated before any business logic executes.
- **test-019** Validation order MUST be: structural → logical → external → dependency.
- **test-020** Structural validations are circuit-breaking: a null entity throws immediately — no
  further fields are checked.
- **test-021** Continuous validations collect ALL invalid fields before throwing — never throw on
  the first failure when multiple fields can be invalid independently.
- **test-025** Validation tests MUST prove that deterministic failures are caught before reaching
  storage or external dependencies.
- **test-026** Validation tests MUST NOT rely on the database to enforce required, length, or
  format constraints.
- **test-037** Foundation services are the primary validation boundary.
- **test-038** Foundation validation MUST be equal to or stricter than storage constraints.
- **test-039** Foundation tests MUST cover required fields, length, format, and all
  persistence constraints.

---

## Exception Testing (test-111 to test-120)

- **test-111** Tests MUST verify that native exceptions (SQL, HTTP, SDK) are localised into custom
  exceptions before leaving the service boundary.
- **test-112** Tests MUST verify the native exception is preserved as the `InnerException` of the
  localised exception.
- **test-113** Tests MUST verify the `Data` collection is copied from the native exception to the
  local exception.
- **test-115** Tests MUST verify that unknown exceptions are mapped to `ServiceException`.
- **test-116** Tests MUST verify logging occurs before the exception is thrown.
- **test-117** Exception handling MUST use a centralised `TryCatch` pattern per service.
- **test-118** Validation failures MUST result in a `{Entity}ValidationException`.
- **test-119** Validation exceptions MUST include aggregated error details in the `Data` dictionary.

**Exception structure (Foundation):**

```csharp
// native exception → local exception → categorical exception
var httpResponseBadRequestException = new HttpResponseBadRequestException();

var invalidStudentException = new InvalidStudentException(
    message: "Invalid student error occurred, fix errors and try again.",
    innerException: httpResponseBadRequestException,
    data: httpResponseBadRequestException.Data);

var expectedStudentDependencyValidationException =
    new StudentDependencyValidationException(
        message: "Student dependency validation error occurred, fix the errors.",
        innerException: invalidStudentException);
```

---

## Exception Rewrapping by Layer (test-121 to test-124)

From the processing service layer upwards:

- **test-121/122** `{Entity}ValidationException` and `{Entity}DependencyValidationException` from
  dependencies MUST be rewrapped as `{Entity}{Layer}DependencyValidationException`.
- **test-123/124** `{Entity}DependencyException` and `{Entity}ServiceException` from dependencies
  MUST be rewrapped as `{Entity}{Layer}DependencyException`.

Processing and orchestration exception tests SHOULD use `[Theory]` to cover multiple dependency
exception types in a single test method — avoid duplicating near-identical tests.

---

## Service-Layer Specific Rules

### Foundation (test-037 to test-039)
- Primary validation boundary — enforce all deterministic constraints.
- Validate required fields, max/min length, format, and all constraints that could cause
  persistence failure.

### Processing (test-040 to test-049)
- **test-040** Test higher-order logic, not primitive broker details.
- **test-041** Validate ONLY fields the processing service actually uses — do not re-validate
  full entity constraints already covered by foundation.
- **test-042** Test shifter operations: entity → bool, entity → count.
- **test-043** Test combination operations: retrieve+add, retrieve+modify, upsert, ensure-exists.

### Orchestration (test-050 to test-058)
- **test-051** Test call order when the flow depends on it.
- **test-052** Prefer natural order (encoded by input/output shape) over mock-sequence assertions.
- **test-054** Perform structural validation only — no full entity validation.
- **test-057** Delegate full validation to downstream services.

### Aggregation (test-060 to test-065)
- **test-060** MUST NOT test dependency call order.
- **test-061** MUST NOT use mock-sequence style order assertions.
- **test-062** Test only basic structural validations and exposure-level behavior.
- **test-065** MUST NOT perform business or domain validation.

---

## Controller Tests (test-070 to test-075)

- **test-070** Unit-test every success code mapping: `200` for GET/PUT/DELETE, `201` for POST.
- **test-071** Unit-test every error mapping:
  `ValidationException` → `400`, `DependencyValidationException` → `400`,
  `CriticalDependencyException` → `500`, `DependencyException` → `500`,
  `ServiceException` → `500`.
- **test-072** Unit-test authorization and authentication failure mappings.
- **test-073** Acceptance-test every endpoint.
- **test-074** Clean up all test data after each acceptance test run.
- **test-075** Emulate external resources not owned by the microservice in acceptance tests.

---

## Key Libraries

| Package | Role |
|---|---|
| `xunit` | Test framework — `[Fact]` / `[Theory]` |
| `Moq` | Mock all dependencies |
| `FluentAssertions` | `actual.Should().BeEquivalentTo(expected)` |
| `Xeption` | Enhanced exceptions + `SameExceptionAs` helper |
| `DeepCloner` | `obj.DeepClone()` for state isolation |
| `Tynamix.ObjectFiller` | `Filler<Student>` randomized data generation |
| `EFxceptions` | EF Core exception wrappers (storage-based services) |
| `RESTFulSense` | HTTP exception wrappers (API-based services) |
