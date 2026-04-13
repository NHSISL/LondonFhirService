---
applyTo: "**/*.cs"
description: "C# coding conventions aligned with The Standard."
---

# C# Coding Conventions — The Standard

Enforce all rules below on every `.cs` file during code generation and review.
Line-length (120 chars), blank-line separation, continuation detection, and argument
wrapping are governed by `copilot-instructions.md` and apply here too.

---

## Files

- **cs-001** File names MUST use PascalCase and end with `.cs` — `Student.cs`, `StudentService.cs`.
- **cs-002** Partial class files MUST use dot-notation — `StudentService.Validations.cs`,
  `StudentService.Exceptions.cs`, `StudentService.Validations.Add.cs`.
- **cs-003** Partial class files MUST NOT use concatenation — `StudentServiceValidations.cs` is forbidden.
- **cs-004** Partial class files MUST NOT use underscores — `StudentService_Validations.cs` is forbidden.

---

## Variables — Naming

- **cs-010** Variable names MUST be full and descriptive. Single letters and abbreviations are forbidden.

  ```csharp
  // ✅
  var student = new Student();

  // ❌
  var s = new Student();
  var stdnt = new Student();
  ```

- **cs-011** Lambda parameter names MUST be full — never single letters.

  ```csharp
  // ✅
  students.Where(student => student.IsActive)

  // ❌
  students.Where(s => s.IsActive)
  ```

- **cs-012** Collections MUST use natural plural — never `List`, `Collection`, or `Array` suffix.

  ```csharp
  // ✅
  var students = new List<Student>();

  // ❌
  var studentList = new List<Student>();
  var studentCollection = new List<Student>();
  ```

- **cs-013** Variable names MUST NOT carry a type suffix.

  ```csharp
  // ✅
  var student = new Student();

  // ❌
  var studentModel = new Student();
  var studentObj = new Student();
  var studentEntity = new Student();
  ```

- **cs-014** Variables holding `null` or a default value MUST signal that intent in their name.

  ```csharp
  // ✅
  Student noStudent = null;

  // ❌
  Student student = null;
  ```

- **cs-015** Zero-value numeric variables MUST signal intent.

  ```csharp
  // ✅
  int noChangeCount = 0;

  // ❌
  int changeCount = 0;
  ```

---

## Variables — Declarations

- **cs-020** When the right-side type is explicit (e.g. `new Student()`), use `var`.

  ```csharp
  // ✅
  var student = new Student();

  // ❌
  Student student = new Student();
  ```

- **cs-021** When the right-side type is NOT visible from the expression (e.g. a method call),
  declare with the explicit type.

  ```csharp
  // ✅
  Student student = GetStudent();

  // ❌
  var student = GetStudent();
  ```

- **cs-022** Anonymous types require `var`.

- **cs-023** Single-property object initialisation: assign the property on the line after construction.
  Multi-property objects: use an initializer block.

  ```csharp
  // ✅ single property
  var inputStudentEvent = new StudentEvent();
  inputStudentEvent.Student = inputProcessedStudent;

  // ❌ single property
  var inputStudentEvent = new StudentEvent { Student = inputProcessedStudent };

  // ✅ multiple properties
  var studentEvent = new StudentEvent
  {
      Student = someStudent,
      Date = someDate
  };

  // ❌ multiple properties
  var studentEvent = new StudentEvent();
  studentEvent.Student = someStudent;
  studentEvent.Date = someDate;
  ```

---

## Variables — Organization

- **cs-030** A declaration that exceeds 120 characters MUST break after the `=` sign.

  ```csharp
  // ✅
  List<Student> washingtonSchoolsStudentsWithGrades =
      await GetAllWashingtonSchoolsStudentsWithTheirGradesAsync();

  // ❌
  List<Student> washingtonSchoolsStudentsWithGrades = await GetAllWashingtonSchoolsStudentsWithTheirGradesAsync();
  ```

- **cs-031** A multi-line declaration MUST have a blank line before AND after it.

  ```csharp
  // ✅
  Student student = GetStudent();

  List<Student> washingtonSchoolsStudentsWithGrades =
      await GetAllWashingtonSchoolsStudentsWithTheirGradesAsync();

  School school = await GetSchoolAsync();
  ```

- **cs-032** Consecutive single-line declarations MUST NOT have blank lines between them.

  ```csharp
  // ✅
  Student student = GetStudent();
  School school = await GetSchoolAsync();

  // ❌
  Student student = GetStudent();

  School school = await GetSchoolAsync();
  ```

---

## Methods — Naming

- **cs-040** Method names MUST contain a verb.

  ```csharp
  // ✅
  public List<Student> GetStudents() { ... }

  // ❌
  public List<Student> Students() { ... }
  ```

