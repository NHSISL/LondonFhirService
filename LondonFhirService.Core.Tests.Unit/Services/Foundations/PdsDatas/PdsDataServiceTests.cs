// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using LondonFhirService.Core.Brokers.DateTimes;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Brokers.Storages.Sql;
using LondonFhirService.Core.Models.Foundations.PdsDatas;
using LondonFhirService.Core.Services.Foundations.PdsDatas;
using Microsoft.Data.SqlClient;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.PdsDatas
{
    public partial class PdsDataServiceTests
    {
        private readonly Mock<IStorageBroker> storageBroker;
        private readonly Mock<IDateTimeBroker> dateTimeBroker;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly PdsDataService pdsDataService;

        public PdsDataServiceTests()
        {
            this.storageBroker = new Mock<IStorageBroker>();
            this.dateTimeBroker = new Mock<IDateTimeBroker>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.pdsDataService = new PdsDataService(
                storageBroker: this.storageBroker.Object,
                dateTimeBroker: this.dateTimeBroker.Object,
                loggingBroker: this.loggingBrokerMock.Object);
        }

        private static Expression<Func<Xeption, bool>> SameExceptionAs(Xeption expectedException) =>
            actualException => actualException.SameExceptionAs(expectedException);

        private static string GetRandomString() =>
            new MnemonicString(wordCount: GetRandomNumber()).GetValue();

        private static List<string> GetRandomStringsWithLengthOf(int length) =>
            Enumerable.Range(start: 0, count: GetRandomNumber())
                .Select(selector: _ => GetRandomStringWithLengthOf(length))
                .ToList();

        private static string GetRandomStringWithLengthOf(int length)
        {
            string result = new MnemonicString(wordCount: 1, wordMinLength: length, wordMaxLength: length).GetValue();

            return result.Length > length ? result.Substring(0, length) : result;
        }

        private SqlException CreateSqlException() =>
            (SqlException)RuntimeHelpers.GetUninitializedObject(type: typeof(SqlException));

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private static int GetRandomNegativeNumber() =>
            -1 * new IntRange(min: 2, max: 10).GetValue();

        private static DateTimeOffset GetRandomDateTimeOffset() =>
            new DateTimeRange(earliestDate: new DateTime()).GetValue();

        private static DateTimeOffset GetRandomFutureDateTimeOffset()
        {
            DateTime futureStartDate = DateTimeOffset.UtcNow.AddDays(1).Date;
            int randomDaysInFuture = GetRandomNumber();
            DateTime futureEndDate = futureStartDate.AddDays(randomDaysInFuture).Date;

            return new DateTimeRange(earliestDate: futureStartDate, latestDate: futureEndDate).GetValue();
        }

        private static PdsData CreateRandomModifyPdsData(DateTimeOffset dateTimeOffset)
        {
            int randomDaysInPast = GetRandomNegativeNumber();
            PdsData PandomPdsData = CreateRandomPdsData(dateTimeOffset);

            return PandomPdsData;
        }

        private static List<PdsData> CreateRandomPdsDatas()
        {
            return CreatePdsDataFiller(dateTimeOffset: GetRandomDateTimeOffset())
                .Create(count: GetRandomNumber())
                    .ToList();
        }

        private static PdsData CreateRandomPdsData() =>
            CreatePdsDataFiller(dateTimeOffset: GetRandomDateTimeOffset()).Create();

        private static PdsData CreateRandomPdsData(DateTimeOffset dateTimeOffset) =>
            CreatePdsDataFiller(dateTimeOffset).Create();

        private static Filler<PdsData> CreatePdsDataFiller(DateTimeOffset dateTimeOffset)
        {
            string user = Guid.NewGuid().ToString();
            var filler = new Filler<PdsData>();

            filler.Setup()
                .OnType<DateTimeOffset>().Use(dateTimeOffset)
                .OnType<DateTimeOffset?>().Use((DateTimeOffset?)default)
                .OnProperty(pdsData => pdsData.NhsNumber).Use(GetRandomStringWithLengthOf(10))
                .OnProperty(pdsData => pdsData.OrgCode).Use(GetRandomStringWithLengthOf(15));

            return filler;
        }
    }
}