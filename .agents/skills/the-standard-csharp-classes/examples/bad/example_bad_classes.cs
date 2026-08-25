// ---
// skill: the-standard-csharp-classes
// type: example
// source-section: "4. Classes — 4.0 Naming, 4.1 Fields, 4.2 Instantiations"
// demonstrates: "tsc-csharp-classes-001 through tsc-csharp-classes-012"
// ---

// ❌ Model suffix — violates tsc-csharp-classes-001
public class StudentModel { }

// ❌ Plural service — violates tsc-csharp-classes-002
public class StudentsService { }

// ❌ Wrong service suffix — violates tsc-csharp-classes-003
public class StudentBusinessLogic { }
public class StudentBL { }

// ❌ Plural broker — violates tsc-csharp-classes-004
public class StudentsBroker { }

// ❌ Singular controller — violates tsc-csharp-classes-005
public class StudentController { }

// ❌ PascalCase field — violates tsc-csharp-classes-006
private readonly string StudentName;

// ❌ Underscore field prefix — violates tsc-csharp-classes-007
private readonly string _studentName;

// ❌ Missing this. reference — violates tsc-csharp-classes-008
public StudentsController(IStudentService studentService)
{
    studentService = studentService; // assigns to itself — bug!
}

// ❌ Explicit type with target-typed new — violates tsc-csharp-classes-009
Student student = new (...);

// ❌ Unnamed positional literals — violates tsc-csharp-classes-010
var student2 = new Student("Josh", 150);

// ❌ Wrong property initializer order — violates tsc-csharp-classes-011
// Class declares: Id, then Name
var student3 = new Student
{
    Name = "Elbek",  // Name before Id — wrong order
    Id = Guid.NewGuid()
};
