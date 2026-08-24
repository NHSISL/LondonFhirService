// ---
// skill: the-standard-versioning
// type: example
// source-section: "1.9 Versioning / 3.1.1 RESTful APIs"
// demonstrates: "ts-versioning-005, ts-versioning-007"
// ---

// ---------------------------------------------------------------
// Copyright (c) Coalition of the Good-Hearted Engineers
// FREE TO USE TO CONNECT THE WORLD
// ---------------------------------------------------------------

// ✅ Version exposed in URL path via route attribute
[ApiController]
[Route("api/v1/[controller]")]
public class StudentsController : RESTFulController
{
    private readonly IStudentService studentService;

    public StudentsController(IStudentService studentService) =>
        this.studentService = studentService;

    [HttpGet]
    public ActionResult<IQueryable<Student>> GetAllStudents()
    {
        try
        {
            return Ok(this.studentService.RetrieveAllStudents());
        }
        catch (StudentDependencyException studentDependencyException)
        {
            return FailedDependency(studentDependencyException.InnerException);
        }
        catch (StudentServiceException studentServiceException)
        {
            return InternalServerError(studentServiceException);
        }
    }
}

// ✅ Version 2 controller alongside v1 — both maintained during migration
[ApiController]
[Route("api/v2/[controller]")]
public class StudentsV2Controller : RESTFulController
{
    private readonly IStudentV2Service studentV2Service;

    public StudentsV2Controller(IStudentV2Service studentV2Service) =>
        this.studentV2Service = studentV2Service;

    [HttpGet]
    public ActionResult<IQueryable<StudentV2>> GetAllStudents()
    {
        try
        {
            return Ok(this.studentV2Service.RetrieveAllStudents());
        }
        catch (StudentV2DependencyException studentDependencyException)
        {
            return FailedDependency(studentDependencyException.InnerException);
        }
        catch (StudentV2ServiceException studentServiceException)
        {
            return InternalServerError(studentServiceException);
        }
    }
}
