// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using Hl7.Fhir.Model;
using LondonFhirService.Api.Controllers;
using LondonFhirService.Core.Services.Coordinations.Patients;
using Moq;
using RESTFulSense.Controllers;
using Tynamix.ObjectFiller;

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

    }
}
