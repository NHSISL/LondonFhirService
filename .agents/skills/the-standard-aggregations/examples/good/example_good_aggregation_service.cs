// ---
// skill: the-standard-aggregations
// type: example
// source-section: "2.4 Aggregation Services"
// demonstrates: "ts-aggregations-001, ts-aggregations-002, ts-aggregations-003, ts-aggregations-004"
// ---

// ---------------------------------------------------------------
// Copyright (c) Coalition of the Good-Hearted Engineers
// FREE TO USE TO CONNECT THE WORLD
// ---------------------------------------------------------------

public interface ISchoolAggregationService
{
    ValueTask ProcessNewSchoolYearAsync();
}

// ✅ Depends only on orchestration services
public partial class SchoolAggregationService : ISchoolAggregationService
{
    private readonly IStudentEnrollmentOrchestrationService studentEnrollmentOrchestrationService;
    private readonly ICourseAssignmentOrchestrationService courseAssignmentOrchestrationService;
    private readonly ITeacherAllocationOrchestrationService teacherAllocationOrchestrationService;

    public SchoolAggregationService(
        IStudentEnrollmentOrchestrationService studentEnrollmentOrchestrationService,
        ICourseAssignmentOrchestrationService courseAssignmentOrchestrationService,
        ITeacherAllocationOrchestrationService teacherAllocationOrchestrationService)
    {
        this.studentEnrollmentOrchestrationService = studentEnrollmentOrchestrationService;
        this.courseAssignmentOrchestrationService = courseAssignmentOrchestrationService;
        this.teacherAllocationOrchestrationService = teacherAllocationOrchestrationService;
    }
}

public partial class SchoolAggregationService
{
    // ✅ Single operation fans out to multiple orchestration services
    public async ValueTask ProcessNewSchoolYearAsync()
    {
        try
        {
            await this.studentEnrollmentOrchestrationService.EnrollAllStudentsAsync();
            await this.courseAssignmentOrchestrationService.AssignCoursesAsync();
            await this.teacherAllocationOrchestrationService.AllocateTeachersAsync();
        }
        catch (StudentEnrollmentOrchestrationDependencyException orchestrationDependencyException)
        {
            // ✅ Wraps orchestration exception in aggregation-level exception
            throw new SchoolAggregationDependencyException(orchestrationDependencyException);
        }
        catch (StudentEnrollmentOrchestrationServiceException orchestrationServiceException)
        {
            throw new SchoolAggregationServiceException(orchestrationServiceException);
        }
    }
}
