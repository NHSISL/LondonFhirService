// ---
// skill: the-standard-csharp-methods
// type: example
// source-section: "1. Methods — 1.0 Naming, 1.1 Organization"
// demonstrates: "tsc-csharp-methods-001 through tsc-csharp-methods-012"
// ---

// ✅ One-liner with fat arrow (tsc-csharp-methods-006)
public List<Student> GetStudents() => this.storageBroker.GetStudents();

// ✅ One-liner over 120 chars — break after => with extra tab (tsc-csharp-methods-007)
public async ValueTask<List<Student>> GetAllWashingtonSchoolsStudentsAsync() =>
    await this.storageBroker.GetStudentsAsync();

// ✅ Multi-liner with scoped block and blank line before return (tsc-csharp-methods-008, tsc-csharp-methods-009)
public List<Student> GetStudents()
{
    StudentsClient studentsApiClient = InitializeStudentApiClient();

    return studentsApiClient.GetStudents();
}

// ✅ Multiple consecutive calls — stacked (both < 120 chars) (tsc-csharp-methods-010)
public List<Student> GetStudents()
{
    StudentsClient studentsApiClient = InitializeStudentApiClient();
    List<Student> students = studentsApiClient.GetStudents();

    return students;
}

// ✅ Long declaration — one param per line (tsc-csharp-methods-011)
public async ValueTask<List<Student>> GetAllRegisteredWashingtonSchoolsStudentsAsync(
    StudentsQuery studentsQuery)
{
    // ...
}

// ✅ Async naming (tsc-csharp-methods-002)
public async ValueTask<List<Student>> GetStudentsAsync() { }

// ✅ Entity-prefixed parameter names (tsc-csharp-methods-003, tsc-csharp-methods-004)
public async ValueTask<Student> GetStudentByIdAsync(Guid studentId) { }

// ✅ Named parameters when variable name differs (tsc-csharp-methods-005)
var student = await GetStudentByNameAsync(studentName: "Todd");

// ✅ Uglification for chained calls (tsc-csharp-methods-012)
students.Where(student => student.Name is "Elbek")
    .Select(student => student.Name)
        .ToList();
