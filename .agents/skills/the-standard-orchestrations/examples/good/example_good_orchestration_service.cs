// ---
// skill: the-standard-orchestrations
// type: example
// source-section: "2.3 Orchestration Services"
// demonstrates: "ts-orchestrations-001, ts-orchestrations-002, ts-orchestrations-003, ts-orchestrations-004"
// ---

// ---------------------------------------------------------------
// Copyright (c) Coalition of the Good-Hearted Engineers
// FREE TO USE TO CONNECT THE WORLD
// ---------------------------------------------------------------

public interface IStudentEnrollmentOrchestrationService
{
    ValueTask<StudentEnrollment> EnrollStudentAsync(Student student, Course course);
}

// ✅ Depends only on processing services — two different entity types coordinated
public partial class StudentEnrollmentOrchestrationService : IStudentEnrollmentOrchestrationService
{
    private readonly IStudentProcessingService studentProcessingService;
    private readonly ICourseProcessingService courseProcessingService;
    private readonly IEnrollmentProcessingService enrollmentProcessingService;

    public StudentEnrollmentOrchestrationService(
        IStudentProcessingService studentProcessingService,
        ICourseProcessingService courseProcessingService,
        IEnrollmentProcessingService enrollmentProcessingService)
    {
        this.studentProcessingService = studentProcessingService;
        this.courseProcessingService = courseProcessingService;
        this.enrollmentProcessingService = enrollmentProcessingService;
    }
}

public partial class StudentEnrollmentOrchestrationService
{
    public async ValueTask<StudentEnrollment> EnrollStudentAsync(Student student, Course course)
    {
        try
        {
            // ✅ Each entity coordinated through its own processing service
            Student persistedStudent =
                await this.studentProcessingService.EnsureStudentExistsAsync(student);

            Course persistedCourse =
                await this.courseProcessingService.EnsureCourseExistsAsync(course);

            var enrollment = new StudentEnrollment
            {
                StudentId = persistedStudent.Id,
                CourseId = persistedCourse.Id,
                EnrolledDate = DateTimeOffset.UtcNow
            };

            return await this.enrollmentProcessingService.AddEnrollmentAsync(enrollment);
        }
        catch (StudentProcessingDependencyException studentProcessingDependencyException)
        {
            // ✅ Wraps downstream exception in orchestration-level exception
            throw new StudentEnrollmentOrchestrationDependencyException(
                studentProcessingDependencyException);
        }
        catch (CourseProcessingDependencyException courseProcessingDependencyException)
        {
            throw new StudentEnrollmentOrchestrationDependencyException(
                courseProcessingDependencyException);
        }
        catch (StudentProcessingServiceException studentProcessingServiceException)
        {
            throw new StudentEnrollmentOrchestrationServiceException(
                studentProcessingServiceException);
        }
    }
}
