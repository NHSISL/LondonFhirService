// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using Hl7.Fhir.Model;
using LondonFhirService.Api.Controllers;
using LondonFhirService.Core.Models.Coordinations.Patients.Exceptions;
using LondonFhirService.Core.Services.Coordinations.Patients;
using Moq;
using RESTFulSense.Controllers;
using Tynamix.ObjectFiller;
using Xeptions;

namespace LondonFhirService.Api.Tests.Unit.Controllers.Patients
{
    public partial class PatientsControllerTests : RESTFulController
    {
        private readonly Mock<IPatientCoordinationService> patientCoordinationServiceMock;
        private readonly PatientsController patientsController;

        public PatientsControllerTests()
        {
            this.patientCoordinationServiceMock = new Mock<IPatientCoordinationService>();

            this.patientsController = new PatientsController(
                this.patientCoordinationServiceMock.Object);
        }

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private static DateTimeOffset GetRandomDateTimeOffset() =>
            new DateTimeRange(earliestDate: new DateTime()).GetValue();

        private static string GetRandomString() =>
            new MnemonicString(wordCount: GetRandomNumber()).GetValue();

        private static Bundle CreateRandomBundle() =>
            new Bundle
            {
                Id = GetRandomString(),
                Type = Bundle.BundleType.Searchset,
                Total = GetRandomNumber()
            };

        public static TheoryData<Xeption> ValidationExceptions()
        {
            var someInnerException = new Xeption();
            string someMessage = GetRandomString();

            return new TheoryData<Xeption>
            {
                new PatientCoordinationValidationException(
                    message: someMessage,
                    innerException: someInnerException),

                new PatientCoordinationDependencyValidationException(
                    message: someMessage,
                    innerException: someInnerException)
            };
        }
    }
}
