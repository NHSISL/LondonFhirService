// ---
// skill: the-standard-versioning
// type: example
// source-section: "1.9 Versioning"
// demonstrates: "ts-versioning-005 violation, ts-versioning-006 violation"
// ---

// ---------------------------------------------------------------
// Copyright (c) Coalition of the Good-Hearted Engineers
// FREE TO USE TO CONNECT THE WORLD
// ---------------------------------------------------------------

// ❌ VIOLATION ts-versioning-005: version not in URL — consumers cannot distinguish versions
[ApiController]
[Route("api/[controller]")]
public class StudentsController : RESTFulController
{
    private readonly IStudentService studentService;

    public StudentsController(IStudentService studentService) =>
        this.studentService = studentService;

    // ❌ VIOLATION ts-versioning-006: old Student model removed without maintaining previous version
    // Previous StudentController (v1) was deleted when v2 was released — breaking existing consumers
    [HttpGet]
    public ActionResult<IQueryable<StudentV2>> GetAllStudents()
    {
        // Returns StudentV2 model but consumers expecting Student (v1) format receive no warning
        return Ok(this.studentService.RetrieveAllStudents());
    }
}

/* ❌ VIOLATION ts-versioning-007 in .csproj:
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <!-- Missing <Version> element — version not declared in project -->
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
</Project>
*/
