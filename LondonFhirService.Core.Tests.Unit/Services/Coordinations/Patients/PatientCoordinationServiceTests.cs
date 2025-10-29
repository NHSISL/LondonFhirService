// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq.Expressions;
using Hl7.Fhir.Model;
using KellermanSoftware.CompareNetObjects;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Models.Orchestrations.Accesses.Exceptions;
using LondonFhirService.Core.Models.Orchestrations.Patients.Exceptions;
using LondonFhirService.Core.Services.Coordinations.Patients;
using LondonFhirService.Core.Services.Orchestrations.Accesses;
using LondonFhirService.Core.Services.Orchestrations.Patients;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Coordinations.Patients
{
    public partial class PatientCoordinationServiceTests
    {
        private readonly Mock<IAccessOrchestrationService> accessOrchestrationServiceMock;
        private readonly Mock<IPatientOrchestrationService> patientOrchestrationServiceMock;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly ICompareLogic compareLogic;
        private readonly IPatientCoordinationService patientCoordinationService;

        public PatientCoordinationServiceTests()
        {
            this.accessOrchestrationServiceMock = new Mock<IAccessOrchestrationService>();
            this.patientOrchestrationServiceMock = new Mock<IPatientOrchestrationService>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();
            this.compareLogic = new CompareLogic();

            this.patientCoordinationService = new PatientCoordinationService(
                accessOrchestrationService: accessOrchestrationServiceMock.Object,
                patientOrchestrationService: patientOrchestrationServiceMock.Object,
                loggingBroker: loggingBrokerMock.Object);
        }

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private static DateTimeOffset GetRandomDateTimeOffset() =>
            new DateTimeRange(earliestDate: new DateTime()).GetValue();

        private static string GetRandomString() =>
            new MnemonicString(wordCount: GetRandomNumber()).GetValue();
        private static Expression<Func<Xeption, bool>> IsSameExceptionAs(Xeption expectedException) =>
            actualException => actualException.SameExceptionAs(expectedException);

        private static Expression<Func<Xeption, bool>> SameExceptionAs(Xeption expectedException) =>
            actualException => actualException.SameExceptionAs(expectedException);

        private static Bundle CreateRandomBundle() =>
            new Bundle
            {
                Id = GetRandomString(),
                Type = Bundle.BundleType.Searchset,
                Total = GetRandomNumber()
            };

        public static TheoryData<Xeption> DependencyValidationExceptions()
        {
            string randomMessage = GetRandomString();
            string exceptionMessage = randomMessage;
            var innerException = new Xeption(exceptionMessage);

            return new TheoryData<Xeption>
            {
                new AccessOrchestrationValidationException(
                    message: "Access orchestration validation error occured, please try again",
                    innerException),

                new AccessOrchestrationDependencyValidationException(
                    message: "Access orchestration dependency validation error occurred, please try again.",
                    innerException),

                new PatientOrchestrationValidationException(
                    message: "Patient orchestration validation error occured, please try again.",
                    innerException),

                new PatientOrchestrationDependencyValidationException(
                    message: "Patient orchestration dependency validation error occurred, please try again.",
                    innerException),
            };
        }

        public static TheoryData<Xeption> DependencyExceptions()
        {
            string randomMessage = GetRandomString();
            string exceptionMessage = randomMessage;
            var innerException = new Xeption(exceptionMessage);

            return new TheoryData<Xeption>
            {
                new AccessOrchestrationDependencyException(
                    message: "Access orchestration dependency error occured, please try again.",
                    innerException),

                new AccessOrchestrationServiceException(
                    message: "Access orchestration service error occurred, please contact support.",
                    innerException),

                new PatientOrchestrationDependencyException(
                    message: "Patient orchestration dependency error occured, please try again.",
                    innerException),

                new PatientOrchestrationServiceException(
                    message: "Patient orchestration service error occurred, please contact support.",
                    innerException)
            };
        }
    }
}
