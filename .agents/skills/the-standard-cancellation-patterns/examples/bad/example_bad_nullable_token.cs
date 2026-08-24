// ---
// skill: the-standard-cancellation-patterns
// type: example
// source-section: "3.3 Never use nullable CancellationToken"
// demonstrates: "tsc-csharp-cp-003 violation"
// ---

using System;
using System.Threading;
using System.Threading.Tasks;

namespace TheStandard.CancellationPatterns.Examples.Bad
{
    public partial class StudentService
    {
        // ❌ WRONG: CancellationToken is nullable
        public ValueTask<Student> RetrieveStudentByIdAsync(
            Guid studentId,
            CancellationToken? cancellationToken)
        {
            // This creates unnecessary null-checking complexity
            // and breaks the standard contract
            return default;
        }
    }

    public class Student
    {
        public Guid Id { get; set; }
    }
}
