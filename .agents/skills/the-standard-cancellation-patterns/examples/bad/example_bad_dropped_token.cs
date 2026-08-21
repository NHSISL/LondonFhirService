// ---
// skill: the-standard-cancellation-patterns
// type: example
// source-section: "6.1 The same token MUST flow through all layers"
// demonstrates: "tsc-csharp-cp-006, tsc-csharp-cp-007 violation"
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

        // ❌ WRONG: Token accepted but not propagated to dependency
        public async ValueTask<Student> RetrieveStudentAsync(
            Guid studentId,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // ❌ Token is silently dropped here
            return await this.storageBroker.SelectStudentByIdAsync(studentId);
        }
    }

    public interface IStorageBroker
    {
        // Broker method accepts CancellationToken but caller doesn't pass it
        ValueTask<Student> SelectStudentByIdAsync(
            Guid studentId,
            CancellationToken cancellationToken = default);
    }

    public class Student
    {
        public Guid Id { get; set; }
    }
}
