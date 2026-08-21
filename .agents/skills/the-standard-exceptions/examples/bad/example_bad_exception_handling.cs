// ---
// skill: the-standard-exceptions
// type: example
// source-section: "2.1.3.1 Exception Handling"
// demonstrates: "ts-exceptions-001 violation, ts-exceptions-003 violation, ts-exceptions-006 violation, ts-exceptions-007 violation"
// ---

// ---------------------------------------------------------------
// Copyright (c) Coalition of the Good-Hearted Engineers
// FREE TO USE TO CONNECT THE WORLD
// ---------------------------------------------------------------

public partial class StudentService
{
    public async ValueTask<Student> AddStudentAsync(Student student)
    {
        try
        {
            return await this.storageBroker.InsertStudentAsync(student);
        }
        // ❌ VIOLATION ts-exceptions-001: single catch-all block — no category distinction
        catch (Exception ex)
        {
            // ❌ VIOLATION ts-exceptions-008: exposing raw exception message to caller
            throw new Exception($"Database error: {ex.Message} — Stack: {ex.StackTrace}");
            // ❌ VIOLATION ts-exceptions-007: inner exception is discarded — only message string passed
        }
    }

    public async ValueTask<Student> ModifyStudentAsync(Student student)
    {
        try
        {
            return await this.storageBroker.UpdateStudentAsync(student);
        }
        catch (SqlException sqlException)
        {
            // ❌ VIOLATION ts-exceptions-006: re-throwing a lower-layer exception type directly
            // StudentProcessingService will now receive a SqlException — layer boundary broken
            throw new StudentDependencyException(
                new FailedStudentStorageException(sqlException));
            // The StudentProcessingService should define StudentProcessingDependencyException
            // and wrap the foundation's StudentDependencyException inside it.
        }
        catch (Exception)
        {
            // ❌ VIOLATION ts-exceptions-007: swallowing the exception silently
            return student;
        }
    }
}
