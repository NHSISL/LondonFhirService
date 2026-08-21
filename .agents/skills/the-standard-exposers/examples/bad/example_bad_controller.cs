// ---
// skill: the-standard-exposers
// type: example
// source-section: "3.1.1 RESTful APIs"
// demonstrates: "ts-exposers-001 violation, ts-exposers-002 violation, ts-exposers-003 violation"
// ---

// ---------------------------------------------------------------
// Copyright (c) Coalition of the Good-Hearted Engineers
// FREE TO USE TO CONNECT THE WORLD
// ---------------------------------------------------------------

[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private readonly IStudentService studentService;
    private readonly ICourseService courseService; // ❌ VIOLATION ts-exposers-001: multiple services injected

    public StudentsController(IStudentService studentService, ICourseService courseService)
    {
        this.studentService = studentService;
        this.courseService = courseService;
    }

    [HttpPost]
    public async ValueTask<ActionResult<Student>> PostStudentAsync(Student student)
    {
        // ❌ VIOLATION ts-exposers-001: business logic in controller
        if (string.IsNullOrWhiteSpace(student.Name))
            return BadRequest("Name is required.");

        // ❌ VIOLATION ts-exposers-001: calling multiple services
        var course = await this.courseService.RetrieveDefaultCourseAsync();
        student.CourseId = course.Id;

        // ❌ VIOLATION ts-exposers-003: returns 200 OK instead of 201 Created
        var added = await this.studentService.AddStudentAsync(student);
        return Ok(added);

        // ❌ VIOLATION ts-exposers-002: no exception handling — raw exceptions leak to client
    }
}
