// ---
// skill: the-standard-cancellation-patterns
// type: example
// source-section: "3.4 Use the canonical parameter name"
// demonstrates: "tsc-csharp-cp-004 violation"
// ---

using System;
using System.Threading;
using System.Threading.Tasks;

namespace TheStandard.CancellationPatterns.Examples.Bad
{
    public partial class StudentService
    {
        // ❌ WRONG: Parameter named "ct"
        public ValueTask<Student> RetrieveStudentByIdAsync(
            Guid studentId,
            CancellationToken ct = default)
        {
            return default;
        }

        // ❌ WRONG: Parameter named "token"
        public ValueTask<Student> UpdateStudentAsync(
            Student student,
            CancellationToken token = default)
        {
            return default;
        }

        // ❌ WRONG: Parameter named "cancelToken"
        public ValueTask DeleteStudentAsync(
            Guid studentId,
            CancellationToken cancelToken = default)
        {
            return default;
        }
    }

    public class Student
    {
        public Guid Id { get; set; }
    }
}
