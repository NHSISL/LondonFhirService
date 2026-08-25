// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Clients.AuditAndMetrics.Brokers.DateTimes;
using LondonFhirService.Clients.AuditAndMetrics.Brokers.Loggings;
using LondonFhirService.Clients.AuditAndMetrics.Brokers.Metrics;
using LondonFhirService.Core.Abstractions.Brokers;
using LondonFhirService.Core.Abstractions.Models.Metrics;
using LondonFhirService.Clients.AuditAndMetrics.Models.Configurations;
using LondonFhirService.Clients.AuditAndMetrics.Services.Foundations.Metrics;
using LondonFhirService.Clients.AuditAndMetrics.Tests.Unit.Models.Metrics;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;

namespace LondonFhirService.Clients.AuditAndMetrics.Tests.Unit.Services.Foundations.Metrics
{
    public partial class MetricServiceTests
    {
        private readonly Mock<IAuditAndMetricStorageBroker> storageBrokerMock;
        private readonly Mock<IMetricBroker> metricBrokerMock;
        private readonly Mock<IDateTimeBroker> dateTimeBrokerMock;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly Mock<IAuditAndMetricsDispatcher> dispatcherMock;
        private readonly AuditAndMetricsConfigurations metricServiceConfigurations;
        private readonly IMetricService metricService;

        public MetricServiceTests()
        {
            this.storageBrokerMock = new Mock<IAuditAndMetricStorageBroker>();
            this.metricBrokerMock = new Mock<IMetricBroker>();
            this.dateTimeBrokerMock = new Mock<IDateTimeBroker>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();
            this.dispatcherMock = new Mock<IAuditAndMetricsDispatcher>();

            // Inline, so a dispatched write is observable by the time the call returns.
            this.dispatcherMock.Setup(dispatcher =>
                dispatcher.TryDispatch(It.IsAny<Func<CancellationToken, ValueTask>>()))
                    .Returns((Func<CancellationToken, ValueTask> work) =>
                    {
                        work(CancellationToken.None).AsTask().GetAwaiter().GetResult();

                        return true;
                    });

            // Held as a field rather than a mock so a test can flip a switch in place and
            // exercise the configured behaviour without building a second service.
            this.metricServiceConfigurations = new AuditAndMetricsConfigurations
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
                metricServiceConfigurations: this.metricServiceConfigurations,
                dispatcher: this.dispatcherMock.Object);
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

        /// <summary>
        /// Stands in for whatever the hosting application's storage broker failed with. This
        /// library carries no ORM and no database driver, so it can never see a SqlException or
        /// a DbUpdateException - the adapter implementing the storage port catches those and
        /// re-throws the port's own exceptions, which is what these tests drive it with.
        /// </summary>
        private static Exception GetStorageException() =>
            new Exception(GetRandomString());

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

        private static List<IMetric> CreateRandomMetrics(DateTimeOffset dateTimeOffset) =>
            CreateMetricFiller(dateTimeOffset).Create(count: GetRandomNumber())
                .Cast<IMetric>()
                    .ToList();

        private static IQueryable<IMetric> CreateRandomMetricsQueryable() =>
            CreateMetricFiller(GetRandomDateTimeOffset())
                .Create(count: GetRandomNumber())
                    .Cast<IMetric>()
                        .AsQueryable();

        private static IMetric CreateRandomMetric() =>
            CreateMetricFiller(GetRandomDateTimeOffset()).Create();

        private static IMetric CreateRandomMetric(DateTimeOffset dateTimeOffset) =>
            CreateMetricFiller(dateTimeOffset).Create();

        /// <summary>
        /// Every DateTimeOffset takes the same value so that Completed is never earlier than
        /// Started, and the string properties are pinned to their exact maximum lengths so a
        /// valid metric sits on the boundary rather than well inside it.
        /// </summary>
        private static Filler<TestMetric> CreateMetricFiller(DateTimeOffset dateTimeOffset)
        {
            var filler = new Filler<TestMetric>();

            filler.Setup()
                .OnType<DateTimeOffset>().Use(dateTimeOffset)
                .OnProperty(metric => metric.Type).Use(MetricType.Provider)
                .OnProperty(metric => metric.Status).Use(MetricStatus.Succeeded)
                .OnProperty(metric => metric.Method).Use(GetRandomStringWithLengthOf(255))
                .OnProperty(metric => metric.Name).Use(GetRandomStringWithLengthOf(255))
                .OnProperty(metric => metric.Target).Use(GetRandomStringWithLengthOf(255))
                .OnProperty(metric => metric.ErrorCode).Use(GetRandomStringWithLengthOf(100))
                .OnProperty(metric => metric.Consumer).Use(GetRandomStringWithLengthOf(255))
                .OnProperty(metric => metric.Description).Use(GetRandomStringWithLengthOf(1000))
                .OnProperty(metric => metric.DurationMs).Use(GetRandomDuration())
                .OnProperty(metric => metric.PayloadBytes).Use((long?)GetRandomNumber());

            return filler;
        }
    }
}
