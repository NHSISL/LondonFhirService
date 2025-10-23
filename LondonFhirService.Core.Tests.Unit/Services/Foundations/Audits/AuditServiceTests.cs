// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using ISL.Security.Client.Models.Foundations.Users;
using KellermanSoftware.CompareNetObjects;
using LondonFhirService.Core.Brokers.DateTimes;
using LondonFhirService.Core.Brokers.Identifiers;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Brokers.Securities;
using LondonFhirService.Core.Brokers.Storages.Sql;
using LondonFhirService.Core.Models.Foundations.Audits;
using LondonFhirService.Core.Services.Foundations.Audits;
using Microsoft.Data.SqlClient;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Audits
{
    public partial class AuditServiceTests
    {
        private readonly Mock<IStorageBroker> storageBrokerMock;
        private readonly Mock<IIdentifierBroker> identifierBrokerMock;
        private readonly Mock<IDateTimeBroker> dateTimeBrokerMock;
        private readonly Mock<ISecurityAuditBroker> securityAuditBrokerMock;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly IAuditService auditService;
        private readonly ICompareLogic compareLogic;

        public AuditServiceTests()
        {
            this.storageBrokerMock = new Mock<IStorageBroker>();
            this.identifierBrokerMock = new Mock<IIdentifierBroker>();
            this.dateTimeBrokerMock = new Mock<IDateTimeBroker>();
            this.securityAuditBrokerMock = new Mock<ISecurityAuditBroker>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();
            this.compareLogic = new CompareLogic();

            this.auditService = new AuditService(
                storageBroker: this.storageBrokerMock.Object,
                identifierBroker: this.identifierBrokerMock.Object,
                dateTimeBroker: this.dateTimeBrokerMock.Object,
                securityAuditBroker: this.securityAuditBrokerMock.Object,
                loggingBroker: this.loggingBrokerMock.Object);
        }

        private static Expression<Func<Xeption, bool>> SameExceptionAs(Xeption expectedException) =>
            actualException => actualException.SameExceptionAs(expectedException);

        private Expression<Func<Audit, bool>> SameAuditAs(Audit expectedAudit) =>
            actualAudit => this.compareLogic.Compare(expectedAudit, actualAudit).AreEqual;

        private static string GetRandomString() =>
            new MnemonicString(wordCount: GetRandomNumber()).GetValue();

        private static string GetRandomStringWithLengthOf(int length)
        {
            string result = new MnemonicString(wordCount: 1, wordMinLength: length, wordMaxLength: length).GetValue();

            return result.Length > length ? result.Substring(0, length) : result;
        }

        public static TheoryData<int> MinutesBeforeOrAfter()
        {
            int randomNumber = GetRandomNumber();
            int randomNegativeNumber = GetRandomNegativeNumber();

            return new TheoryData<int>
            {
                randomNumber,
                randomNegativeNumber
            };
        }

        private static SqlException GetSqlException() =>
            (SqlException)RuntimeHelpers.GetUninitializedObject(typeof(SqlException));

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private static int GetRandomNegativeNumber() =>
            -1 * new IntRange(min: 2, max: 10).GetValue();

        private static DateTimeOffset GetRandomDateTimeOffset() =>
            new DateTimeRange(earliestDate: new DateTime()).GetValue();

        private static Audit CreateRandomModifyAudit(
            DateTimeOffset dateTimeOffset,
            string userId)
        {
            int randomDaysInPast = GetRandomNegativeNumber();
            Audit randomAudit = CreateRandomAudit(dateTimeOffset, userId);

            randomAudit.CreatedDate =
                randomAudit.CreatedDate.AddDays(randomDaysInPast);

            return randomAudit;
        }

        private static List<Audit> CreateRandomAudits()
        {
            return CreateAuditFiller(dateTimeOffset: GetRandomDateTimeOffset())
                .Create(count: GetRandomNumber())
                    .ToList();
        }

        private static Audit CreateRandomAudit() =>
            CreateAuditFiller(dateTimeOffset: GetRandomDateTimeOffset()).Create();

        private static Audit CreateRandomAudit(DateTimeOffset dateTimeOffset) =>
            CreateAuditFiller(dateTimeOffset).Create();

        private static Filler<Audit> CreateAuditFiller(DateTimeOffset dateTimeOffset)
        {
            string user = Guid.NewGuid().ToString();
            var filler = new Filler<Audit>();

            filler.Setup()
                .OnType<DateTimeOffset>().Use(dateTimeOffset)
                .OnProperty(audit => audit.CreatedBy).Use(user)
                .OnProperty(audit => audit.UpdatedBy).Use(user);

            return filler;
        }

        private static Audit CreateRandomAudit(
            DateTimeOffset dateTimeOffset,
            string userId) =>
            CreateAuditFiller(dateTimeOffset, userId).Create();

        private static Filler<Audit> CreateAuditFiller(
            DateTimeOffset dateTimeOffset,
            string userId)
        {
            var filler = new Filler<Audit>();

            filler.Setup()
                .OnType<DateTimeOffset>().Use(dateTimeOffset)
                .OnProperty(audit => audit.CreatedBy).Use(userId)
                .OnProperty(audit => audit.UpdatedBy).Use(userId);

            return filler;
        }

        private User CreateRandomUser(string userId = "")
        {
            return new User(
                userId: string.IsNullOrWhiteSpace(userId) ? GetRandomStringWithLengthOf(255) : userId,
                givenName: GetRandomString(),
                surname: GetRandomString(),
                displayName: GetRandomString(),
                email: GetRandomString(),
                jobTitle: GetRandomString(),
                roles: new List<string> { GetRandomString() },

                claims: new List<System.Security.Claims.Claim>
                {
                    new System.Security.Claims.Claim(type: GetRandomString(), value: GetRandomString())
                });
        }
    }
}