// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
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
        public async Task ShouldReturnBadRequestOnDeleteIfValidationErrorOccurredAsync(Xeption validationException)
        {
            // given
            Guid someId = Guid.NewGuid();

            BadRequestObjectResult expectedBadRequestObjectResult =
                BadRequest(validationException.InnerException);

            var expectedActionResult =
                new ActionResult<Metric>(expectedBadRequestObjectResult);

            this.metricServiceMock.Setup(service =>
                service.RemoveMetricByIdAsync(It.IsAny<Guid>(), default))
                    .ThrowsAsync(validationException);

            // when
            ActionResult<Metric> actualActionResult =
                await this.metricsController.DeleteMetricByIdAsync(someId);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.metricServiceMock.Verify(service =>
                service.RemoveMetricByIdAsync(It.IsAny<Guid>(), default),
                    Times.Once);

            this.metricServiceMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(ServerExceptions))]
        public async Task ShouldReturnInternalServerErrorOnDeleteIfServerErrorOccurredAsync(
            Xeption validationException)
        {
            // given
            Guid someId = Guid.NewGuid();

            InternalServerErrorObjectResult expectedInternalServerErrorObjectResult =
                InternalServerError(validationException);

            var expectedActionResult =
                new ActionResult<Metric>(expectedInternalServerErrorObjectResult);

            this.metricServiceMock.Setup(service =>
                service.RemoveMetricByIdAsync(It.IsAny<Guid>(), default))
                    .ThrowsAsync(validationException);

            // when
            ActionResult<Metric> actualActionResult =
                await this.metricsController.DeleteMetricByIdAsync(someId);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.metricServiceMock.Verify(service =>
                service.RemoveMetricByIdAsync(It.IsAny<Guid>(), default),
                    Times.Once);

            this.metricServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnNotFoundOnDeleteIfItemDoesNotExistAsync()
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
                service.RemoveMetricByIdAsync(It.IsAny<Guid>(), default))
                    .ThrowsAsync(metricServiceValidationException);

            // when
            ActionResult<Metric> actualActionResult =
                await this.metricsController.DeleteMetricByIdAsync(someId);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.metricServiceMock.Verify(service =>
                service.RemoveMetricByIdAsync(It.IsAny<Guid>(), default),
                    Times.Once);

            this.metricServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnLockedOnDeleteIfRecordIsLockedAsync()
        {
            // given
            Guid someId = Guid.NewGuid();
            var someInnerException = new Exception();
            string someMessage = GetRandomString();

            var lockedMetricServiceException =
                new LockedMetricServiceException(
                    message: someMessage,
                    innerException: someInnerException);

            var metricServiceDependencyValidationException =
                new MetricServiceDependencyValidationException(
                    message: someMessage,
                    innerException: lockedMetricServiceException);

            LockedObjectResult expectedConflictObjectResult =
                Locked(lockedMetricServiceException);

            var expectedActionResult =
                new ActionResult<Metric>(expectedConflictObjectResult);

            this.metricServiceMock.Setup(service =>
                service.RemoveMetricByIdAsync(It.IsAny<Guid>(), default))
                    .ThrowsAsync(metricServiceDependencyValidationException);

            // when
            ActionResult<Metric> actualActionResult =
                await this.metricsController.DeleteMetricByIdAsync(someId);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.metricServiceMock.Verify(service =>
                service.RemoveMetricByIdAsync(It.IsAny<Guid>(), default),
                    Times.Once);

            this.metricServiceMock.VerifyNoOtherCalls();
        }
    }
}
