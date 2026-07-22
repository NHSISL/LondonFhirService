// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using ISL.Security.Client.Models.Foundations.Users;
using LondonFhirService.Core.Brokers.Audits;
using LondonFhirService.Core.Brokers.DateTimes;
using LondonFhirService.Core.Brokers.Hashing;
using LondonFhirService.Core.Brokers.Identifiers;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Brokers.Securities;
using LondonFhirService.Core.Models.Brokers.ConsumerAccesses;
using LondonFhirService.Core.Models.Foundations.ConsumerAccesses.Exceptions;
using LondonFhirService.Core.Models.Orchestrations.Accesses;
using LondonFhirService.Core.Services.Foundations.ConsumerAccesses;
using LondonFhirService.Core.Services.Orchestrations.Accesses;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Orchestrations.Accesses
{
    public partial class AccessOrchestrationServiceTests
    {
        private readonly Mock<IConsumerAccessService> consumerAccessServiceMock;
        private readonly Mock<IAuditBroker> auditBrokerMock;
        private readonly Mock<ISecurityBroker> securityBrokerMock;
        private readonly Mock<IDateTimeBroker> dateTimeBrokerMock;
        private readonly Mock<IIdentifierBroker> identifierBrokerMock;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly Mock<IHashBroker> hashBrokerMock;
        private readonly AccessConfigurations accessConfigurations;
        private readonly AccessOrchestrationService accessOrchestrationService;

        public AccessOrchestrationServiceTests()
        {
            this.consumerAccessServiceMock = new Mock<IConsumerAccessService>();
            this.auditBrokerMock = new Mock<IAuditBroker>();
            this.securityBrokerMock = new Mock<ISecurityBroker>();
            this.dateTimeBrokerMock = new Mock<IDateTimeBroker>();
            this.identifierBrokerMock = new Mock<IIdentifierBroker>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();
            this.hashBrokerMock = new Mock<IHashBroker>();

            this.accessConfigurations = new AccessConfigurations
            {
                UseHashedNhsNumber = true,
                HashPepper = GetRandomStringWithLength(100),
                CheckAccessPermissions = true
            };

            this.accessOrchestrationService = new AccessOrchestrationService(
                consumerAccessService: this.consumerAccessServiceMock.Object,
                auditBroker: this.auditBrokerMock.Object,
                securityBroker: this.securityBrokerMock.Object,
                dateTimeBroker: this.dateTimeBrokerMock.Object,
                identifierBroker: this.identifierBrokerMock.Object,
                loggingBroker: this.loggingBrokerMock.Object,
                hashBroker: this.hashBrokerMock.Object,
                accessConfigurations: this.accessConfigurations);
        }

        private static string GetRandomString() =>
            new MnemonicString().GetValue();

        private static string GetRandomStringWithLength(int length)
        {
            string result = new MnemonicString(wordCount: 1, wordMinLength: length, wordMaxLength: length).GetValue();

            return result.Length > length ? result.Substring(0, length) : result;
        }

        private static Guid GetRandomGuid() =>
            Guid.NewGuid();

        private static User CreateRandomUser(string userId)
        {
            string randomString = GetRandomString();

            User user = new User(
                userId: userId,
                givenName: randomString,
                surname: randomString,
                displayName: randomString,
                email: randomString,
                jobTitle: randomString,
                roles: new List<string> { randomString },
                claims: CreateRandomClaims());

            return user;
        }

        private static List<Claim> CreateRandomClaims()
        {
            string randomString = GetRandomString();

            return Enumerable.Range(start: 1, count: 2)
                .Select(_ => new Claim(type: randomString, value: randomString)).ToList();
        }

        private static ConsumerAccess CreateRandomConsumerAccess(bool isAccessAllowed) =>
            CreateConsumerAccessFiller(isAccessAllowed).Create();

        private static Filler<ConsumerAccess> CreateConsumerAccessFiller(bool isAccessAllowed)
        {
            var filler = new Filler<ConsumerAccess>();

            filler.Setup()
                .OnProperty(consumerAccess => consumerAccess.IsAccessAllowed).Use(isAccessAllowed);

            return filler;
        }

        private static JsonSerializerOptions CreateJsonSerializerOptions() =>
            new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };

        private static Expression<Func<Xeption, bool>> SameExceptionAs(
            Xeption expectedException)
        {
            return actualException =>
                actualException.SameExceptionAs(expectedException);
        }

        public static TheoryData<Xeption> DependencyValidationExceptions()
        {
            string randomMessage = GetRandomString();
            string exceptionMessage = randomMessage;
            var innerException = new Xeption(exceptionMessage);

            return new TheoryData<Xeption>
            {
                new ConsumerAccessServiceValidationException(
                    message: "Consumer access validation errors occurred, please try again",
                    innerException),

                new ConsumerAccessServiceDependencyValidationException(
                    message: "Consumer access dependency validation occurred, please try again.",
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
