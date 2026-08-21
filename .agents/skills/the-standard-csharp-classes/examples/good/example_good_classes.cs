// ---
// skill: the-standard-csharp-classes
// type: example
// source-section: "4. Classes — 4.0 Naming, 4.1 Fields, 4.2 Instantiations"
// demonstrates: "tsc-csharp-classes-001 through tsc-csharp-classes-012"
// ---

// ✅ Model — no suffix
public class Student { }

// ✅ Service — singular
public class StudentService : IStudentService { }

// ✅ Broker — singular
public class StudentBroker : IStudentBroker { }

// ✅ Controller — plural
public class StudentsController : RESTFulController { }

// ✅ Fields — camelCase, referenced with this.
public class StudentsController
{
    private readonly IStudentService studentService;

    public StudentsController(IStudentService studentService) =>
        this.studentService = studentService;
}

// ✅ Instantiation — var, named params, property order matches declaration
public class Student
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}

var student = new Student
{
    Id = Guid.NewGuid(),
    Name = "Elbek"
};

// ✅ Instantiation — named params when variable names differ from params
int score = 150;
string name = "Josh";
var student2 = new Student(name: name, score: score);
