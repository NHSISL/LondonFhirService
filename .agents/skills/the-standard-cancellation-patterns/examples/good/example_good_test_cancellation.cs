// ---
// skill: the-standard-cancellation-patterns
// type: example
// source-section: "9.0 Testing Standards"
// demonstrates: "tsc-csharp-cp-018, tsc-csharp-cp-019"
// ---

using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace TheStandard.CancellationPatterns.Examples.Good.Tests
{
    public class StudentServiceTests
    {
        // ✅ CORRECT: Test verifies OperationCanceledException is never wrapped
        [Fact]
        public async Task ShouldThrowOperationCanceledExceptionOnCancellationAsync()
        {
            // given
            Guid randomStudentId = Guid.NewGuid();
            Guid inputStudentId = randomStudentId;

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            var storageBrokerMock = new Mock<IStorageBroker>();

            storageBrokerMock
                .Setup(broker =>
                    broker.SelectStudentByIdAsync(
                        inputStudentId,
                        cancellationToken))
                .ThrowsAsync(new OperationCanceledException());

            var studentService = new StudentService(storageBrokerMock.Object);

            // when
            ValueTask<Student> retrieveStudentTask =
                studentService.RetrieveStudentByIdAsync(
                    inputStudentId,
                    cancellationToken);

            // then
            // ✅ CORRECT: Verifies OperationCanceledException is NOT wrapped
            await Assert.ThrowsAsync<OperationCanceledException>(
                () => retrieveStudentTask.AsTask());

            storageBrokerMock.Verify(
                broker => broker.SelectStudentByIdAsync(
                    inputStudentId,
                    cancellationToken),
                Times.Once);

            storageBrokerMock.VerifyNoOtherCalls();
        }

        // ✅ CORRECT: TimeoutException is included in Theory MemberData
        [Theory]
        [MemberData(nameof(DependencyExceptions))]
        public async Task ShouldThrowDependencyExceptionOnDependencyErrorAsync(
            Exception dependencyException)
        {
            // given
            Guid randomStudentId = Guid.NewGuid();
            Guid inputStudentId = randomStudentId;
            CancellationToken cancellationToken = default;

            var storageBrokerMock = new Mock<IStorageBroker>();

            storageBrokerMock
                .Setup(broker =>
                    broker.SelectStudentByIdAsync(
                        inputStudentId,
                        cancellationToken))
                .ThrowsAsync(dependencyException);

            var studentService = new StudentService(storageBrokerMock.Object);

            // when
            ValueTask<Student> retrieveStudentTask =
                studentService.RetrieveStudentByIdAsync(
                    inputStudentId,
                    cancellationToken);

            // then
            StudentDependencyException actualStudentDependencyException =
                await Assert.ThrowsAsync<StudentDependencyException>(
                    () => retrieveStudentTask.AsTask());

            Assert.Equal(
                dependencyException,
                actualStudentDependencyException.InnerException);

            storageBrokerMock.Verify(
                broker => broker.SelectStudentByIdAsync(
                    inputStudentId,
                    cancellationToken),
                Times.Once);

            storageBrokerMock.VerifyNoOtherCalls();
        }

        // ✅ CORRECT: TimeoutException is present in the MemberData
        public static TheoryData DependencyExceptions()
        {
            return new TheoryData<Exception>
            {
                new TimeoutException(),
                new InvalidOperationException(),
                new HttpRequestException()
            };
        }
    }

    // Mock placeholder for demonstration
    public class Mock<T> where T : class
    {
        public T Object { get; set; }
        public Mock<T> Setup(Action<T> expression) => this;
        public Mock<T> ThrowsAsync(Exception exception) => this;
        public void Verify(Action<T> expression, Times times) { }
        public void VerifyNoOtherCalls() { }
    }

    public class Times
    {
        public static Times Once { get; } = new Times();
    }

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

    public class StudentService
    {
        private readonly IStorageBroker storageBroker;

        public StudentService(IStorageBroker storageBroker)
        {
            this.storageBroker = storageBroker;
        }

        public ValueTask<Student> RetrieveStudentByIdAsync(
            Guid studentId,
            CancellationToken cancellationToken = default)
        {
            // Implementation would go here
            return default;
        }
    }
}
