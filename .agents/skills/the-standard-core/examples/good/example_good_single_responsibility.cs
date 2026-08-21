// ---
// skill: the-standard-core
// type: example
// source-section: "0.1 Purposing / 0.2 Principles"
// demonstrates: "ts-core-001, ts-core-002, ts-core-005"
// ---

// ---------------------------------------------------------------
// Copyright (c) Coalition of the Good-Hearted Engineers
// FREE TO USE TO CONNECT THE WORLD
// ---------------------------------------------------------------

// ✅ Single responsibility: this class only adds, retrieves, modifies, and removes students.
// ✅ Named by what it is (StudentService), not how it works.
// ✅ Exposes only the interface contract — implementation hidden.

public partial class StudentService : IStudentService
{
    private readonly IStorageBroker storageBroker;
    private readonly ILoggingBroker loggingBroker;

    public StudentService(
        IStorageBroker storageBroker,
        ILoggingBroker loggingBroker)
    {
        this.storageBroker = storageBroker;
        this.loggingBroker = loggingBroker;
    }

    public ValueTask<Student> AddStudentAsync(Student student) =>
        TryCatch(async () =>
        {
            ValidateStudentOnAdd(student);
            return await this.storageBroker.InsertStudentAsync(student);
        });
}
