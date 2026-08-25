// ---
// skill: the-standard-exposers
// type: example
// source-section: "3.1.1 RESTful APIs"
// demonstrates: "ts-exposers-001, ts-exposers-002, ts-exposers-003, ts-exposers-004, ts-exposers-005, ts-exposers-006, ts-exposers-007, ts-exposers-008"
// ---

// ---------------------------------------------------------------
// Copyright (c) Coalition of the Good-Hearted Engineers
// FREE TO USE TO CONNECT THE WORLD
// ---------------------------------------------------------------

[ApiController]
[Route("api/[controller]")]
public class StudentsController : RESTFulController
{
    private readonly IStudentService studentService;

    public StudentsController(IStudentService studentService) =>
        this.studentService = studentService;

    // ✅ POST → 201 Created
    [HttpPost]
    public async ValueTask<ActionResult<Student>> PostStudentAsync(Student student)
    {
        try
        {
            Student addedStudent = await this.studentService.AddStudentAsync(student);
            return Created(addedStudent);
        }
        catch (StudentValidationException studentValidationException)
        {
            return BadRequest(studentValidationException.InnerException);
        }
        catch (StudentDependencyValidationException studentDependencyValidationException)
            when (studentDependencyValidationException.InnerException is AlreadyExistsStudentException)
        {
            return Conflict(studentDependencyValidationException.InnerException);
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

    // ✅ GET → 200 OK
    [HttpGet]
    public ActionResult<IQueryable<Student>> GetAllStudents()
    {
        try
        {
            IQueryable<Student> students = this.studentService.RetrieveAllStudents();
            return Ok(students);
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

    // ✅ DELETE → 200 OK with deleted resource
    [HttpDelete("{studentId}")]
    public async ValueTask<ActionResult<Student>> DeleteStudentByIdAsync(Guid studentId)
    {
        try
        {
            Student deletedStudent =
                await this.studentService.RemoveStudentByIdAsync(studentId);

            return Ok(deletedStudent);
        }
        catch (StudentDependencyValidationException studentDependencyValidationException)
            when (studentDependencyValidationException.InnerException is NotFoundStudentException)
        {
            return NotFound(studentDependencyValidationException.InnerException);
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
