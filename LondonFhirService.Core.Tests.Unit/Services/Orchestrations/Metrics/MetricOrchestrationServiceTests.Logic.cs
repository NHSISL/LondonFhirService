// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Abstractions.Models.Metrics;
using LondonFhirService.Core.Models.Processings.Metrics;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Orchestrations.Metrics
{
    public partial class MetricOrchestrationServiceTests
    {
        [Fact]
        public async Task ShouldExportRequestMetricsToCsvAsync()
        {
            // given
            Guid randomCorrelationId = Guid.NewGuid();
            string randomUserId = GetRandomString();
            MetricStatus? randomStatus = MetricStatus.Failed;
            DateTimeOffset randomFromDate = DateTimeOffset.UtcNow.AddDays(-1);
            DateTimeOffset randomToDate = DateTimeOffset.UtcNow;
            IQueryable<MetricExport> randomMetricExports = CreateRandomMetricExports();
            string expectedCsv = GetRandomString();
            List<MetricExport> writtenMetricExports = null;
            Dictionary<string, int> writtenFieldMappings = null;

            this.metricProcessingServiceMock.Setup(service =>
                service.RetrieveRequestMetricExportsAsync(
                    randomCorrelationId,
                    randomUserId,
                    randomStatus,
                    randomFromDate,
                    randomToDate,
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(randomMetricExports);

            this.csvHelperBrokerMock.Setup(broker =>
                broker.MapObjectToCsvAsync(
                    It.IsAny<IAsyncEnumerable<MetricExport>>(),
                    It.IsAny<Stream>(),
                    true,
                    It.IsAny<Dictionary<string, int>>(),
                    false,
                    It.IsAny<CancellationToken>()))
                        .Returns(async (
                            IAsyncEnumerable<MetricExport> rows,
                            Stream outputStream,
                            bool addHeaderRecord,
                            Dictionary<string, int> fieldMappings,
                            bool? shouldAddTrailingComma,
                            CancellationToken cancellationToken) =>
                        {
                            writtenMetricExports = await rows.ToListAsync(cancellationToken);
                            writtenFieldMappings = fieldMappings;
                            await outputStream.WriteAsync(Encoding.UTF8.GetBytes(expectedCsv), cancellationToken);
                        });

            // when
            Stream actualCsvStream =
                await this.metricOrchestrationService.ExportRequestMetricsToCsvAsync(
                    randomCorrelationId,
                    randomUserId,
                    randomStatus,
                    randomFromDate,
                    randomToDate,
                    TestContext.Current.CancellationToken);

            // then
            // Rewound, so a caller can hand the stream straight back without seeking it first.
            actualCsvStream.Position.Should().Be(0);

            using var reader = new StreamReader(actualCsvStream);
            (await reader.ReadToEndAsync(TestContext.Current.CancellationToken)).Should().Be(expectedCsv);

            writtenMetricExports.Should().Equal(randomMetricExports);
            writtenFieldMappings.Should().Equal(ExpectedFieldMappings());

            this.metricProcessingServiceMock.Verify(service =>
                service.RetrieveRequestMetricExportsAsync(
                    randomCorrelationId,
                    randomUserId,
                    randomStatus,
                    randomFromDate,
                    randomToDate,
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.csvHelperBrokerMock.Verify(broker =>
                broker.MapObjectToCsvAsync(
                    It.IsAny<IAsyncEnumerable<MetricExport>>(),
                    It.IsAny<Stream>(),
                    true,
                    It.IsAny<Dictionary<string, int>>(),
                    false,
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.metricProcessingServiceMock.VerifyNoOtherCalls();
            this.csvHelperBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
