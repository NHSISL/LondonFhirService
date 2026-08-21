// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Core.Brokers.DateTimes;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Brokers.Metrics;
using LondonFhirService.Core.Brokers.Storages.Sql;
using LondonFhirService.Core.Models.Foundations.Metrics;
using LondonFhirService.Core.Services.Foundations.Metrics;
using Microsoft.Data.SqlClient;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Metrics
{
    public partial class MetricServiceTests
    {
        private readonly Mock<IStorageBroker> storageBrokerMock;
        private readonly Mock<IMetricBroker> metricBrokerMock;
        private readonly Mock<IDateTimeBroker> dateTimeBrokerMock;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly MetricServiceConfigurations metricServiceConfigurations;
        private readonly IMetricService metricService;

        public MetricServiceTests()
        {
            this.storageBrokerMock = new Mock<IStorageBroker>();
            this.metricBrokerMock = new Mock<IMetricBroker>();
            this.dateTimeBrokerMock = new Mock<IDateTimeBroker>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            // Held as a field rather than a mock so a test can flip a switch in place and
            // exercise the configured behaviour without building a second service.
            this.metricServiceConfigurations = new MetricServiceConfigurations
            {
                IsEnabled = true,
                IsPurgingAllowed = true,
                RetentionPeriodInDays = GetRandomNumber(),
                PurgeBatchSize = GetRandomNumber()
            };

            this.metricService = new MetricService(
                storageBroker: this.storageBrokerMock.Object,
                metricBroker: this.metricBrokerMock.Object,
                dateTimeBroker: this.dateTimeBrokerMock.Object,
                loggingBroker: this.loggingBrokerMock.Object,
                metricServiceConfigurations: this.metricServiceConfigurations);
        }

        private void VerifyNoOtherCallsOnAllBrokers()
        {
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.metricBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        private static Expression<Func<Xeption, bool>> SameExceptionAs(Xeption expectedException) =>
            actualException => actualException.SameExceptionAs(expectedException);

        public static TheoryData<Exception> TimeoutExceptions()
        {
            string randomMessage = GetRandomString();

            return new TheoryData<Exception>
            {
                new TimeoutException(randomMessage),

                new TaskCanceledException(
                    message: randomMessage,
                    innerException: new TimeoutException(randomMessage)),

                new OperationCanceledException(
                    message: randomMessage,
                    innerException: new TimeoutException(randomMessage))
            };
        }

        public static TheoryData<Exception> CancellationExceptions()
        {
            string randomMessage = GetRandomString();
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            return new TheoryData<Exception>
            {
                new OperationCanceledException(randomMessage),
                new OperationCanceledException(cancellationTokenSource.Token),
                new TaskCanceledException(randomMessage)
            };
        }

        private static SqlException GetSqlException() =>
            (SqlException)RuntimeHelpers.GetUninitializedObject(typeof(SqlException));

        private static string GetRandomString() =>
            new MnemonicString(wordCount: GetRandomNumber()).GetValue();

        private static string GetRandomStringWithLengthOf(int length)
        {
            string result = new MnemonicString(wordCount: 1, wordMinLength: length, wordMaxLength: length).GetValue();

            return result.Length > length ? result.Substring(0, length) : result;
        }

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private static int GetRandomNegativeNumber() =>
            -1 * new IntRange(min: 2, max: 10).GetValue();

        private static double GetRandomDuration() =>
            new IntRange(min: 1, max: 5000).GetValue();

        private static DateTimeOffset GetRandomDateTimeOffset() =>
            new DateTimeRange(earliestDate: new DateTime()).GetValue();

        /// <summary>
        /// Bounded to the last year so that the retention tests can subtract days from it without
        /// running off the bottom of DateTime, which the unbounded generator above can reach.
        /// </summary>
        private static DateTimeOffset GetRandomRecentDateTimeOffset() =>
            new DateTimeRange(earliestDate: DateTime.UtcNow.AddYears(-1)).GetValue();

        private static List<Metric> CreateRandomMetrics(DateTimeOffset dateTimeOffset) =>
            CreateMetricFiller(dateTimeOffset).Create(count: GetRandomNumber()).ToList();

        private static IQueryable<Metric> CreateRandomMetricsQueryable() =>
            CreateMetricFiller(GetRandomDateTimeOffset())
                .Create(count: GetRandomNumber())
                    .AsQueryable();

        private static Metric CreateRandomMetric() =>
            CreateMetricFiller(GetRandomDateTimeOffset()).Create();

        private static Metric CreateRandomMetric(DateTimeOffset dateTimeOffset) =>
            CreateMetricFiller(dateTimeOffset).Create();

        /// <summary>
        /// Every DateTimeOffset takes the same value so that Completed is never earlier than
        /// Started, and the string properties are pinned to their exact maximum lengths so a
        /// valid metric sits on the boundary rather than well inside it.
        /// </summary>
        private static Filler<Metric> CreateMetricFiller(DateTimeOffset dateTimeOffset)
        {
            var filler = new Filler<Metric>();

            filler.Setup()
                .OnType<DateTimeOffset>().Use(dateTimeOffset)
                .OnProperty(metric => metric.Type).Use(MetricType.Provider)
                .OnProperty(metric => metric.Status).Use(MetricStatus.Succeeded)
                .OnProperty(metric => metric.Method).Use(GetRandomStringWithLengthOf(255))
                .OnProperty(metric => metric.Name).Use(GetRandomStringWithLengthOf(255))
                .OnProperty(metric => metric.Target).Use(GetRandomStringWithLengthOf(255))
                .OnProperty(metric => metric.ErrorCode).Use(GetRandomStringWithLengthOf(100))
                .OnProperty(metric => metric.Consumer).Use(GetRandomStringWithLengthOf(255))
                .OnProperty(metric => metric.DurationMs).Use(GetRandomDuration())
                .OnProperty(metric => metric.PayloadBytes).Use((long?)GetRandomNumber());

            return filler;
        }
    }
}
