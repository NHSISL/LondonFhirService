// ---
// skill: the-standard-cancellation-patterns
// type: example
// source-section: "7.4 Whenever a CancellationToken is used, BOTH catch blocks MUST exist"
// demonstrates: "tsc-csharp-cp-005, tsc-csharp-cp-014"
// ---

using System;
using System.Threading;
using System.Threading.Tasks;

namespace TheStandard.CancellationPatterns.Examples.Good
{
    public partial class StudentService
    {
        private readonly IStorageBroker storageBroker;

        public StudentService(IStorageBroker storageBroker)
        {
            this.storageBroker = storageBroker;
        }

        // ✅ CORRECT: TryCatch with CancellationToken but NO timeout logic
        // ✅ CORRECT: Plain OperationCanceledException catch block is present
        public ValueTask<Student> RetrieveStudentAsync(
            Guid studentId,
            CancellationToken cancellationToken = default) =>
        TryCatch(async () =>
        {
            // ✅ CORRECT: Validate cancellation before dependency operation
            cancellationToken.ThrowIfCancellationRequested();

            // No timeout source created — token passed directly
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
            // ✅ CORRECT: Even without timeout, this catch block MUST exist
            // to prevent OperationCanceledException from being caught by
            // the catch-all exception handler below
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw CreateAndLogServiceException(exception);
            }
        }

        private StudentServiceException CreateAndLogServiceException(
            Exception exception)
        {
            var studentServiceException =
                new StudentServiceException(exception);

            // Log exception here
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

    public class StudentServiceException : Exception
    {
        public StudentServiceException(Exception innerException)
            : base("Student service error occurred.", innerException)
        { }
    }
}
