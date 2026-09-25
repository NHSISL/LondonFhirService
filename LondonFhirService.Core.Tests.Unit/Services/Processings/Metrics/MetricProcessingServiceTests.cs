// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq.Expressions;
using LondonFhirService.Core.Abstractions.Models.Metrics;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Models.Foundations.Metrics;
using LondonFhirService.Core.Models.Foundations.Metrics.Exceptions;
using LondonFhirService.Core.Services.Foundations.Metrics;
using LondonFhirService.Core.Services.Processings.Metrics;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Processings.Metrics
{
    public partial class MetricProcessingServiceTests
    {
        private readonly Mock<IMetricService> metricServiceMock;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly IMetricProcessingService metricProcessingService;

        public MetricProcessingServiceTests()
        {
            this.metricServiceMock = new Mock<IMetricService>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.metricProcessingService = new MetricProcessingService(
                metricService: this.metricServiceMock.Object,
                loggingBroker: this.loggingBrokerMock.Object);
        }

        public static TheoryData<Xeption> DependencyValidationExceptions()
        {
            string randomMessage = GetRandomString();
            var innerException = new Xeption(randomMessage);

            return new TheoryData<Xeption>
            {
                new MetricServiceValidationException(randomMessage, innerException),
                new MetricServiceDependencyValidationException(randomMessage, innerException)
            };
        }

        public static TheoryData<Xeption> DependencyExceptions()
        {
            string randomMessage = GetRandomString();
            var innerException = new Xeption(randomMessage);

            return new TheoryData<Xeption>
            {
                new MetricServiceDependencyException(randomMessage, innerException),
                new MetricServiceException(randomMessage, innerException)
            };
        }

        private static string GetRandomString() =>
            new MnemonicString().GetValue();

        private static DateTimeOffset GetRandomDateTimeOffset() =>
            new DateTimeRange(earliestDate: DateTime.UtcNow.AddYears(-1)).GetValue();

        private static Metric CreateRandomSpan(
            Guid correlationId,
            MetricType type,
            double durationMs,
            DateTimeOffset started)
        {
            var filler = new Filler<Metric>();

            filler.Setup()
                .OnType<DateTimeOffset>().Use(started)
                .OnType<DateTimeOffset?>().Use(started)
                .OnProperty(metric => metric.CorrelationId).Use(correlationId)
                .OnProperty(metric => metric.Type).Use(type)
                .OnProperty(metric => metric.DurationMs).Use(durationMs);

            return filler.Create();
        }

        private static Expression<Func<Xeption, bool>> SameExceptionAs(Xeption expectedException) =>
            actualException => actualException.SameExceptionAs(expectedException);
    }
}
