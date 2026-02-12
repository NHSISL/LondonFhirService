// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Hl7.Fhir.Model;
using LondonFhirService.Core.Brokers.Audits;
using LondonFhirService.Core.Brokers.Identifiers;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Models.Foundations.FhirReconciliations.Exceptions;
using LondonFhirService.Core.Models.Foundations.Patients.Exceptions;
using LondonFhirService.Core.Models.Foundations.Providers;
using LondonFhirService.Core.Models.Foundations.Providers.Exceptions;
using LondonFhirService.Core.Services.Foundations.FhirReconciliations.STU3;
using LondonFhirService.Core.Services.Foundations.Patients.STU3;
using LondonFhirService.Core.Services.Foundations.Providers;
using LondonFhirService.Core.Services.Orchestrations.Patients.STU3;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Orchestrations.Patients.STU3
{
    public partial class Stu3PatientOrchestrationServiceTests
    {
        private readonly Mock<IProviderService> providerServiceMock;
        private readonly Mock<IStu3PatientService> patientServiceMock;
        private readonly Mock<IStu3FhirReconciliationService> fhirReconciliationServiceMock;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly Mock<IAuditBroker> auditBrokerMock;
        private readonly Mock<IIdentifierBroker> identifierBrokerMock;
        private readonly IStu3PatientOrchestrationService patientOrchestrationService;

        public Stu3PatientOrchestrationServiceTests()
        {
            this.providerServiceMock = new Mock<IProviderService>();
            this.patientServiceMock = new Mock<IStu3PatientService>();
            this.auditBrokerMock = new Mock<IAuditBroker>();
            this.identifierBrokerMock = new Mock<IIdentifierBroker>();
            this.fhirReconciliationServiceMock = new Mock<IStu3FhirReconciliationService>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.patientOrchestrationService = new Stu3PatientOrchestrationService(
                providerService: providerServiceMock.Object,
                patientService: patientServiceMock.Object,
                fhirReconciliationService: fhirReconciliationServiceMock.Object,
                loggingBroker: loggingBrokerMock.Object,
                auditBroker: auditBrokerMock.Object,
                identifierBroker: identifierBrokerMock.Object);
        }

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private static DateTimeOffset GetRandomDateTimeOffset() =>
            new DateTimeRange(earliestDate: new DateTime()).GetValue();

        private static string GetRandomString() =>
            new MnemonicString(wordCount: GetRandomNumber()).GetValue();

        private static string GetRandomStringWithLengthOf(int length)
        {
            string result = new MnemonicString(wordCount: 1, wordMinLength: length, wordMaxLength: length).GetValue();

            return result.Length > length ? result.Substring(0, length) : result;
        }

        private static Expression<Func<Xeption, bool>> IsSameExceptionAs(Xeption expectedException) =>
            actualException => actualException.SameExceptionAs(expectedException);

        private static Expression<Func<Xeption, bool>> SameExceptionAs(Xeption expectedException) =>
            actualException => actualException.SameExceptionAs(expectedException);

        private static List<Bundle> CreateRandomBundles()
        {
            int numberOfBundles = GetRandomNumber();
            var bundles = new List<Bundle>();

            for (int i = 0; i < numberOfBundles; i++)
            {
                bundles.Add(CreateRandomBundle());
            }

            return bundles;
        }

        private static Bundle CreateRandomBundle() =>
            new Bundle
            {
                Id = GetRandomString(),
                Type = Bundle.BundleType.Searchset,
                Total = GetRandomNumber()
            };

        private static Provider CreateRandomActiveProvider() =>
            CreateProviderFiller(isActive: true).Create();

        private static Provider CreateRandomPrimaryProvider() =>
            CreateProviderFiller(isActive: true, isPrimary: true).Create();

        private static Provider CreateRandomInactiveProvider() =>
            CreateProviderFiller().Create();

        private static Filler<Provider> CreateProviderFiller(bool isActive = false, bool isPrimary = false)
        {
            DateTimeOffset now = DateTimeOffset.Now;
            var filler = new Filler<Provider>();

            DateTimeOffset activeFrom = isActive ? now.AddDays(-30) : now.AddDays(-60);
            DateTimeOffset activeTo = isActive ? now.AddDays(30) : now.AddDays(-30);

            filler.Setup()
                .OnType<DateTimeOffset>().Use(now)
                .OnProperty(provider => provider.FullyQualifiedName).Use(GetRandomStringWithLengthOf(500))
                .OnProperty(provider => provider.FhirVersion).Use("STU3")
                .OnProperty(provider => provider.IsActive).Use(isActive)
                .OnProperty(provider => provider.ActiveFrom).Use(activeFrom)
                .OnProperty(provider => provider.ActiveTo).Use(activeTo)
                .OnProperty(provider => provider.IsForComparisonOnly).Use(false)
                .OnProperty(provider => provider.IsPrimary).Use(isPrimary)
                .OnProperty(provider => provider.CreatedBy).Use(GetRandomString())
                .OnProperty(provider => provider.UpdatedBy).Use(GetRandomString());

            return filler;
        }

        public static TheoryData<Xeption> DependencyValidationExceptions()
        {
            string randomMessage = GetRandomString();
            string exceptionMessage = randomMessage;
            var innerException = new Xeption(exceptionMessage);

            return new TheoryData<Xeption>
            {
                new ProviderServiceValidationException(
                    message: "Provider validation errors occurred, please try again",
                    innerException),

                new ProviderServiceDependencyValidationException(
                    message: "Provider dependency validation errors occurred, please try again.",
                    innerException),

                new PatientServiceValidationException(
                    message: "Patient validation errors occurred, please try again.",
                    innerException),

                new PatientServiceDependencyValidationException(
                    message: "Patient dependency validation errors occurred, please try again.",
                    innerException),

                new FhirReconciliationServiceValidationException(
                    message: "FHIR reconciliation validation errors occurred, please try again",
                    innerException),

                new FhirReconciliationServiceDependencyValidationException(
                    message: "FHIR reconciliation dependency validation errors occurred, please try again.",
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
                new ProviderServiceDependencyException(
                    message: "Provider dependency error occurred, please try again",
                    innerException),

                new ProviderServiceException(
                    message: "Provider service error occurred, please contact support.",
                    innerException),

                new PatientServiceDependencyException(
                    message: "Patient dependency error occurred, please try again",
                    innerException),

                new PatientServiceException(
                    message: "Patient service error occurred, please contact support.",
                    innerException),

                new FhirReconciliationServiceDependencyException(
                    message: "FHIR reconciliation dependency error occurred, please try again",
                    innerException),

                new FhirReconciliationServiceException(
                    message: "FHIR reconciliation service error occurred, please contact support.",
                    innerException),
            };
        }
    }
}