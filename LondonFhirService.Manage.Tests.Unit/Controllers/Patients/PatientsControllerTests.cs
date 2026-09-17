// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using LondonFhirService.Manage.Controllers.Patients;
using LondonFhirService.Manage.Models.Foundations.Patients;
using LondonFhirService.Manage.Models.Foundations.Patients.Exceptions;
using LondonFhirService.Manage.Services.Foundations.Patients;
using Moq;
using RESTFulSense.Controllers;
using Tynamix.ObjectFiller;
using Xeptions;

namespace LondonFhirService.Manage.Tests.Unit.Controllers.Patients
{
    public partial class PatientsControllerTests : RESTFulController
    {
        private readonly Mock<IPatientService> patientServiceMock;
        private readonly PatientsController patientsController;

        public PatientsControllerTests()
        {
            this.patientServiceMock = new Mock<IPatientService>();
            this.patientsController = new PatientsController(this.patientServiceMock.Object);
        }

        /// <summary>
        /// Only the exceptions whose answer is built from Exception.Data, which is what
        /// RESTFulSense's BadRequest(Exception) reads. The two dependency exceptions answer with
        /// what the upstream said instead, so they are covered on their own below.
        /// </summary>
        public static TheoryData<Xeption> ValidationExceptions()
        {
            var someInnerException = new Xeption();
            string someMessage = GetRandomString();

            return new TheoryData<Xeption>
            {
                new PatientServiceValidationException(
                    message: someMessage,
                    innerException: someInnerException)
            };
        }

        public static TheoryData<Xeption> ServerExceptions()
        {
            var someInnerException = new Xeption();
            string someMessage = GetRandomString();

            return new TheoryData<Xeption>
            {
                new PatientServiceException(
                    message: someMessage,
                    innerException: someInnerException)
            };
        }

        /// <summary>
        /// What a provider actually sends back when it refuses, rather than a random string. The
        /// assertions using it are about an identifiable payload reaching the operator who asked
        /// and nothing else, so it has to look like one.
        /// </summary>
        private static string CreateRefusalBody() =>
            "{\"resourceType\":\"OperationOutcome\",\"issue\":[{\"severity\":\"error\"," +
                "\"diagnostics\":\"" + GetRandomString() + "\"}]}";

        private static string GetRandomString() =>
            new MnemonicString(wordCount: GetRandomNumber()).GetValue();

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private static string GetRandomDateOfBirth() =>
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-GetRandomNumber() * 1000))
                .ToString("yyyy-MM-dd");

        private static StructuredRecordRequest CreateRandomStructuredRecordRequest() =>
            new StructuredRecordRequest
            {
                ClientId = GetRandomString(),
                ClientSecret = GetRandomString(),
                Scope = GetRandomString(),
                GrantType = GetRandomString(),
                NhsNumber = GetRandomString(),
                DateOfBirth = GetRandomDateOfBirth(),
                DemographicsOnly = false
            };
    }
}