- **cs-041** Async methods MUST end with the `Async` suffix.

  ```csharp
  // ✅
  public async ValueTask<List<Student>> GetStudentsAsync() { ... }

  // ❌
  public async ValueTask<List<Student>> GetStudents() { ... }
  ```

- **cs-042** Input parameter names MUST be fully qualified — never generic names like `name`,
  `text`, or `value`.

  ```csharp
  // ✅
  public ValueTask<Student> GetStudentByNameAsync(string studentName) { ... }

  // ❌
  public ValueTask<Student> GetStudentByNameAsync(string name) { ... }
  public ValueTask<Student> GetStudentByNameAsync(string text) { ... }
  ```

- **cs-043** When a method acts on a specific property, the parameter MUST identify it.

  ```csharp
  // ✅
  public ValueTask<Student> GetStudentByIdAsync(Guid studentId) { ... }

  // ❌
  public ValueTask<Student> GetStudentAsync(Guid studentId) { ... }
  ```

- **cs-044** When calling a method: if the variable name matches the parameter alias (fully or
  partially), no alias is needed. When passing a literal or a non-matching variable, the alias
  is required.

  ```csharp
  // ✅ — variable name matches
  Student student = await GetStudentByNameAsync(studentName);

  // ✅ — literal requires alias
  Student student = await GetStudentByNameAsync(studentName: "Todd");

  // ❌ — literal without alias
  Student student = await GetStudentByNameAsync("Todd");
  ```

---

## Methods — Organization

- **cs-050** A method with exactly one line of code MUST use fat-arrow syntax.

  ```csharp
  // ✅
  public List<Student> GetStudents() => this.storageBroker.GetStudents();

  // ❌
  public List<Student> GetStudents()
  {
      return this.storageBroker.GetStudents();
  }
  ```

- **cs-051** A method with multiple lines of code MUST use a scope body `{ }`.

- **cs-052** A fat-arrow one-liner that exceeds 120 characters MUST break after `=>` with one
  additional level of indentation.

  ```csharp
  // ✅
  public async ValueTask<List<Student>> GetAllWashingtonSchoolsStudentsAsync() =>
      await this.storageBroker.GetStudentsAsync();
  ```

- **cs-053** A multi-liner with chaining MUST use a scope body — not fat arrow.

  ```csharp
  // ✅
  public Student AddStudent(Student student)
  {
      return this.storageBroker.InsertStudent(student)
          .WithLogging();
  }

  // ❌
  public Student AddStudent(Student student) =>
      this.storageBroker.InsertStudent(student)
          .WithLogging();
  ```

- **cs-054** Multi-line methods MUST have a blank line between the last logic statement and the
  `return` statement.

  ```csharp
  // ✅
  public List<Student> GetStudents()
  {
      StudentsClient studentsApiClient = InitializeStudentApiClient();

      return studentsApiClient.GetStudents();
  }

  // ❌
  public List<Student> GetStudents()
  {
      StudentsClient studentsApiClient = InitializeStudentApiClient();
      return studentsApiClient.GetStudents();
  }
  ```

- **cs-055** Multiple consecutive calls that are each under 120 characters MAY stack without
  blank lines, unless the final call is the `return` statement.

- **cs-056** If any call in a sequence exceeds 120 characters, it MUST be separated from adjacent
  calls with a blank line.

  ```csharp
  // ✅
  public async ValueTask<List<Student>> GetStudentsAsync()
  {
      StudentsClient washingtonSchoolsStudentsApiClient =
          await InitializeWashingtonSchoolsStudentsApiClientAsync();

      List<Student> students = studentsApiClient.GetStudents();

      return students;
  }
  ```

- **cs-057** A method declaration exceeding 120 characters MUST break parameters onto the next line.

  ```csharp
  // ✅
  public async ValueTask<List<Student>> GetAllRegisteredWashingtonSchoolsStudentsAsync(
      StudentsQuery studentsQuery)
  { ... }

  // ❌
  public async ValueTask<List<Student>> GetAllRegisteredWashingtonSchoolsStudentsAsync(StudentsQuery studentsQuery)
  { ... }
  ```

- **cs-058** When parameters are broken onto new lines, each parameter MUST be on its own line.

  ```csharp
  // ✅
  List<Student> redmondHighStudents = await QueryAllWashingtonStudentsByScoreAndSchoolAsync(
      minimumScore: 130,
      schoolName: "Redmond High");

  // ❌
  List<Student> redmondHighStudents = await QueryAllWashingtonStudentsByScoreAndSchoolAsync(
      minimumScore: 130, schoolName: "Redmond High");
  ```

