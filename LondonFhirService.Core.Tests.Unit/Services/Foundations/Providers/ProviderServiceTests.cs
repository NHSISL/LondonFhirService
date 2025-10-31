// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using LondonFhirService.Core.Brokers.DateTimes;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Brokers.Securities;
using LondonFhirService.Core.Brokers.Storages.Sql;
using LondonFhirService.Core.Models.Foundations.Providers;
using LondonFhirService.Core.Services.Foundations.Providers;
using Microsoft.Data.SqlClient;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Providers
{
    public partial class ProviderServiceTests
    {
        private readonly Mock<IStorageBroker> storageBrokerMock;
        private readonly Mock<IDateTimeBroker> dateTimeBrokerMock;
        private readonly Mock<ISecurityAuditBroker> securityAuditBrokerMock;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly IProviderService providerService;

        public ProviderServiceTests()
        {
            this.storageBrokerMock = new Mock<IStorageBroker>();
            this.dateTimeBrokerMock = new Mock<IDateTimeBroker>();
            this.securityAuditBrokerMock = new Mock<ISecurityAuditBroker>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.providerService = new ProviderService(
                storageBroker: this.storageBrokerMock.Object,
                dateTimeBroker: this.dateTimeBrokerMock.Object,
                securityAuditBroker: this.securityAuditBrokerMock.Object,
                loggingBroker: this.loggingBrokerMock.Object);
        }

        private static Expression<Func<Xeption, bool>> SameExceptionAs(Xeption expectedException) =>
            actualException => actualException.SameExceptionAs(expectedException);

        private static SqlException GetSqlException() =>
            (SqlException)RuntimeHelpers.GetUninitializedObject(typeof(SqlException));

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private static int GetRandomNegativeNumber() =>
            -1 * new IntRange(min: 2, max: 10).GetValue();

        private static DateTimeOffset GetRandomDateTimeOffset() =>
            new DateTimeRange(earliestDate: new DateTime()).GetValue();

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

        private static IQueryable<Provider> CreateRandomProviders()
        {
            return CreateProviderFiller(dateTimeOffset: GetRandomDateTimeOffset())
                .Create(count: GetRandomNumber())
                .AsQueryable();
        }

        private static Provider CreateRandomProvider() =>
            CreateProviderFiller(dateTimeOffset: GetRandomDateTimeOffset()).Create();

        private static Provider CreateRandomProvider(DateTimeOffset dateTimeOffset, string userId = "") =>
            CreateProviderFiller(dateTimeOffset, userId).Create();

        private static Provider CreateRandomModifyProvider(DateTimeOffset dateTimeOffset, string userId = "")
        {
            int randomDaysInPast = GetRandomNegativeNumber();
            Provider randomProvider = CreateRandomProvider(dateTimeOffset, userId);
            randomProvider.CreatedDate = randomProvider.CreatedDate.AddDays(randomDaysInPast);

            return randomProvider;
        }

        private static Filler<Provider> CreateProviderFiller(DateTimeOffset dateTimeOffset, string userId = "")
        {
            userId = string.IsNullOrWhiteSpace(userId) ? Guid.NewGuid().ToString() : userId;
            var filler = new Filler<Provider>();

            filler.Setup()
                .OnType<DateTimeOffset>().Use(dateTimeOffset)
                .OnProperty(provider => provider.Name).Use(GetRandomStringWithLengthOf(500))
                .OnProperty(provider => provider.CreatedBy).Use(userId)
                .OnProperty(provider => provider.UpdatedBy).Use(userId);

            return filler;
        }
    }
}
