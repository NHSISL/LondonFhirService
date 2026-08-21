// ---
// skill: the-standard-csharp-variables
// type: example
// source-section: "0. Variables — 0.0 Naming, 0.1 Declarations, 0.2 Organization"
// demonstrates: "tsc-csharp-variables-001 through tsc-csharp-variables-012"
// ---

// ❌ Abbreviated name — violates tsc-csharp-variables-001
var s = new Student();
var stdnt = new Student();
students.Where(s => s.Name == "Todd"); // single-letter lambda param

// ❌ Collection type suffix — violates tsc-csharp-variables-002
var studentList = new List<Student>();

// ❌ Type suffix in name — violates tsc-csharp-variables-003
var studentModel = new Student();
var studentObj = new Student();

// ❌ Null variable without no prefix — violates tsc-csharp-variables-004
Student student = null;
int changeCount = 0;

// ❌ Explicit type when clear — violates tsc-csharp-variables-005
Student student = new Student();

// ❌ var for semi-clear method return — violates tsc-csharp-variables-006
var student = GetStudent();

// ❌ Object initializer for single property — violates tsc-csharp-variables-008
var inputStudentEvent = new StudentEvent
{
    Student = inputProcessedStudent
};

// ❌ Individual assignments for multi-property — violates tsc-csharp-variables-009
var studentEvent = new StudentEvent();
studentEvent.Student = someStudent;
studentEvent.Date = someDate;

// ❌ Long declaration not broken — violates tsc-csharp-variables-010
List<Student> washingtonSchoolsStudentsWithGrades = await GetAllWashingtonSchoolsStudentsWithTheirGradesAsync();

// ❌ Blank line between single-line declarations — violates tsc-csharp-variables-011
Student student = GetStudent();

School school = await GetSchoolAsync();

// ❌ No blank line around multi-line declaration — violates tsc-csharp-variables-012
Student student = GetStudent();
List<Student> washingtonSchoolsStudentsWithGrades =
    await GetAllWashingtonSchoolsStudentsWithTheirGradesAsync();
School school = await GetSchoolAsync();
