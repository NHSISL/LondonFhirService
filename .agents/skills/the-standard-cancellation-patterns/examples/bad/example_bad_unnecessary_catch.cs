// ---
// skill: the-standard-cancellation-patterns
// type: example
// source-section: "7.4 Whenever a CancellationToken is used, BOTH catch blocks MUST exist"
// demonstrates: "tsc-csharp-cp-014 violation"
// ---

using System;
using System.Threading;
using System.Threading.Tasks;

namespace TheStandard.CancellationPatterns.Examples.Bad
{
    public class StudentService
    {
        private readonly IStorageBroker storageBroker;

        public StudentService(IStorageBroker storageBroker)
        {
            this.storageBroker = storageBroker;
        }

        public ValueTask<Student> RetrieveStudentAsync(
            Guid studentId,
            CancellationToken cancellationToken = default) =>
        TryCatch(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            return await this.storageBroker.SelectStudentByIdAsync(
                studentId,
                cancellationToken);
        });

        private async ValueTask<Student> TryCatch(
            ReturningStudentFunction returningStudentFunction)
        {
            try
            {
                return await returningStudentFunction();
            }
            // ❌ WRONG: Missing `catch (OperationCanceledException) { throw; }`
            // This causes OperationCanceledException to be caught by the
            // catch-all exception handler below and wrapped in StudentServiceException
            catch (Exception exception)
            {
                throw CreateAndLogServiceException(exception);
            }
        }

        private StudentServiceException CreateAndLogServiceException(
            Exception exception)
        {
            // OperationCanceledException will be wrapped here,
            // preventing it from bubbling correctly
            var studentServiceException =
                new StudentServiceException(exception);

            return studentServiceException;
        }
    }

    public delegate ValueTask<Student> ReturningStudentFunction();

    public interface IStorageBroker
    {
        ValueTask<Student> SelectStudentByIdAsync(
            Guid studentId,
            CancellationToken cancellationToken);
    }

    public class Student
    {
        public Guid Id { get; set; }
    }

    public class StudentServiceException : Exception
    {
        public StudentServiceException(Exception innerException)
            : base("Student service error occurred.", innerException)
        { }
    }
}
