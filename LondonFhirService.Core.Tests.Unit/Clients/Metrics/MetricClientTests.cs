// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using LondonFhirService.Core.Clients.Metrics;
using LondonFhirService.Core.Models.Clients.MetricClient.Exceptions;
using LondonFhirService.Core.Models.Foundations.Metrics;
using LondonFhirService.Core.Models.Foundations.Metrics.Exceptions;
using LondonFhirService.Core.Services.Foundations.Metrics;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Clients.Metrics
{
    public partial class MetricClientTests
    {
        private readonly Mock<IMetricService> metricServiceMock;
        private readonly IMetricClient metricClient;

        public MetricClientTests()
        {
            this.metricServiceMock = new Mock<IMetricService>();
            this.metricClient = new MetricClient(metricService: this.metricServiceMock.Object);
        }

        /// <summary>
        /// Each case is a service exception paired with the client exception it must surface as.
        /// The client keeps its own contract, so callers never have to catch service types.
        /// </summary>
        public static TheoryData<Xeption, Xeption> ServiceExceptionMappings()
        {
            var innerException = new NullMetricException(message: "Metric is null.");

            return new TheoryData<Xeption, Xeption>
            {
                {
                    new MetricValidationException("Service validation.", innerException),
                    new MetricClientValidationException(
                        "Metric client validation error occurred, fix errors and try again.",
                        innerException)
                },
                {
                    new MetricDependencyValidationException("Service dependency validation.", innerException),
                    new MetricClientValidationException(
                        "Metric client validation error occurred, fix errors and try again.",
                        innerException)
                },
                {
                    new MetricDependencyException("Service dependency.", innerException),
                    new MetricClientDependencyException(
                        "Metric client dependency error occurred, please contact support.",
                        innerException)
                },
                {
                    new MetricServiceException("Service.", innerException),
                    new MetricClientServiceException(
                        "Metric client service error occurred, fix errors and try again.",
                        innerException)
                }
            };
        }

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private static DateTimeOffset GetRandomDateTimeOffset() =>
            new DateTimeRange(earliestDate: DateTime.UtcNow.AddYears(-1)).GetValue();

        private static Metric CreateRandomMetric() =>
            CreateMetricFiller().Create();

        private static List<Metric> CreateRandomMetrics() =>
            CreateMetricFiller().Create(count: GetRandomNumber()).ToList();

        private static IQueryable<Metric> CreateRandomMetricsQueryable() =>
            CreateMetricFiller().Create(count: GetRandomNumber()).AsQueryable();

        private static Filler<Metric> CreateMetricFiller()
        {
            var filler = new Filler<Metric>();
            filler.Setup().OnType<DateTimeOffset>().Use(GetRandomDateTimeOffset());

            return filler;
        }
    }
}
