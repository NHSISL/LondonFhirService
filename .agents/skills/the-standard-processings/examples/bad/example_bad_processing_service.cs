// ---
// skill: the-standard-processings
// type: example
// source-section: "3. Processing Services"
// demonstrates: "ts-processings-001 violation, ts-processings-002 violation"
// ---

// ---------------------------------------------------------------
// Copyright (c) Coalition of the Good-Hearted Engineers
// FREE TO USE TO CONNECT THE WORLD
// ---------------------------------------------------------------

// ❌ VIOLATION ts-processings-001: injects and calls a broker directly
public class StudentProcessingService : IStudentProcessingService
{
    private readonly IStudentService studentService;
    private readonly IStorageBroker storageBroker;           // ❌ must not inject broker
    private readonly ICourseProcessingService courseProcessingService; // ❌ must not inject peer processing service

    public StudentProcessingService(
        IStudentService studentService,
        IStorageBroker storageBroker,
        ICourseProcessingService courseProcessingService)
    {
        this.studentService = studentService;
        this.storageBroker = storageBroker;
        this.courseProcessingService = courseProcessingService;
    }

    public async ValueTask<Student> EnsureStudentExistsAsync(Student student)
    {
        // ❌ VIOLATION ts-processings-001: calling broker directly
        var existing = await this.storageBroker.SelectStudentByIdAsync(student.Id);

        // ❌ VIOLATION ts-processings-002: managing a second entity (Course)
        var course = await this.courseProcessingService.EnsureCourseExistsAsync(student.DefaultCourse);
        student.CourseId = course.Id;

        return existing ?? await this.studentService.AddStudentAsync(student);
        // ❌ VIOLATION ts-processings-004: no exception wrapping — raw exceptions leak
    }
}
