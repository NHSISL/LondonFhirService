// ---
// skill: the-standard-cancellation-patterns
// type: example
// source-section: "8.0 Parallel Orchestration"
// demonstrates: "tsc-csharp-cp-015"
// ---

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TheStandard.CancellationPatterns.Examples.Good
{
    public class StudentOrchestrationService
    {
        private readonly IStudentService studentService;
        private readonly IAddressService addressService;
        private readonly IEnrollmentService enrollmentService;

        public StudentOrchestrationService(
            IStudentService studentService,
            IAddressService addressService,
            IEnrollmentService enrollmentService)
        {
            this.studentService = studentService;
            this.addressService = addressService;
            this.enrollmentService = enrollmentService;
        }

        // ✅ CORRECT: Same token passed to all parallel operations
        public async ValueTask<StudentProfile> RetrieveStudentProfileAsync(
            Guid studentId,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // ✅ All tasks receive the same cancellationToken
            Task<Student> studentTask =
                this.studentService.RetrieveStudentByIdAsync(
                    studentId,
                    cancellationToken);

            Task<Address> addressTask =
                this.addressService.RetrieveAddressByStudentIdAsync(
                    studentId,
                    cancellationToken);

            Task<IEnumerable<Enrollment>> enrollmentsTask =
                this.enrollmentService.RetrieveEnrollmentsByStudentIdAsync(
                    studentId,
                    cancellationToken);

            // ✅ Await all tasks together
            await Task.WhenAll(studentTask, addressTask, enrollmentsTask);

            return new StudentProfile
            {
                Student = await studentTask,
                Address = await addressTask,
                Enrollments = await enrollmentsTask
            };
        }
    }

    public interface IStudentService
    {
        ValueTask<Student> RetrieveStudentByIdAsync(
            Guid studentId,
            CancellationToken cancellationToken);
    }

    public interface IAddressService
    {
        ValueTask<Address> RetrieveAddressByStudentIdAsync(
            Guid studentId,
            CancellationToken cancellationToken);
    }

    public interface IEnrollmentService
    {
        ValueTask<IEnumerable<Enrollment>> RetrieveEnrollmentsByStudentIdAsync(
            Guid studentId,
            CancellationToken cancellationToken);
    }

    public class StudentProfile
    {
        public Student Student { get; set; }
        public Address Address { get; set; }
        public IEnumerable<Enrollment> Enrollments { get; set; }
    }

    public class Student
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class Address
    {
        public Guid Id { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
    }

    public class Enrollment
    {
        public Guid Id { get; set; }
        public Guid StudentId { get; set; }
        public Guid CourseId { get; set; }
    }
}
