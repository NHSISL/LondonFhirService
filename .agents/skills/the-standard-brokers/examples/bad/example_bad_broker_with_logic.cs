// ---
// skill: the-standard-brokers
// type: example
// source-section: "1. Brokers"
// demonstrates: "ts-brokers-001 violation, ts-brokers-002 violation"
// ---

// ---------------------------------------------------------------
// Copyright (c) Coalition of the Good-Hearted Engineers
// FREE TO USE TO CONNECT THE WORLD
// ---------------------------------------------------------------

// ❌ VIOLATION ts-brokers-002: technology name in the interface
public interface ISqlStorageBroker
{
    ValueTask<Student> InsertStudentAsync(Student student);
}

// ❌ VIOLATION ts-brokers-001: business logic inside a broker
public class SqlStorageBroker : ISqlStorageBroker
{
    private readonly string connectionString;

    public SqlStorageBroker()
    {
        this.connectionString = "Server=.;Database=School;";
    }

    public async ValueTask<Student> InsertStudentAsync(Student student)
    {
        // ❌ Validation belongs in the service layer — not here
        if (student == null)
            throw new ArgumentNullException(nameof(student));

        if (string.IsNullOrWhiteSpace(student.Name))
            throw new ArgumentException("Name is required.");

        // ❌ Calling another broker from inside a broker
        await loggingBroker.LogInformationAsync($"Inserting student {student.Id}");

        // ❌ Raw SQL in broker exposes infrastructure detail and leaks technology
        using var conn = new SqlConnection(this.connectionString);
        var cmd = new SqlCommand("INSERT INTO Students ...", conn);
        await conn.OpenAsync();
        await cmd.ExecuteNonQueryAsync();

        return student;
    }
}
