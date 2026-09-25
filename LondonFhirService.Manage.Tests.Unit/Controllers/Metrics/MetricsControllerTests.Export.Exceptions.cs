// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Orchestrations.Metrics.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RESTFulSense.Models;
using Xeptions;

namespace LondonFhirService.Manage.Tests.Unit.Controllers.Metrics
{
    public partial class MetricsControllerTests
    {
        [Fact]
        public async Task ShouldReturnBadRequestOnGetMetricExportIfDependencyValidationErrorOccurredAsync()
        {
            // given
            var someInnerException = new Xeption(GetRandomString());

            var dependencyValidationException =
                new MetricOrchestrationDependencyValidationException(
                    message: GetRandomString(),
                    innerException: someInnerException);

            BadRequestObjectResult expectedBadRequestObjectResult = BadRequest(someInnerException);

            this.metricOrchestrationServiceMock.Setup(service =>
                service.ExportRequestMetricsToCsvAsync(
                    It.IsAny<Guid?>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTimeOffset?>(),
                    It.IsAny<DateTimeOffset?>(),
                    It.IsAny<CancellationToken>()))
                        .ThrowsAsync(dependencyValidationException);

            // when
            ActionResult actualActionResult =
                await this.metricsController.GetMetricExportAsync(
                    correlationId: null,
                    userId: null,
                    fromDate: null,
                    toDate: null);

            // then
            actualActionResult.Should().BeEquivalentTo(expectedBadRequestObjectResult);
            VerifyExportRequestedOnce();
        }

        [Theory]
        [MemberData(nameof(ExportServerExceptions))]
        public async Task ShouldReturnInternalServerErrorOnGetMetricExportIfServerErrorOccurredAsync(
            Xeption serverException)
        {
            // given
            InternalServerErrorObjectResult expectedInternalServerErrorObjectResult =
                InternalServerError(serverException);

            this.metricOrchestrationServiceMock.Setup(service =>
                service.ExportRequestMetricsToCsvAsync(
                    It.IsAny<Guid?>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTimeOffset?>(),
                    It.IsAny<DateTimeOffset?>(),
                    It.IsAny<CancellationToken>()))
                        .ThrowsAsync(serverException);

            // when
            ActionResult actualActionResult =
                await this.metricsController.GetMetricExportAsync(
                    correlationId: null,
                    userId: null,
                    fromDate: null,
                    toDate: null);

            // then
            actualActionResult.Should().BeEquivalentTo(expectedInternalServerErrorObjectResult);
            VerifyExportRequestedOnce();
        }

        private void VerifyExportRequestedOnce()
        {
            this.metricOrchestrationServiceMock.Verify(service =>
                service.ExportRequestMetricsToCsvAsync(
                    It.IsAny<Guid?>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTimeOffset?>(),
                    It.IsAny<DateTimeOffset?>(),
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.metricOrchestrationServiceMock.VerifyNoOtherCalls();
            this.metricServiceMock.VerifyNoOtherCalls();
        }
    }
}
