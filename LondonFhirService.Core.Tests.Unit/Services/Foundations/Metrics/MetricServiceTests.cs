// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using LondonFhirService.Core.Brokers.AuditAndMetrics;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Models.Foundations.Metrics;
using LondonFhirService.Core.Models.Foundations.Metrics.Exceptions;
using LondonFhirService.Core.Services.Foundations.Metrics;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;
using ClientExceptions = LondonFhirService.Clients.AuditAndMetrics.Models.Metrics.Exceptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Metrics
{
    /// <summary>
    /// The metric counterpart to AuditServiceTests. This service validates and stamps nothing -
    /// both live in the audit and metrics library, behind the broker. What is left to test is
    /// that it forwards faithfully and that it localises the client's exceptions into this
    /// application's own, so callers keep depending on Core's contract.
    /// </summary>
    public partial class MetricServiceTests
    {
        private readonly Mock<IAuditAndMetricBroker> auditAndMetricBrokerMock;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly IMetricService metricService;

        public MetricServiceTests()
        {
            this.auditAndMetricBrokerMock = new Mock<IAuditAndMetricBroker>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.metricService = new MetricService(
                auditAndMetricBroker: this.auditAndMetricBrokerMock.Object,
                loggingBroker: this.loggingBrokerMock.Object);
        }

        /// <summary>
        /// Each case is a client exception paired with the service exception it must surface as.
        ///
        /// The metric client collapses plain validation and dependency validation into one
        /// MetricClientValidationException, so which category comes out is decided by the inner
        /// exception rather than by the type caught. Both directions are covered.
        /// </summary>
        public static TheoryData<Xeption, Xeption> ClientExceptionMappings()
        {
            var innerException = new Xeption(message: "Inner.");

            return new TheoryData<Xeption, Xeption>
            {
                {
                    new ClientExceptions.MetricClientValidationException("Client validation.", innerException),
                    new MetricServiceValidationException(
                        "Metric validation errors occurred, please try again.",
                        innerException)
                },
                {
                    new ClientExceptions.MetricClientDependencyException("Client dependency.", innerException),
                    new MetricServiceDependencyException(
                        "Metric dependency error occurred, please contact support.",
                        innerException)
                },
                {
                    new ClientExceptions.MetricClientServiceException("Client service.", innerException),
                    new MetricServiceException(
                        "Metric service error occurred, please contact support.",
                        new FailedMetricServiceException(
                            "Failed metric service error occurred, please contact support.",
                            new ClientExceptions.MetricClientServiceException("Client service.", innerException)))
                }
            };
        }

        private void VerifyNoOtherCallsOnAllBrokers()
        {
            this.auditAndMetricBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        private static Expression<Func<Xeption, bool>> SameExceptionAs(Xeption expectedException) =>
            actualException => actualException.SameExceptionAs(expectedException);

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private static DateTimeOffset GetRandomDateTimeOffset() =>
            new DateTimeRange(earliestDate: DateTime.UtcNow.AddYears(-1)).GetValue();

        private static Metric CreateRandomMetric() =>
            CreateMetricFiller().Create();

        private static List<Metric> CreateRandomMetrics() =>
            CreateMetricFiller().Create(count: GetRandomNumber()).ToList();

        private static Filler<Metric> CreateMetricFiller()
        {
            var filler = new Filler<Metric>();
            filler.Setup().OnType<DateTimeOffset>().Use(GetRandomDateTimeOffset());

            return filler;
        }
    }
}
