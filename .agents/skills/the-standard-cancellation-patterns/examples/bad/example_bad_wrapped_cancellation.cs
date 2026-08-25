// ---
// skill: the-standard-cancellation-patterns
// type: example
// source-section: "7.3 OperationCanceledException MUST NEVER be wrapped"
// demonstrates: "tsc-csharp-cp-013 violation"
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
            // ❌ WRONG: OperationCanceledException is wrapped
            catch (OperationCanceledException exception)
            {
                throw new FailedStudentServiceException(exception);
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

    public class FailedStudentServiceException : Exception
    {
        public FailedStudentServiceException(Exception innerException)
            : base("Failed student service error occurred.", innerException)
        { }
    }

    public class StudentServiceException : Exception
    {
        public StudentServiceException(Exception innerException)
            : base("Student service error occurred.", innerException)
        { }
    }
}