- **cs-059** Method chaining (uglification-beautification): the first call goes on the same line
  as the subject; each subsequent chained call is indented one additional level.

  ```csharp
  // ✅
  students.Where(student => student.Name is "Elbek")
      .Select(student => student.Name)
          .ToList();

  // ❌ — subject on its own line
  students
      .Where(student => student.Name is "Elbek")
      .Select(student => student.Name)
      .ToList();
  ```

---

## Classes — Naming

- **cs-060** Model class names carry NO type suffix.

  ```csharp
  // ✅
  class Student { }

  // ❌
  class StudentModel { }
  class StudentDTO { }
  class StudentEntity { }
  ```

- **cs-061** Service class names are singular PascalCase + `Service`.

  ```csharp
  // ✅
  class StudentService { }

  // ❌
  class StudentsService { }
  class StudentBL { }
  ```

- **cs-062** Broker class names are singular PascalCase + `Broker`.

  ```csharp
  // ✅
  class StorageBroker { }

  // ❌
  class StorageBrokers { }
  ```

- **cs-063** Controller class names are PLURAL PascalCase + `Controller`.

  ```csharp
  // ✅
  class StudentsController { }

  // ❌
  class StudentController { }
  ```

---

## Classes — Fields

- **cs-070** Class fields MUST be named in camelCase — no PascalCase, no underscore prefix.

  ```csharp
  // ✅
  private readonly string studentName;

  // ❌
  private readonly string StudentName;
  private readonly string _studentName;
  ```

- **cs-071** Class fields MUST follow the same naming rules as variables: descriptive, no
  abbreviation, no type suffix.

- **cs-072** Private class fields MUST be referenced using `this.`.

  ```csharp
  // ✅
  this.studentName = studentName;

  // ❌
  _studentName = studentName;
  studentName = studentName;
  ```

---

## Classes — Instantiation

- **cs-080** When passing literals directly to a constructor or method, named aliases are required.

  ```csharp
  // ✅
  var student = new Student(name: "Josh", score: 150);

  // ❌
  var student = new Student("Josh", 150);
  ```

- **cs-081** When variable names match the constructor parameter aliases (fully or partially),
  aliases are not required.

  ```csharp
  // ✅ — variables already named name and score
  var student = new Student(name, score);
  ```

- **cs-082** Target-typed `new()` is forbidden.

  ```csharp
  // ❌
  Student student = new(...);
  ```

- **cs-083** Property assignment order in an initializer MUST match the order properties are
  declared in the class.

  ```csharp
  // class declares: Id, then Name
  // ✅
  var student = new Student { Id = Guid.NewGuid(), Name = "Elbek" };

  // ❌
  var student = new Student { Name = "Elbek", Id = Guid.NewGuid() };
  ```

---

## Comments

- **cs-090** Copyright headers MUST use the exact Standard format.

  ```csharp
  // ✅
  // ---------------------------------------------------------------
  // Copyright (c) Coalition of the Good-Hearted Engineers
  // FREE TO USE TO CONNECT THE WORLD
  // ---------------------------------------------------------------

  // ❌ — XML style
  // <copyright file="StudentService.cs" company="OpenSource">

  // ❌ — block comment
  /* Copyright (c) Coalition of the Good-Hearted Engineers */
  ```

- **cs-091** XML-style copyright comments (`<copyright>`) are forbidden.
- **cs-092** Block comment copyright (`/* ... */`) is forbidden.
- **cs-093** [WARNING] Methods whose implementation is not visible at dev-time SHOULD document:
  Purpose, Incomes, Outcomes, Side Effects.

---

## Naming Conventions — Implementation Profile

| Element | Pattern | Example |
|---|---|---|
| Broker interface | `I{Resource}Broker` | `IStorageBroker`, `ILoggingBroker` |
| Broker class | `{Resource}Broker` | `StorageBroker`, `LoggingBroker` |
| Broker method | `{Action}{Entity}Async` | `InsertStudentAsync`, `SelectStudentByIdAsync` |
| Service interface | `I{Entity}Service` | `IStudentService` |
| Service class | `{Entity}Service` | `StudentService` |
| Service method | `{Action}{Entity}Async` | `AddStudentAsync`, `RemoveStudentByIdAsync` |
| Inner exception | `{Adjective}{Entity}Exception` | `NullStudentException`, `InvalidStudentException` |
| Outer exception | `{Entity}{Category}Exception` | `StudentValidationException`, `StudentDependencyException` |
| Test class | `{Entity}ServiceTests` | `StudentServiceTests` |
| Test method (success) | `Should{Action}Async` | `ShouldAddStudentAsync` |
| Test method (failure) | `ShouldThrow{Exception}On{Action}If{Condition}AndLogItAsync` | `ShouldThrowValidationExceptionOnAddIfStudentIsNullAndLogItAsync` |
