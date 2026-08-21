// ---
// skill: the-standard-csharp-files
// type: example
// source-section: "0. Files — 0.0 Naming"
// demonstrates: "tsc-csharp-files-001, tsc-csharp-files-002"
// ---

// ❌ File: student.cs — violates tsc-csharp-files-001 (not PascalCase)
public class Student { }

// ❌ File: studentService.cs — violates tsc-csharp-files-002 (camelCase)
public class StudentService { }

// ❌ File: Student_Service.cs — violates tsc-csharp-files-002 (underscore separator)
public class StudentService { }

// ❌ File: StudentService2.cs — violates tsc-csharp-files-003 (missing aspect name for partial)
public partial class StudentService
{
    private void ValidateStudent(Student student) { }
}
