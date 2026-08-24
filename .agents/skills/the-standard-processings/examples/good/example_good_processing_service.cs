// ---
// skill: the-standard-processings
// type: example
// source-section: "3. Processing Services"
// demonstrates: "ts-processings-001, ts-processings-003, ts-processings-004, ts-processings-005"
// ---

// ---------------------------------------------------------------
// Copyright (c) Coalition of the Good-Hearted Engineers
// FREE TO USE TO CONNECT THE WORLD
// ---------------------------------------------------------------

public interface IStudentProcessingService
{
    ValueTask<Student> EnsureStudentExistsAsync(Student student);
    ValueTask<Student> UpsertStudentAsync(Student student);
}

public partial class StudentProcessingService : IStudentProcessingService
{
    private readonly IStudentService studentService; // ✅ calls foundation service only

    public StudentProcessingService(IStudentService studentService) =>
        this.studentService = studentService;
}

public partial class StudentProcessingService
{
    // ✅ Higher-order operation: try-retrieve, if missing then add
    public async ValueTask<Student> EnsureStudentExistsAsync(Student student)
    {
        try
        {
            Student maybeStudent =
                await this.studentService.RetrieveStudentByIdAsync(student.Id);

            return maybeStudent ?? await this.studentService.AddStudentAsync(student);
        }
        catch (StudentDependencyException studentDependencyException)
        {
            throw new StudentProcessingDependencyException(studentDependencyException);
        }
        catch (StudentServiceException studentServiceException)
        {
            throw new StudentProcessingServiceException(studentServiceException);
        }
    }

    // ✅ Higher-order operation: add or update
    public async ValueTask<Student> UpsertStudentAsync(Student student)
    {
        try
        {
            IQueryable<Student> allStudents = this.studentService.RetrieveAllStudents();

            bool studentExists = allStudents.Any(s => s.Id == student.Id);

            return studentExists
                ? await this.studentService.ModifyStudentAsync(student)
                : await this.studentService.AddStudentAsync(student);
        }
        catch (StudentDependencyException studentDependencyException)
        {
            throw new StudentProcessingDependencyException(studentDependencyException);
        }
        catch (StudentServiceException studentServiceException)
        {
            throw new StudentProcessingServiceException(studentServiceException);
        }
    }
}
