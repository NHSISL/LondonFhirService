// ---
// skill: the-standard-core
// type: example
// source-section: "0.2 Principles"
// demonstrates: "ts-core-001, ts-core-003, ts-core-006"
// ---

// ❌ Mixed concerns: a controller calling a broker directly (layer skipping)
// ❌ Business logic (filtering) inside a controller (not a service)
// ❌ Infrastructure concern (SQL query) mixed with business concern

[ApiController]
public class StudentsController : ControllerBase
{
    private readonly SqlStudentHelper sqlHelper; // ❌ infrastructure exposed to controller

    public StudentsController(SqlStudentHelper sqlHelper)
    {
        this.sqlHelper = sqlHelper;
    }

    [HttpGet]
    public async Task<IActionResult> GetActiveStudents()
    {
        // ❌ Business rule (IsActive filter) inside controller
        // ❌ Direct infrastructure call — bypasses service layer
        var students = await this.sqlHelper
            .RunQuery("SELECT * FROM Students WHERE IsActive = 1");

        return Ok(students);
    }
}
