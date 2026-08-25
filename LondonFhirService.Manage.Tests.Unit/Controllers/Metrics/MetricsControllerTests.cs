// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using LondonFhirService.Core.Abstractions.Models.Metrics;
using LondonFhirService.Core.Models.Foundations.Metrics;
using LondonFhirService.Core.Models.Foundations.Metrics.Exceptions;
using LondonFhirService.Core.Services.Foundations.Metrics;
using LondonFhirService.Manage.Controllers.Metrics;
using Moq;
using RESTFulSense.Controllers;
using Tynamix.ObjectFiller;
using Xeptions;

namespace LondonFhirService.Manage.Tests.Unit.Controllers.Metrics
{
    public partial class MetricsControllerTests : RESTFulController
    {
        private readonly Mock<IMetricService> metricServiceMock;
        private readonly MetricsController metricsController;

        public MetricsControllerTests()
        {
            metricServiceMock = new Mock<IMetricService>();
            metricsController = new MetricsController(metricServiceMock.Object);
        }

        public static TheoryData<Xeption> ValidationExceptions()
        {
            var someInnerException = new Xeption();
            string someMessage = GetRandomString();

            return new TheoryData<Xeption>
            {
                new MetricServiceValidationException(
                    message: someMessage,
                    innerException: someInnerException),

                new MetricServiceDependencyValidationException(
                    message: someMessage,
                    innerException: someInnerException)
            };
        }

        public static TheoryData<Xeption> ServerExceptions()
        {
            var someInnerException = new Xeption();
            string someMessage = GetRandomString();

            return new TheoryData<Xeption>
            {
                new MetricServiceDependencyException(
                    message: someMessage,
                    innerException: someInnerException),

                new MetricServiceException(
                    message: someMessage,
                    innerException: someInnerException)
            };
        }

        private static string GetRandomString() =>
            new MnemonicString(wordCount: GetRandomNumber()).GetValue();

        private static string GetRandomStringWithLengthOf(int length)
        {
            string result = new MnemonicString(wordCount: 1, wordMinLength: length, wordMaxLength: length).GetValue();

            return result.Length > length ? result.Substring(0, length) : result;
        }

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private static Metric CreateRandomMetric() =>
            CreateMetricFiller().Create();

        private static IQueryable<Metric> CreateRandomMetrics()
        {
            return CreateMetricFiller()
                .Create(count: GetRandomNumber())
                    .AsQueryable();
        }

        /// <summary>
        /// A metric carries no CreatedBy or UpdatedBy - a span is not authored by anyone, so
        /// unlike the audit filler there is no user to stamp. The bounded columns are filled to
        /// their configured lengths so a round trip cannot fail on truncation.
        /// </summary>
        private static Filler<Metric> CreateMetricFiller()
        {
            DateTimeOffset dateTimeOffset = DateTimeOffset.UtcNow;
            var filler = new Filler<Metric>();

            filler.Setup()
                .OnType<DateTimeOffset>().Use(dateTimeOffset)
                .OnType<DateTimeOffset?>().Use(dateTimeOffset)
                .OnProperty(metric => metric.Method).Use(GetRandomStringWithLengthOf(255))
                .OnProperty(metric => metric.Name).Use(GetRandomStringWithLengthOf(255))
                .OnProperty(metric => metric.Target).Use(GetRandomStringWithLengthOf(255))
                .OnProperty(metric => metric.Consumer).Use(GetRandomStringWithLengthOf(255))
                .OnProperty(metric => metric.ErrorCode).Use(GetRandomStringWithLengthOf(100))
                .OnProperty(metric => metric.Description).Use(GetRandomStringWithLengthOf(1000))
                .OnProperty(metric => metric.Type).Use(MetricType.Provider)
                .OnProperty(metric => metric.Status).Use(MetricStatus.Succeeded);

            return filler;
        }
    }
}
