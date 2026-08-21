// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using LondonFhirService.Clients.AuditAndMetrics.Clients.Metrics;
using LondonFhirService.Clients.AuditAndMetrics.Models.Metrics.Exceptions;
using LondonFhirService.Clients.AuditAndMetrics.Tests.Unit.Models.Metrics;
using LondonFhirService.Core.Abstractions.Models.Metrics;
using LondonFhirService.Clients.AuditAndMetrics.Services.Foundations.Metrics;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;

namespace LondonFhirService.Clients.AuditAndMetrics.Tests.Unit.Metrics
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

        private static IMetric CreateRandomMetric() =>
            CreateMetricFiller().Create();

        private static List<IMetric> CreateRandomMetrics() =>
            CreateMetricFiller().Create(count: GetRandomNumber())
                .Cast<IMetric>()
                    .ToList();

        private static IQueryable<IMetric> CreateRandomMetricsQueryable() =>
            CreateMetricFiller().Create(count: GetRandomNumber())
                .Cast<IMetric>()
                    .AsQueryable();

        private static Filler<TestMetric> CreateMetricFiller()
        {
            var filler = new Filler<TestMetric>();
            filler.Setup().OnType<DateTimeOffset>().Use(GetRandomDateTimeOffset());

            return filler;
        }
    }
}
