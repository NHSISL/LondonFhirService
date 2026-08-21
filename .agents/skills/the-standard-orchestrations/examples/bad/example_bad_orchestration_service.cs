// ---
// skill: the-standard-orchestrations
// type: example
// source-section: "2.3 Orchestration Services"
// demonstrates: "ts-orchestrations-001 violation, ts-orchestrations-003 violation"
// ---

// ---------------------------------------------------------------
// Copyright (c) Coalition of the Good-Hearted Engineers
// FREE TO USE TO CONNECT THE WORLD
// ---------------------------------------------------------------

// ❌ VIOLATION ts-orchestrations-001: injects a foundation service directly
public class StudentEnrollmentOrchestrationService : IStudentEnrollmentOrchestrationService
{
    private readonly IStudentService studentService;           // ❌ must not inject foundation service
    private readonly IStorageBroker storageBroker;             // ❌ must not inject broker
    private readonly ICourseProcessingService courseProcessingService;

    public StudentEnrollmentOrchestrationService(
        IStudentService studentService,
        IStorageBroker storageBroker,
        ICourseProcessingService courseProcessingService)
    {
        this.studentService = studentService;
        this.storageBroker = storageBroker;
        this.courseProcessingService = courseProcessingService;
    }

    public async ValueTask<StudentEnrollment> EnrollStudentAsync(Student student, Course course)
    {
        // ❌ VIOLATION ts-orchestrations-001: calling foundation service directly
        Student persistedStudent = await this.studentService.AddStudentAsync(student);

        // ❌ VIOLATION ts-orchestrations-001: calling broker directly
        var existingEnrollments = this.storageBroker.SelectAllEnrollments();

        Course persistedCourse = await this.courseProcessingService.EnsureCourseExistsAsync(course);

        var enrollment = new StudentEnrollment
        {
            StudentId = persistedStudent.Id,
            CourseId = persistedCourse.Id
        };

        // ❌ VIOLATION ts-orchestrations-003: no exception wrapping — processing exceptions leak out
        return await this.storageBroker.InsertEnrollmentAsync(enrollment);
    }
}
