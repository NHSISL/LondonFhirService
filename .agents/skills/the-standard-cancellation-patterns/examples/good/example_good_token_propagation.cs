// ---
// skill: the-standard-cancellation-patterns
// type: example
// source-section: "6.0 Propagation Rules"
// demonstrates: "tsc-csharp-cp-006, tsc-csharp-cp-007"
// ---

using System;
using System.Threading;
using System.Threading.Tasks;

namespace TheStandard.CancellationPatterns.Examples.Good
{
    // ✅ CORRECT: Token flows through all layers

    // LAYER: API Controller (Exposer)
    public class StudentsController
    {
        private readonly IStudentCoordinationService studentCoordinationService;

        public StudentsController(
            IStudentCoordinationService studentCoordinationService)
        {
            this.studentCoordinationService = studentCoordinationService;
        }

        public async ValueTask<Student> GetStudentByIdAsync(
            Guid studentId,
            CancellationToken cancellationToken)
        {
            // ✅ Token propagated to coordination layer
            return await this.studentCoordinationService
                .RetrieveStudentByIdAsync(studentId, cancellationToken);
        }
    }

    // LAYER: Coordination Service
    public class StudentCoordinationService : IStudentCoordinationService
    {
        private readonly IStudentOrchestrationService studentOrchestrationService;

        public StudentCoordinationService(
            IStudentOrchestrationService studentOrchestrationService)
        {
            this.studentOrchestrationService = studentOrchestrationService;
        }

        public async ValueTask<Student> RetrieveStudentByIdAsync(
            Guid studentId,
            CancellationToken cancellationToken = default)
        {
            // ✅ Token propagated to orchestration layer
            return await this.studentOrchestrationService
                .RetrieveStudentByIdAsync(studentId, cancellationToken);
        }
    }

    // LAYER: Orchestration Service
    public class StudentOrchestrationService : IStudentOrchestrationService
    {
        private readonly IStudentService studentService;

        public StudentOrchestrationService(IStudentService studentService)
        {
            this.studentService = studentService;
        }

        public async ValueTask<Student> RetrieveStudentByIdAsync(
            Guid studentId,
            CancellationToken cancellationToken = default)
        {
            // ✅ Token propagated to foundation layer
            return await this.studentService
                .RetrieveStudentByIdAsync(studentId, cancellationToken);
        }
    }

    // LAYER: Foundation Service
    public class StudentService : IStudentService
    {
        private readonly IStorageBroker storageBroker;

        public StudentService(IStorageBroker storageBroker)
        {
            this.storageBroker = storageBroker;
        }

        public async ValueTask<Student> RetrieveStudentByIdAsync(
            Guid studentId,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // ✅ Token propagated to broker
            return await this.storageBroker
                .SelectStudentByIdAsync(studentId, cancellationToken);
        }
    }

    // LAYER: Broker
    public class StorageBroker : IStorageBroker
    {
        public async ValueTask<Student> SelectStudentByIdAsync(
            Guid studentId,
            CancellationToken cancellationToken)
        {
            // ✅ Token propagated to dependency (e.g., EF Core, HTTP client)
            // return await dbContext.Students
            //     .FindAsync(new object[] { studentId }, cancellationToken);

            return await Task.FromResult(new Student { Id = studentId });
        }
    }

    public interface IStudentCoordinationService
    {
        ValueTask<Student> RetrieveStudentByIdAsync(
            Guid studentId,
            CancellationToken cancellationToken);
    }

    public interface IStudentOrchestrationService
    {
        ValueTask<Student> RetrieveStudentByIdAsync(
            Guid studentId,
            CancellationToken cancellationToken);
    }

    public interface IStudentService
    {
        ValueTask<Student> RetrieveStudentByIdAsync(
            Guid studentId,
            CancellationToken cancellationToken);
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
}
