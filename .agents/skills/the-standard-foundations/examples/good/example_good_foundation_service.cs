// ---
// skill: the-standard-foundations
// type: example
// source-section: "2. Foundation Services"
// demonstrates: "ts-foundations-001, ts-foundations-002, ts-foundations-003, ts-foundations-004, ts-foundations-005, ts-foundations-006, ts-foundations-007, ts-foundations-008"
// ---

// ---------------------------------------------------------------
// Copyright (c) Coalition of the Good-Hearted Engineers
// FREE TO USE TO CONNECT THE WORLD
// ---------------------------------------------------------------

// ✅ Interface named I{Entity}Service
public interface IStudentService
{
    ValueTask<Student> AddStudentAsync(Student student);
    IQueryable<Student> RetrieveAllStudents();
    ValueTask<Student> RetrieveStudentByIdAsync(Guid studentId);
    ValueTask<Student> ModifyStudentAsync(Student student);
    ValueTask<Student> RemoveStudentByIdAsync(Guid studentId);
}

// ✅ Partial root class — single entity, one broker dependency
public partial class StudentService : IStudentService
{
    private readonly IStorageBroker storageBroker;
    private readonly ILoggingBroker loggingBroker;

    public StudentService(IStorageBroker storageBroker, ILoggingBroker loggingBroker)
    {
        this.storageBroker = storageBroker;
        this.loggingBroker = loggingBroker;
    }
}

// ✅ Partial class for Add operations — structural validation + exception wrapping
public partial class StudentService
{
    public async ValueTask<Student> AddStudentAsync(Student student)
    {
        // ✅ Structural validation
        ValidateStudentOnAdd(student);

        try
        {
            // ✅ Pure pass-through to broker — no transformation
            return await this.storageBroker.InsertStudentAsync(student);
        }
        catch (DuplicateKeyException duplicateKeyException)
        {
            var alreadyExistsStudentException =
                new AlreadyExistsStudentException(duplicateKeyException);

            throw new StudentDependencyValidationException(alreadyExistsStudentException);
        }
        catch (DbUpdateConcurrencyException dbUpdateConcurrencyException)
        {
            var lockedStudentException =
                new LockedStudentException(dbUpdateConcurrencyException);

            throw new StudentDependencyException(lockedStudentException);
        }
        catch (SqlException sqlException)
        {
            var failedStudentStorageException =
                new FailedStudentStorageException(sqlException);

            throw new StudentDependencyException(failedStudentStorageException);
        }
    }
}
