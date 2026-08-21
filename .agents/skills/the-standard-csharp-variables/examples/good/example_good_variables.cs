// ---
// skill: the-standard-csharp-variables
// type: example
// source-section: "0. Variables — 0.0 Naming, 0.1 Declarations, 0.2 Organization"
// demonstrates: "tsc-csharp-variables-001 through tsc-csharp-variables-012"
// ---

// ✅ Full name — no abbreviations (tsc-csharp-variables-001)
var student = new Student();

// ✅ Plural collection name (tsc-csharp-variables-002)
var students = new List<Student>();

// ✅ No type suffix (tsc-csharp-variables-003)
var student = new Student();

// ✅ Null variable named with no prefix (tsc-csharp-variables-004)
Student noStudent = null;
int noChangeCount = 0;

// ✅ var for clear type (tsc-csharp-variables-005)
var student = new Student();

// ✅ Explicit type for semi-clear method return (tsc-csharp-variables-006)
Student student = GetStudent();

// ✅ var for anonymous type (tsc-csharp-variables-007)
var student = new { Name = "Hassan", Score = 100 };

// ✅ Single-property — declare then assign (tsc-csharp-variables-008)
var inputStudentEvent = new StudentEvent();
inputStudentEvent.Student = inputProcessedStudent;

// ✅ Multi-property — object initializer block (tsc-csharp-variables-009)
var studentEvent = new StudentEvent
{
    Student = someStudent,
    Date = someDate
};

// ✅ Break long declaration at = (tsc-csharp-variables-010)
List<Student> washingtonSchoolsStudentsWithGrades =
    await GetAllWashingtonSchoolsStudentsWithTheirGradesAsync();

// ✅ Single-line declarations stacked — no blank lines (tsc-csharp-variables-011)
Student student = GetStudent();
School school = await GetSchoolAsync();

// ✅ Multi-line declaration surrounded by blank lines (tsc-csharp-variables-012)
Student student = GetStudent();

List<Student> washingtonSchoolsStudentsWithGrades =
    await GetAllWashingtonSchoolsStudentsWithTheirGradesAsync();

School school = await GetSchoolAsync();
