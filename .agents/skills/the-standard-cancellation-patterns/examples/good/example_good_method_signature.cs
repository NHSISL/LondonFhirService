// ---
// skill: the-standard-cancellation-patterns
// type: example
// source-section: "3.0 Method Signature Conventions"
// demonstrates: "tsc-csharp-cp-001, tsc-csharp-cp-002, tsc-csharp-cp-003, tsc-csharp-cp-004"
// ---

using System;
using System.Threading;
using System.Threading.Tasks;

namespace TheStandard.CancellationPatterns.Examples.Good
{
    public partial class StudentService
    {
        // ✅ CORRECT: CancellationToken is the last parameter
        // ✅ CORRECT: Defaults to `default`
        // ✅ CORRECT: Not nullable
        // ✅ CORRECT: Named `cancellationToken`
        public ValueTask<Student> RetrieveStudentByIdAsync(
            Guid studentId,
            CancellationToken cancellationToken = default)
        {
            // Implementation
            return default;
        }

        // ✅ CORRECT: Multi-parameter method with CancellationToken last
        public ValueTask<IEnumerable<Student>> RetrieveStudentsByFilterAsync(
            string firstName,
            string lastName,
            int? age,
            DateTimeOffset? enrollmentDate,
            CancellationToken cancellationToken = default)
        {
            // Implementation
            return default;
        }
    }

    public class Student
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
