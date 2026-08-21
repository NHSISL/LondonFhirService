// ---
// skill: the-standard-testing
// type: example
// source-section: "2.1 Foundation Services — Testing"
// demonstrates: "ts-testing-001 violation, ts-testing-002 violation, ts-testing-003 violation, ts-testing-004 violation"
// ---

// ---------------------------------------------------------------
// Copyright (c) Coalition of the Good-Hearted Engineers
// FREE TO USE TO CONNECT THE WORLD
// ---------------------------------------------------------------

// ❌ VIOLATION ts-testing-007: class not named {Subject}Tests
public class StudentTests
{
    // ❌ VIOLATION ts-testing-004: uses a real DbContext — not mocked
    private readonly SchoolDbContext dbContext = new SchoolDbContext();

    // ❌ VIOLATION ts-testing-002: vague name — does not describe outcome or condition
    [Fact]
    public async Task TestAdd()
    {
        // ❌ VIOLATION ts-testing-004: real infrastructure dependency
        var service = new StudentService(new StorageBroker(dbContext), new LoggingBroker());

        var student = new Student { Id = Guid.NewGuid(), Name = "Alice" };

        // ❌ VIOLATION ts-testing-001: implementation already exists — test was written after
        var result = await service.AddStudentAsync(student);

        // ❌ VIOLATION ts-testing-003: multiple unrelated assertions in one test
        Assert.NotNull(result);
        Assert.Equal("Alice", result.Name);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.True(result.CreatedDate > DateTime.MinValue);
        Assert.True(result.UpdatedDate > DateTime.MinValue);
        // This test will fail for ANY of these reasons, making diagnosis impossible
    }
}
