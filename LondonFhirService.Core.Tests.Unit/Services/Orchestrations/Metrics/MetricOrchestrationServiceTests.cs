// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using LondonFhirService.Core.Brokers.CsvHelpers;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Models.Processings.Metrics;
using LondonFhirService.Core.Models.Processings.Metrics.Exceptions;
using LondonFhirService.Core.Services.Orchestrations.Metrics;
using LondonFhirService.Core.Services.Processings.Metrics;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Orchestrations.Metrics
{
    public partial class MetricOrchestrationServiceTests
    {
        private readonly Mock<IMetricProcessingService> metricProcessingServiceMock;
        private readonly Mock<ICsvHelperBroker> csvHelperBrokerMock;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly IMetricOrchestrationService metricOrchestrationService;

        public MetricOrchestrationServiceTests()
        {
            this.metricProcessingServiceMock = new Mock<IMetricProcessingService>();
            this.csvHelperBrokerMock = new Mock<ICsvHelperBroker>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.metricOrchestrationService = new MetricOrchestrationService(
                metricProcessingService: this.metricProcessingServiceMock.Object,
                csvHelperBroker: this.csvHelperBrokerMock.Object,
                loggingBroker: this.loggingBrokerMock.Object);
        }

        public static TheoryData<Xeption> DependencyValidationExceptions()
        {
            string randomMessage = GetRandomString();
            var innerException = new Xeption(randomMessage);

            return new TheoryData<Xeption>
            {
                new MetricProcessingValidationException(randomMessage, innerException),
                new MetricProcessingDependencyValidationException(randomMessage, innerException)
            };
        }

        public static TheoryData<Xeption> DependencyExceptions()
        {
            string randomMessage = GetRandomString();
            var innerException = new Xeption(randomMessage);

            return new TheoryData<Xeption>
            {
                new MetricProcessingDependencyException(randomMessage, innerException),
                new MetricProcessingServiceException(randomMessage, innerException)
            };
        }

        private static string GetRandomString() =>
            new MnemonicString().GetValue();

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private static IQueryable<MetricExport> CreateRandomMetricExports()
        {
            var filler = new Filler<MetricExport>();
            filler.Setup().OnType<DateTimeOffset>().Use(DateTimeOffset.UtcNow);

            return filler.Create(count: GetRandomNumber()).AsQueryable();
        }

        private static Expression<Func<Xeption, bool>> SameExceptionAs(Xeption expectedException) =>
            actualException => actualException.SameExceptionAs(expectedException);

        private static Dictionary<string, int> ExpectedFieldMappings() => new()
        {
            { "StartedUtc", 0 },
            { "CorrelationId", 1 },
            { "Method", 2 },
            { "Name", 3 },
            { "Status", 4 },
            { "ErrorCode", 5 },
            { "DurationMs", 6 },
            { "ProviderRequestsMs", 7 },
            { "ProxyOverheadMs", 8 },
            { "Consumer", 9 },
            { "UserId", 10 }
        };
    }
}
