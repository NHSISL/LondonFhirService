---
applyTo: "**/*.cs"
description: "Architecture conventions for brokers, services, exposers, and dependency flow."
---

# Architecture Conventions — The Standard

These rules govern how brokers, services, exposers, and models are structured and connected.
Core design principles are in `core.instructions.md`.
C# naming and formatting rules are in `csharp.instructions.md`.
Test conventions are in `testing.instructions.md`.

---

## Dependency Flow

The dependency flow is strictly one direction and MUST NOT be reversed:

```
Exposer -> Service(s) -> Broker(s) -> External Resource
```

- **arch-082** Dependency flows forward only. Never reverse the direction.
- **arch-080** Services MUST NOT call other services at the same layer.
- **arch-081** Services MUST NOT call infrastructure directly -- only through brokers.

---

## Folder Structure

```
Models/Foundations/{EntityPlural}/{Entity}.cs
Models/Foundations/{EntityPlural}/Exceptions/
Models/Processings/{EntityPlural}/{Entity}.cs
Models/Processings/{EntityPlural}/Exceptions/
Models/Orchestrations/{EntityPlural}/Exceptions/
Models/Aggregations/{EntityPlural}/Exceptions/

Brokers/Storages/IStorageBroker.cs
Brokers/Storages/IStorageBroker.{Entities}.cs
Brokers/Storages/StorageBroker.cs
Brokers/Storages/StorageBroker.{Entities}.cs
Brokers/Apis/IModernApiBroker.cs
Brokers/Apis/IModernApiBroker.{Entities}.cs
Brokers/Apis/ModernApiBroker.cs
Brokers/Apis/ModernApiBroker.{Entities}.cs
Brokers/Loggings/ILoggingBroker.cs
Brokers/Loggings/LoggingBroker.cs

Services/Foundations/{EntityPlural}/I{Entity}Service.cs
Services/Foundations/{EntityPlural}/{Entity}Service.cs
Services/Foundations/{EntityPlural}/{Entity}Service.Validations.cs
Services/Foundations/{EntityPlural}/{Entity}Service.Exceptions.cs
Services/Processings/{EntityPlural}/I{Entity}ProcessingService.cs
Services/Processings/{EntityPlural}/{Entity}ProcessingService.cs
Services/Orchestrations/{EntityPlural}/I{Entity}OrchestrationService.cs
Services/Orchestrations/{EntityPlural}/{Entity}OrchestrationService.cs
Services/Aggregations/{EntityPlural}/I{Entity}AggregationService.cs
Services/Aggregations/{EntityPlural}/{Entity}AggregationService.cs

Controllers/{Entities}Controller.cs
```

---

## Brokers (arch-001 to arch-010)

Brokers are thin wrappers over external resources. They contain zero business logic.

- **arch-001** Every broker MUST implement a local interface (`IStorageBroker`, `IModernApiBroker`).
- **arch-002** Brokers MUST contain no flow control -- no `if`, `switch`, `for`, or `while`.

  ```csharp
  // Correct
  public async ValueTask<Student> InsertStudentAsync(Student student) =>
      await this.InsertAsync(student);

  // Wrong -- flow control inside a broker
  public async ValueTask<Student> InsertStudentAsync(Student student)
  {
      if (student != null)
          return await this.InsertAsync(student);

      return null;
  }
  ```

- **arch-003** Brokers MUST NOT handle exceptions. Native exceptions propagate to services.
- **arch-004** Brokers own their own configuration -- connection strings and credentials are injected into the broker only.
- **arch-006** Brokers MUST use infrastructure language, not business language.

  | Business (Service) | Infrastructure (Broker) |
  |---|---|
  | `AddStudentAsync` | `InsertStudentAsync` |
  | `RetrieveStudentByIdAsync` | `SelectStudentByIdAsync` |
  | `ModifyStudentAsync` | `UpdateStudentAsync` |
  | `RemoveStudentByIdAsync` | `DeleteStudentAsync` |

- **arch-007** Brokers communicate upward to services and sideways to support brokers only. Never to other entity brokers.
- **arch-008** One broker wraps exactly one external resource.
- **arch-009** All broker methods MUST be asynchronous (`ValueTask` or `ValueTask<T>`).
- **arch-010** Entity brokers handle entity CRUD. Support brokers (Logging, DateTime, Identifier) provide cross-cutting infrastructure.

### Broker Base Partial vs Entity Partial

The **base partial** (`StorageBroker.cs`) owns: configuration, constructor, and private generic CRUD helpers.
The **entity partial** (`StorageBroker.Students.cs`) owns: `DbSet<T>` and entity-specific methods.
Entity partials MUST delegate to the generic helpers -- never reference the underlying client directly.

  ```csharp
  // Correct -- entity partial delegates to generic helper
  public async ValueTask<Student> InsertStudentAsync(Student student) =>
      await this.InsertAsync(student);

  // Wrong -- entity partial touches EF context directly
  public async ValueTask<Student> InsertStudentAsync(Student student)
  {
      this.Entry(student).State = EntityState.Added;
      await this.SaveChangesAsync();
      return student;
  }
  ```

