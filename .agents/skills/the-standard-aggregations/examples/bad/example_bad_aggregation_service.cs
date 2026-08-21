// ---
// skill: the-standard-aggregations
// type: example
// source-section: "2.4 Aggregation Services"
// demonstrates: "ts-aggregations-001 violation, ts-aggregations-002 violation, ts-aggregations-003 violation"
// ---

// ---------------------------------------------------------------
// Copyright (c) Coalition of the Good-Hearted Engineers
// FREE TO USE TO CONNECT THE WORLD
// ---------------------------------------------------------------

// ❌ VIOLATION ts-aggregations-001: injects processing and foundation services directly
public class SchoolAggregationService : ISchoolAggregationService
{
    private readonly IStudentProcessingService studentProcessingService; // ❌ must not inject processing service
    private readonly IStorageBroker storageBroker;                        // ❌ must not inject broker

    public SchoolAggregationService(
        IStudentProcessingService studentProcessingService,
        IStorageBroker storageBroker)
    {
        this.studentProcessingService = studentProcessingService;
        this.storageBroker = storageBroker;
    }

    public async ValueTask ProcessNewSchoolYearAsync()
    {
        // ❌ VIOLATION ts-aggregations-002: business logic / conditional in aggregation
        var students = this.studentProcessingService.RetrieveAllStudentsAsync();

        foreach (var student in students)
        {
            if (student.IsActive)
            {
                await this.studentProcessingService.UpsertStudentAsync(student);
            }
        }

        // ❌ VIOLATION ts-aggregations-001: calling broker directly
        await this.storageBroker.InsertAuditLogAsync(new AuditLog { Event = "SchoolYearProcessed" });

        // ❌ VIOLATION ts-aggregations-003: no exception wrapping
    }
}
