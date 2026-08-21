// ---
// skill: the-standard-cancellation-patterns
// type: example
// source-section: "7.5 Whenever timeout logic exists, both catch blocks MUST exist"
// demonstrates: "tsc-csharp-cp-011 violation"
// ---

using System;
using System.Threading;
using System.Threading.Tasks;

namespace TheStandard.CancellationPatterns.Examples.Bad
{
    public class StudentService
    {
        private readonly IStorageBroker storageBroker;
        private CancellationTokenSource timeoutSource;

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

            timeoutSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));

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
            // ❌ WRONG: Plain catch placed BEFORE timeout-guarded catch
            // This will always execute first, preventing timeout detection
            catch (OperationCanceledException)
            {
                throw;
            }
            // ❌ This catch block will NEVER execute because the previous
            // catch already caught all OperationCanceledException instances
            catch (OperationCanceledException)
            when (timeoutSource.IsCancellationRequested)
            {
                var timeoutException =
                    new TimeoutException("Operation timed out.");

                throw CreateAndLogDependencyException(timeoutException);
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

            return studentDependencyException;
        }

        private StudentServiceException CreateAndLogServiceException(
            Exception exception)
        {
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
