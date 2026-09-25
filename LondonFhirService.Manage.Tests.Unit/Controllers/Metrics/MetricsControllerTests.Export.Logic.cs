// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Abstractions.Models.Metrics;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace LondonFhirService.Manage.Tests.Unit.Controllers.Metrics
{
    public partial class MetricsControllerTests
    {
        [Fact]
        public async Task ShouldReturnCsvFileOnGetMetricExportAsync()
        {
            // given
            Guid randomCorrelationId = Guid.NewGuid();
            string randomUserId = GetRandomString();
            MetricStatus? randomStatus = MetricStatus.Failed;
            DateTimeOffset randomFromDate = DateTimeOffset.UtcNow.AddDays(-1);
            DateTimeOffset randomToDate = DateTimeOffset.UtcNow;
            Stream csvStream = new MemoryStream(Encoding.UTF8.GetBytes(GetRandomString()));

            this.metricOrchestrationServiceMock.Setup(service =>
                service.ExportRequestMetricsToCsvAsync(
                    randomCorrelationId,
                    randomUserId,
                    randomStatus,
                    randomFromDate,
                    randomToDate,
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(csvStream);

            // when
            ActionResult actualActionResult =
                await this.metricsController.GetMetricExportAsync(
                    randomCorrelationId,
                    randomUserId,
                    randomStatus,
                    randomFromDate,
                    randomToDate);

            // then
            FileStreamResult fileStreamResult = actualActionResult.Should()
                .BeOfType<FileStreamResult>().Subject;

            fileStreamResult.FileStream.Should().BeSameAs(csvStream);
            fileStreamResult.ContentType.Should().Be("text/csv");
            fileStreamResult.FileDownloadName.Should().Be("metrics.csv");

            this.metricOrchestrationServiceMock.Verify(service =>
                service.ExportRequestMetricsToCsvAsync(
                    randomCorrelationId,
                    randomUserId,
                    randomStatus,
                    randomFromDate,
                    randomToDate,
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.metricOrchestrationServiceMock.VerifyNoOtherCalls();
            this.metricServiceMock.VerifyNoOtherCalls();
        }
    }
}
