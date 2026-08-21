// ---
// skill: the-standard-csharp-methods
// type: example
// source-section: "1. Methods — 1.0 Naming, 1.1 Organization"
// demonstrates: "tsc-csharp-methods-001 through tsc-csharp-methods-012"
// ---

// ❌ No verb in method name — violates tsc-csharp-methods-001
public List<Student> Students() { }

// ❌ Missing Async suffix — violates tsc-csharp-methods-002
public async ValueTask<List<Student>> GetStudents() { }

// ❌ Generic parameter name — violates tsc-csharp-methods-003
public async ValueTask<Student> GetStudentByNameAsync(string name) { }

// ❌ Generic id parameter — violates tsc-csharp-methods-004
public async ValueTask<Student> GetStudentAsync(Guid studentId) { } // method name doesn't say ById

// ❌ Unnamed literal — violates tsc-csharp-methods-005
var student = await GetStudentByNameAsync("Todd");

// ❌ Scoped block for one-liner — violates tsc-csharp-methods-006
public List<Student> GetStudents()
{
    return this.storageBroker.GetStudents();
}

// ❌ Fat-arrow for multi-liner chain — violates tsc-csharp-methods-008
public Student AddStudent(Student student) =>
    this.storageBroker.InsertStudent(student)
        .WithLogging();

// ❌ No blank line before return — violates tsc-csharp-methods-009
public List<Student> GetStudents()
{
    StudentsClient studentsApiClient = InitializeStudentApiClient();
    return studentsApiClient.GetStudents();
}

// ❌ Declaration over 120 chars not broken — violates tsc-csharp-methods-011
public async ValueTask<List<Student>> GetAllRegisteredWashingtonSchoolsStudentsAsync(StudentsQuery studentsQuery) { }

// ❌ No uglification — flat chain — violates tsc-csharp-methods-012
students
.Where(student => student.Name is "Elbek")
.Select(student => student.Name)
.ToList();