`DbSet<T>` declarations belong in the entity partial, NOT the base partial:

  ```csharp
  // Correct -- in StorageBroker.Students.cs
  public DbSet<Student> Students { get; set; }

  // Wrong -- in StorageBroker.cs (base partial must not declare entity members)
  public DbSet<Student> Students { get; set; }
  ```

### Asynchronization Abstraction

Every publicly exposed interface method on brokers, services, and exposers MUST return
`ValueTask` or `ValueTask<T>`, even when the current implementation does not internally await.

  ```csharp
  // Correct -- async keyword, direct call
  public async ValueTask LogWarningAsync(string message) =>
      this.logger.LogWarning(message);

  // Correct -- ValueTask.FromResult wraps a synchronous result
  public ValueTask<IQueryable<Student>> SelectAllStudentsAsync() =>
      ValueTask.FromResult<IQueryable<Student>>(this.Set<Student>().AsNoTracking());

  // Wrong -- synchronous, no ValueTask
  public IQueryable<Student> SelectAllStudents() =>
      this.Students.AsNoTracking();

  // Wrong -- Task.Run wraps a synchronous call needlessly
  public ValueTask LogWarningAsync(string message) =>
      new ValueTask(Task.Run(() => this.logger.LogWarning(message)));
  ```

---

## Foundation Services (arch-020 to arch-032)

Foundation services sit directly above brokers and are their only consumer.

- **arch-020** Foundation services are pure-primitive: input and output are always the same entity type.
- **arch-021** Foundation services integrate with exactly one entity broker.
- **arch-022** Foundation services use business language: `Add`, `Retrieve`, `Modify`, `Remove`.
- **arch-023** Foundation services MUST validate all inputs before delegating to a broker.
- **arch-031** Foundation services MUST verify no unwanted broker calls occur.

### Partial Class Layout

| File | Concern |
|---|---|
| `{Entity}Service.cs` | Constructor, DI fields, public business methods |
| `{Entity}Service.Validations.cs` | All `Validate*` and `IsInvalid*` methods |
| `{Entity}Service.Exceptions.cs` | `TryCatch` delegate, `CreateAndLog*` helpers |

### TryCatch Pattern

Every public method MUST delegate to a `TryCatch` wrapper in the Exceptions partial:

```csharp
// Correct
public ValueTask<Student> AddStudentAsync(Student student) =>
    TryCatch(async () =>
    {
        ValidateStudent(student);

        return await this.storageBroker.InsertStudentAsync(student);
    });
```

### Validation Order (arch-024 to arch-027)

Validation MUST run in this exact order:

1. **Structural** -- null, empty, default values -- circuit-breaking (throw immediately on first failure)
2. **Logical** -- business rules (date ranges, valid states) -- continuous (collect all errors first)
3. **External** -- existence checks in storage (required for Modify/Remove)
4. **Dependency** -- broker-specific failure conditions

### Continuous Validation Pattern (arch-028)

```csharp
private void ValidateStudent(Student student)
{
    ValidateStudentIsNotNull(student);

    Validate(
        (Rule: IsInvalid(student.Id),           Parameter: nameof(Student.Id)),
        (Rule: IsInvalid(student.Name),         Parameter: nameof(Student.Name)),
        (Rule: IsInvalid(student.CreatedDate),  Parameter: nameof(Student.CreatedDate)));
}
```

### Exception Categories (arch-029, arch-030)

| Category | Exception Type | Log Level |
|---|---|---|
| Validation | `{Entity}ValidationException` | LogError |
| Dependency Validation | `{Entity}DependencyValidationException` | LogError |
| Critical Dependency | `{Entity}DependencyException` | LogCritical |
| Dependency | `{Entity}DependencyException` | LogError |
| Service | `{Entity}ServiceException` | LogError |

### Storage-Based Exception Flow

```
Native Exception          ->  Inner (Local) Exception          ->  Outer (Categorical) Exception
null input                ->  NullStudentException             ->  StudentValidationException
validation rules fail     ->  InvalidStudentException          ->  StudentValidationException
DuplicateKeyException     ->  AlreadyExistsStudentException    ->  StudentDependencyValidationException
DbUpdateException         ->  InvalidStudentException          ->  StudentDependencyValidationException
SqlException              ->  FailedStorageStudentException    ->  StudentDependencyException (Critical)
Exception (catch-all)     ->  FailedStudentServiceException    ->  StudentServiceException
```

### API-Based Exception Flow

```
Native Exception                         ->  Inner Exception                  ->  Category             ->  Log Level
HttpResponseBadRequestException          ->  InvalidStudentException          ->  DependencyValidation ->  LogError
HttpResponseConflictException            ->  AlreadyExistsStudentException    ->  DependencyValidation ->  LogError
HttpResponseUnauthorizedException        ->  FailedStudentDependencyException ->  Dependency           ->  LogCritical
HttpResponseForbiddenException           ->  FailedStudentDependencyException ->  Dependency           ->  LogCritical
HttpResponseNotFoundException            ->  FailedStudentDependencyException ->  Dependency           ->  LogCritical
HttpResponseInternalServerErrorException ->  FailedStudentDependencyException ->  Dependency           ->  LogError
HttpRequestException                     ->  FailedStudentDependencyException ->  Dependency           ->  LogCritical
Exception (catch-all)                    ->  FailedStudentServiceException    ->  Service              ->  LogError
```

