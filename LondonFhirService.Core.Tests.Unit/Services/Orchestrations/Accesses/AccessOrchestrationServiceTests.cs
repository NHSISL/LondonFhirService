// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using ISL.Security.Client.Models.Foundations.Users;
using LondonFhirService.Core.Brokers.Audits;
using LondonFhirService.Core.Brokers.DateTimes;
using LondonFhirService.Core.Brokers.Identifiers;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Brokers.Securities;
using LondonFhirService.Core.Models.Foundations.Consumers;
using LondonFhirService.Core.Services.Foundations.ConsumerAccesses;
using LondonFhirService.Core.Services.Foundations.Consumers;
using LondonFhirService.Core.Services.Foundations.PdsDatas;
using LondonFhirService.Core.Services.Orchestrations.Accesses;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Orchestrations.Accesses
{
    public partial class AccessOrchestrationServiceTests
    {
        private readonly Mock<IConsumerService> consumerServiceMock;
        private readonly Mock<IConsumerAccessService> consumerAccessServiceMock;
        private readonly Mock<IPdsDataService> pdsDataServiceMock;
        private readonly Mock<IAuditBroker> auditBrokerMock;
        private readonly Mock<ISecurityBroker> securityBrokerMock;
        private readonly Mock<IDateTimeBroker> dateTimeBrokerMock;
        private readonly Mock<IIdentifierBroker> identifierBrokerMock;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly AccessOrchestrationService accessOrchestrationService;

        public AccessOrchestrationServiceTests()
        {
            this.consumerServiceMock = new Mock<IConsumerService>();
            this.consumerAccessServiceMock = new Mock<IConsumerAccessService>();
            this.pdsDataServiceMock = new Mock<IPdsDataService>();
            this.auditBrokerMock = new Mock<IAuditBroker>();
            this.securityBrokerMock = new Mock<ISecurityBroker>();
            this.dateTimeBrokerMock = new Mock<IDateTimeBroker>();
            this.identifierBrokerMock = new Mock<IIdentifierBroker>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.accessOrchestrationService = new AccessOrchestrationService(
                consumerService: this.consumerServiceMock.Object,
                consumerAccessService: this.consumerAccessServiceMock.Object,
                pdsDataService: this.pdsDataServiceMock.Object,
                auditBroker: this.auditBrokerMock.Object,
                securityBroker: this.securityBrokerMock.Object,
                dateTimeBroker: this.dateTimeBrokerMock.Object,
                identifierBroker: this.identifierBrokerMock.Object,
                loggingBroker: this.loggingBrokerMock.Object);
        }

        private static DateTimeOffset GetRandomDateTimeOffset() =>
            new DateTimeRange(earliestDate: new DateTime()).GetValue();

        private static int GetRandomNumber() =>
            new IntRange(max: 15, min: 2).GetValue();

        private static int GetRandomNegativeNumber() =>
            -1 * new IntRange(min: 2, max: 10).GetValue();

        private static string GetRandomString() =>
            new MnemonicString().GetValue();

        private static string GetRandomStringWithLength(int length)
        {
            string result = new MnemonicString(wordCount: 1, wordMinLength: length, wordMaxLength: length).GetValue();

            return result.Length > length ? result.Substring(0, length) : result;
        }

        private static User CreateRandomUser(string userId)
        {
            string randomString = GetRandomString();
            Guid randomGuid = Guid.NewGuid();

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

            return Enumerable.Range(start: 1, count: GetRandomNumber())
                .Select(_ => new Claim(type: randomString, value: randomString)).ToList();
        }

        private static Consumer CreateRandomConsumer(string userId = "") =>
            CreateConsumerFiller(GetRandomDateTimeOffset(), userId).Create();

        private static Filler<Consumer> CreateConsumerFiller(DateTimeOffset dateTimeOffset, string userId = "")
        {
            userId = string.IsNullOrEmpty(userId) ? Guid.NewGuid().ToString() : userId;
            var filler = new Filler<Consumer>();

            filler.Setup()
                .OnType<DateTimeOffset>().Use(dateTimeOffset)
                .OnType<DateTimeOffset?>().Use(dateTimeOffset)
                .OnProperty(consumer => consumer.UserId).Use(userId)
                .OnProperty(consumer => consumer.Name).Use(GetRandomStringWithLength(255))
                .OnProperty(consumer => consumer.CreatedBy).Use(userId)
                .OnProperty(consumer => consumer.UpdatedBy).Use(userId)
                .OnProperty(consumer => consumer.ConsumerAccesses).IgnoreIt();

            return filler;
        }

        private static Expression<Func<Xeption, bool>> SameExceptionAs(
            Xeption expectedException)
        {
            return actualException =>
                actualException.SameExceptionAs(expectedException);
        }
    }
}
