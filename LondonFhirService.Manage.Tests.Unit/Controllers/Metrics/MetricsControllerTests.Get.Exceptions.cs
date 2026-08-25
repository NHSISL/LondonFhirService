// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.Metrics;
using LondonFhirService.Core.Models.Foundations.Metrics.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RESTFulSense.Clients.Extensions;
using RESTFulSense.Models;
using Xeptions;

namespace LondonFhirService.Manage.Tests.Unit.Controllers.Metrics
{
    public partial class MetricsControllerTests
    {
        [Theory]
        [MemberData(nameof(ValidationExceptions))]
        public async Task ShouldReturnBadRequestOnGetByIdIfValidationErrorOccurredAsync(Xeption validationException)
        {
            // given
            Guid someId = Guid.NewGuid();

            BadRequestObjectResult expectedBadRequestObjectResult =
                BadRequest(validationException.InnerException);

            var expectedActionResult =
                new ActionResult<Metric>(expectedBadRequestObjectResult);

            this.metricServiceMock.Setup(service =>
                service.RetrieveMetricByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                        .ThrowsAsync(validationException);

            // when
            ActionResult<Metric> actualActionResult =
                await this.metricsController.GetMetricByIdAsync(someId);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.metricServiceMock.Verify(service =>
                service.RetrieveMetricByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.metricServiceMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(ServerExceptions))]
        public async Task ShouldReturnInternalServerErrorOnGetByIdIfServerErrorOccurredAsync(
            Xeption validationException)
        {
            // given
            Guid someId = Guid.NewGuid();

            InternalServerErrorObjectResult expectedInternalServerErrorObjectResult =
                InternalServerError(validationException);

            var expectedActionResult =
                new ActionResult<Metric>(expectedInternalServerErrorObjectResult);

            this.metricServiceMock.Setup(service =>
                service.RetrieveMetricByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                        .ThrowsAsync(validationException);

            // when
            ActionResult<Metric> actualActionResult =
                await this.metricsController.GetMetricByIdAsync(someId);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.metricServiceMock.Verify(service =>
                service.RetrieveMetricByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.metricServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnNotFoundOnGetByIdIfItemDoesNotExistAsync()
        {
            // given
            Guid someId = Guid.NewGuid();
            string someMessage = GetRandomString();

            var notFoundMetricServiceException =
                new NotFoundMetricServiceException(
                    message: someMessage);

            var metricServiceValidationException =
                new MetricServiceValidationException(
                    message: someMessage,
                    innerException: notFoundMetricServiceException);

            NotFoundObjectResult expectedNotFoundObjectResult =
                NotFound(notFoundMetricServiceException);

            var expectedActionResult =
                new ActionResult<Metric>(expectedNotFoundObjectResult);

            this.metricServiceMock.Setup(service =>
                service.RetrieveMetricByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                        .ThrowsAsync(metricServiceValidationException);

            // when
            ActionResult<Metric> actualActionResult =
                await this.metricsController.GetMetricByIdAsync(someId);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.metricServiceMock.Verify(service =>
                service.RetrieveMetricByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.metricServiceMock.VerifyNoOtherCalls();
        }
    }
}
