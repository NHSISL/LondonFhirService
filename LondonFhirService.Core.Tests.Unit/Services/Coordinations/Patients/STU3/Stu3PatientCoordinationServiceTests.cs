// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Linq.Expressions;
using System;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using LondonFhirService.Core.Brokers.AuditAndMetrics;
using LondonFhirService.Core.Brokers.Identifiers;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Models.Foundations.Providers;
using LondonFhirService.Core.Models.Orchestrations.FhirReconciliations.Exceptions;
using LondonFhirService.Core.Models.Orchestrations.Patients.Exceptions;
using LondonFhirService.Core.Services.Coordinations.Patients.STU3;
using LondonFhirService.Core.Services.Orchestrations.FhirReconciliations.STU3;
using LondonFhirService.Core.Services.Orchestrations.Patients.STU3;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Coordinations.Patients.STU3
{
    public partial class Stu3PatientCoordinationServiceTests
    {
        private readonly Mock<IStu3PatientOrchestrationService> patientOrchestrationServiceMock;
        private readonly Mock<IStu3FhirReconciliationService> fhirReconciliationServiceMock;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly Mock<IAuditAndMetricBroker> auditAndMetricBrokerMock;
        private readonly Mock<IIdentifierBroker> identifierBrokerMock;
        private readonly IStu3PatientCoordinationService patientCoordinationService;

        public Stu3PatientCoordinationServiceTests()
        {
            this.patientOrchestrationServiceMock = new Mock<IStu3PatientOrchestrationService>();
            this.fhirReconciliationServiceMock = new Mock<IStu3FhirReconciliationService>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();
            this.auditAndMetricBrokerMock = new Mock<IAuditAndMetricBroker>();
            this.identifierBrokerMock = new Mock<IIdentifierBroker>();

            this.patientCoordinationService = new Stu3PatientCoordinationService(
                patientOrchestrationService: patientOrchestrationServiceMock.Object,
                fhirReconciliationService: fhirReconciliationServiceMock.Object,
                loggingBroker: loggingBrokerMock.Object,
                auditAndMetricBroker: auditAndMetricBrokerMock.Object,
                identifierBroker: identifierBrokerMock.Object);
        }

        private static string GetRandomStringWithLength(int length)
        {
            string result = new MnemonicString(wordCount: 1, wordMinLength: length, wordMaxLength: length).GetValue();

            return result.Length > length ? result.Substring(0, length) : result;
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

        private static string SerializeBundle(Bundle bundle)
        {
            var serializer = new FhirJsonSerializer();
            return serializer.SerializeToString(bundle);
        }

        private static List<(string Provider, string Json)> CreateRandomBundles()
        {
            var items = new List<(string Provider, string Json)>();

            for (int index = 0; index < GetRandomNumber(); index++)
            {
                items.Add((GetRandomString(), SerializeBundle(CreateRandomBundle())));
            }

            return items;
        }

        private static Provider CreateRandomProvider()
        {
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            var filler = new Filler<Provider>();

            filler.Setup()
                .OnType<DateTimeOffset>().Use(randomDateTimeOffset)
                .OnType<DateTimeOffset?>().Use(randomDateTimeOffset);

            return filler.Create();
        }

        public static TheoryData<Xeption> DependencyValidationExceptions()
        {
            string randomMessage = GetRandomString();
            string exceptionMessage = randomMessage;
            var innerException = new Xeption(exceptionMessage);

            return new TheoryData<Xeption>
            {
                new PatientOrchestrationValidationException(
                    message: "Patient orchestration validation error occurred, please try again.",
                    innerException),

                new PatientOrchestrationDependencyValidationException(
                    message: "Patient orchestration dependency validation error occurred, please try again.",
                    innerException),

                new FhirReconciliationOrchestrationValidationException(
                    message: "FHIR reconciliation orchestration validation error occurred, please try again.",
                    innerException),

                new FhirReconciliationOrchestrationDependencyValidationException(
                    message: "FHIR reconciliation orchestration dependency validation error occurred, please try again.",
                    innerException)
            };
        }

        public static TheoryData<Xeption> DependencyExceptions()
        {
            string randomMessage = GetRandomString();
            string exceptionMessage = randomMessage;
            var innerException = new Xeption(exceptionMessage);

            return new TheoryData<Xeption>
            {
                new PatientOrchestrationDependencyException(
                    message: "Patient orchestration dependency error occurred, please try again.",
                    innerException),

                new PatientOrchestrationServiceException(
                    message: "Patient orchestration service error occurred, please contact support.",
                    innerException),

                new FhirReconciliationOrchestrationDependencyException(
                    message: "FHIR reconciliation orchestration dependency error occurred, fix the errors and try again.",
                    innerException),

                new FhirReconciliationOrchestrationServiceException(
                    message: "FHIR reconciliation orchestration service error occurred, please contact support.",
                    innerException)
            };
        }
    }
}
