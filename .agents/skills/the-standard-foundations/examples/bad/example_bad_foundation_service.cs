// ---
// skill: the-standard-foundations
// type: example
// source-section: "2. Foundation Services"
// demonstrates: "ts-foundations-001 violation, ts-foundations-003 violation, ts-foundations-004 violation, ts-foundations-006 violation"
// ---

// ---------------------------------------------------------------
// Copyright (c) Coalition of the Good-Hearted Engineers
// FREE TO USE TO CONNECT THE WORLD
// ---------------------------------------------------------------

// ❌ VIOLATION ts-foundations-001 + ts-foundations-003: calls another service AND manages two entities
public class StudentService : IStudentService
{
    private readonly IStorageBroker storageBroker;
    private readonly ICourseService courseService; // ❌ services must not depend on other services at foundation level

    public StudentService(IStorageBroker storageBroker, ICourseService courseService)
    {
        this.storageBroker = storageBroker;
        this.courseService = courseService;
    }

    public async ValueTask<Student> AddStudentAsync(Student student)
    {
        // ❌ VIOLATION ts-foundations-004: model transformation before broker call
        student.CreatedDate = DateTime.UtcNow;
        student.UpdatedDate = DateTime.UtcNow;

        // ❌ VIOLATION ts-foundations-003: calling another service from a foundation service
        var defaultCourse = await this.courseService.RetrieveDefaultCourseAsync();
        student.CourseId = defaultCourse.Id;

        // ❌ VIOLATION ts-foundations-006: no exception wrapping — raw broker exceptions leak out
        return await this.storageBroker.InsertStudentAsync(student);
    }
}