### CreateAndLog Helpers MUST Be Async

`ILoggingBroker` exposes only async (`ValueTask`) log methods.
All `CreateAndLog*` helpers MUST be async and call sites MUST use `throw await`:

```csharp
// Correct
private async ValueTask<StudentValidationException> CreateAndLogValidationException(
    Xeption exception)
{
    var studentValidationException =
        new StudentValidationException(
            message: "Student validation error occurred, fix errors and try again.",
            innerException: exception);

    await this.loggingBroker.LogErrorAsync(studentValidationException);

    return studentValidationException;
}

// Correct -- call site uses throw await
catch (NullStudentException nullStudentException)
{
    throw await CreateAndLogValidationException(nullStudentException);
}

// Wrong -- synchronous helper (LogError does not exist on ILoggingBroker)
private StudentValidationException CreateAndLogValidationException(Xeption exception)
{
    this.loggingBroker.LogError(studentValidationException);  // no sync method
    return studentValidationException;
}
```

---

## Processing Services (arch-040 to arch-046)

- **arch-040** Processing services depend on exactly one foundation service.
- **arch-041** Processing services validate ONLY the data they actually use -- no over-validation.
- **arch-042** Implement higher-order patterns: `Ensure`, `Upsert`, `TryAdd`, `TryRemove`.
- **arch-043** Implement shifters: entity to bool, entity to count.
- **arch-044** Implement combinations: retrieve+add (EnsureExists), retrieve+modify (Upsert).
- **arch-045** MUST unwrap foundation exceptions and re-wrap as processing-level exceptions.
- **arch-046** Pass-through methods (no added logic) are allowed and delegate directly.

---

## Orchestration Services (arch-050 to arch-055)

- **arch-050** Orchestration services MUST follow the Florance Pattern: 2-3 foundation or processing service dependencies.
- **arch-051** Orchestration services coordinate multi-entity flows across multiple services.
- **arch-052** Call order MUST be enforced explicitly when flow correctness depends on it.
- **arch-053** Natural order (enforced by input/output dependencies) is preferred over mock-sequence verification.
- **arch-054** MUST unwrap and re-wrap exceptions from all dependencies.
- **arch-055** Variations: Coordination (2-3 deps), Management (3+ deps), UberManagement (4+), Unit of Work.

---

## Aggregation Services (arch-060 to arch-064)

- **arch-060** MUST NOT validate dependency call order.
- **arch-061** MUST NOT use mock-sequence style order assertions.
- **arch-062** Perform basic structural validations only -- no business or domain validation.
- **arch-063** May pass-through to multiple service dependencies without ordering constraints.
- **arch-064** MUST aggregate exceptions from all dependencies.

---

## Exposers / Controllers (arch-070 to arch-075)

- **arch-070** Controllers contain NO business logic -- pure mapping only.

  ```csharp
  // Correct -- pure mapping
  [HttpPost]
  public async ValueTask<ActionResult<Student>> PostStudentAsync(Student student)
  {
      Student addedStudent = await this.studentService.AddStudentAsync(student);

      return Created(addedStudent);
  }

  // Wrong -- validation in controller
  [HttpPost]
  public async ValueTask<ActionResult<Student>> PostStudentAsync(Student student)
  {
      if (student.Name == null) return BadRequest();
      ...
  }
  ```

- **arch-071** Each entity has exactly one controller.
- **arch-072** REST routes follow `api/[controller]` -- plural entity name.
- **arch-075** Controllers MUST NOT catch exceptions -- use middleware or problem-details handler.

### HTTP Status Codes (arch-073, arch-074)

| Scenario | Status Code |
|---|---|
| Successful POST | 201 Created |
| Successful GET / PUT / DELETE | 200 OK |
| Validation / DependencyValidation error | 400 Bad Request |
| Not found | 404 Not Found |
| Conflict / Already exists | 409 Conflict |
| Critical / Dependency / Service error | 500 Internal Server Error |

---

## Naming Conventions

| Element | Pattern | Example |
|---|---|---|
| Broker interface | `I{Resource}Broker` | `IStorageBroker`, `ILoggingBroker` |
| Broker class | `{Resource}Broker` | `StorageBroker`, `LoggingBroker` |
| Broker method (storage) | `{Action}{Entity}Async` | `InsertStudentAsync`, `SelectStudentByIdAsync` |
| Service interface | `I{Entity}Service` | `IStudentService` |
| Service class | `{Entity}Service` | `StudentService` |
| Service method | `{Action}{Entity}Async` | `AddStudentAsync`, `RemoveStudentByIdAsync` |
| Inner exception | `{Adjective}{Entity}Exception` | `NullStudentException`, `InvalidStudentException` |
| Outer exception | `{Entity}{Category}Exception` | `StudentValidationException` |
| Controller class | `{Entities}Controller` (plural) | `StudentsController` |
| Controller route | `api/[controller]` | resolves to `api/Students` |
