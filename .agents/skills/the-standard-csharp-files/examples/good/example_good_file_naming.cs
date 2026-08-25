// ---
// skill: the-standard-csharp-files
// type: example
// source-section: "0. Files — 0.0 Naming"
// demonstrates: "tsc-csharp-files-001, tsc-csharp-files-003"
// ---

// File: Student.cs
public class Student { }

// File: StudentService.cs
public class StudentService { }

// File: StudentService.Validations.cs
public partial class StudentService
{
    private void ValidateStudent(Student student) { }
}

// File: StudentService.Exceptions.cs
public partial class StudentService
{
    private StudentValidationException CreateStudentValidationException(
        Exception innerException) => new StudentValidationException(innerException);
}

// File: StudentService.Validations.Add.cs
public partial class StudentService
{
    private void ValidateStudentOnAdd(Student student) { }
}
