// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using ISL.Security.Client.Models.Foundations.Users;
using LondonFhirService.Core.Brokers.Audits;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Brokers.Securities;
using LondonFhirService.Core.Models.Brokers.ConsumerAccesses;
using LondonFhirService.Core.Models.Foundations.ConsumerAccesses.Exceptions;
using LondonFhirService.Core.Models.Foundations.Patients.Exceptions;
using LondonFhirService.Core.Models.Foundations.Providers;
using LondonFhirService.Core.Models.Foundations.Providers.Exceptions;
using LondonFhirService.Core.Models.Orchestrations.Accesses;
using LondonFhirService.Core.Services.Foundations.ConsumerAccesses;
using LondonFhirService.Core.Services.Foundations.Patients.STU3;
using LondonFhirService.Core.Services.Foundations.Providers;
using LondonFhirService.Core.Services.Orchestrations.Patients.STU3;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;
using Claim = System.Security.Claims.Claim;

namespace LondonFhirService.Core.Tests.Unit.Services.Orchestrations.Patients.STU3
{
    public partial class Stu3PatientOrchestrationServiceTests
    {
        private readonly Mock<IProviderService> providerServiceMock;
        private readonly Mock<IStu3PatientService> patientServiceMock;
        private readonly Mock<IConsumerAccessService> consumerAccessServiceMock;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly Mock<IAuditBroker> auditBrokerMock;
        private readonly Mock<ISecurityBroker> securityBrokerMock;
        private readonly AccessConfigurations accessConfigurations;
        private readonly IStu3PatientOrchestrationService patientOrchestrationService;

        public Stu3PatientOrchestrationServiceTests()
        {
            this.providerServiceMock = new Mock<IProviderService>();
            this.patientServiceMock = new Mock<IStu3PatientService>();
            this.consumerAccessServiceMock = new Mock<IConsumerAccessService>();
            this.auditBrokerMock = new Mock<IAuditBroker>();
            this.securityBrokerMock = new Mock<ISecurityBroker>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.accessConfigurations = new AccessConfigurations
            {
                CheckAccessPermissions = true
            };

            this.patientOrchestrationService = CreateOrchestrationService(this.accessConfigurations);
        }

        private Stu3PatientOrchestrationService CreateOrchestrationService(
            AccessConfigurations accessConfigurations) =>
            new Stu3PatientOrchestrationService(
                providerService: this.providerServiceMock.Object,
                patientService: this.patientServiceMock.Object,
                consumerAccessService: this.consumerAccessServiceMock.Object,
                auditBroker: this.auditBrokerMock.Object,
                securityBroker: this.securityBrokerMock.Object,
                loggingBroker: this.loggingBrokerMock.Object,
                accessConfigurations: accessConfigurations);

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

        private static Expression<Func<ValidateAccessRequest, bool>> SameValidateAccessRequestAs(
            string consumerUserId,
            string nhsNumber,
            Guid correlationId)
        {
            return actualRequest =>
                actualRequest.ConsumerUserId == consumerUserId
                    && actualRequest.NhsNumber == nhsNumber
                    && actualRequest.CorrelationId == correlationId;
        }

        private static User CreateRandomUser(string userId)
        {
            string randomString = GetRandomString();

            return new User(
                userId: userId,
                givenName: randomString,
                surname: randomString,
                displayName: randomString,
                email: randomString,
                jobTitle: randomString,
                roles: new List<string> { randomString },
                claims: CreateRandomClaims());
        }

        private static List<Claim> CreateRandomClaims()
        {
            string randomString = GetRandomString();

            return Enumerable.Range(start: 1, count: GetRandomNumber())
                .Select(_ => new Claim(type: randomString, value: randomString)).ToList();
        }

        private static ConsumerAccess CreateRandomConsumerAccess(bool isAccessAllowed) =>
            new ConsumerAccess
            {
                NhsNumber = GetRandomString(),
                ConsumerId = GetRandomString(),
                ConsumerOrgCode = GetRandomString(),
                IsAccessAllowed = isAccessAllowed,
                AllowedViaInformationSharingAgreements = CreateRandomStrings(),
                AllowedViaOrganisations = CreateRandomStrings(),

                Reasons = new List<AccessReason>
                {
                    new AccessReason { Code = GetRandomString(), Message = GetRandomString() }
                },

                CorrelationId = Guid.NewGuid()
            };

        private static List<string> CreateRandomStrings() =>
            Enumerable.Range(start: 1, count: GetRandomNumber())
                .Select(_ => GetRandomString()).ToList();

        private static List<(string Provider, string Json)> CreateRandomBundles()
        {
            int numberOfBundles = GetRandomNumber();
            var items = new List<(string Provider, string Json)>();

            for (int i = 0; i < numberOfBundles; i++)
            {
                items.Add((GetRandomString(), SerializeBundle(CreateRandomBundle())));
            }

            return items;
        }

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
                    innerException)
            };
        }

        public static TheoryData<Xeption> ConsumerAccessDependencyValidationExceptions()
        {
            string randomMessage = GetRandomString();
            var innerException = new Xeption(randomMessage);

            return new TheoryData<Xeption>
            {
                new ConsumerAccessServiceValidationException(
                    message: "Consumer access validation errors occurred, please try again",
                    innerException)
            };
        }

        public static TheoryData<Xeption> ConsumerAccessDependencyExceptions()
        {
            string randomMessage = GetRandomString();
            var innerException = new Xeption(randomMessage);

            return new TheoryData<Xeption>
            {
                new ConsumerAccessServiceDependencyException(
                    message: "Consumer access dependency error occurred, please contact support.",
                    innerException),

                new ConsumerAccessServiceException(
                    message: "Consumer access service error occurred, please contact support.",
                    innerException)
            };
        }
    }
}
