// ---
// skill: the-standard-cancellation-patterns
// type: example
// source-section: "7.0 Exception Handling Rules"
// demonstrates: "tsc-csharp-cp-005, tsc-csharp-cp-008, tsc-csharp-cp-010, tsc-csharp-cp-011, tsc-csharp-cp-012, tsc-csharp-cp-013, tsc-csharp-cp-014"
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

        // ✅ CORRECT: TryCatch with timeout logic and correct catch block ordering
        public ValueTask<Student> RetrieveStudentAsync(
            Guid studentId,
            CancellationToken cancellationToken = default) =>
        TryCatch(async () =>
        {
            // ✅ CORRECT: Validate cancellation before dependency operation
            cancellationToken.ThrowIfCancellationRequested();

            // ✅ CORRECT: Create linked token source for timeout support
            using var timeoutSource =
                new CancellationTokenSource(TimeSpan.FromSeconds(30));

            using var linkedSource =
                CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken,
                    timeoutSource.Token);

            return await this.storageBroker.SelectStudentByIdAsync(
                studentId,
                linkedSource.Token);
        });

        private async ValueTask<Student> TryCatch(
            ReturningStudentFunction returningStudentFunction)
        {
            try
            {
                return await returningStudentFunction();
            }
            // ✅ CORRECT: Timeout-guarded catch BEFORE plain OperationCanceledException
            catch (OperationCanceledException)
            when (timeoutSource.IsCancellationRequested)
            {
                // ✅ CORRECT: Timeout is wrapped as a dependency failure
                var timeoutException =
                    new TimeoutException(
                        "The student retrieval operation timed out.");

                throw CreateAndLogDependencyException(timeoutException);
            }
            // ✅ CORRECT: Plain OperationCanceledException is rethrown unchanged
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw CreateAndLogServiceException(exception);
            }
        }

        private StudentDependencyException CreateAndLogDependencyException(
            Exception exception)
        {
            var studentDependencyException =
                new StudentDependencyException(exception);

            // Log exception here
            return studentDependencyException;
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

    public class StudentDependencyException : Exception
    {
        public StudentDependencyException(Exception innerException)
            : base("Student dependency error occurred.", innerException)
        { }
    }

    public class StudentServiceException : Exception
    {
        public StudentServiceException(Exception innerException)
            : base("Student service error occurred.", innerException)
        { }
    }
}
