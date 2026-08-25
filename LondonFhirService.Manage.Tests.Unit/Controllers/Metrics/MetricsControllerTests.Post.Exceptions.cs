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
        public async Task ShouldReturnBadRequestOnPostIfValidationErrorOccurredAsync(Xeption validationException)
        {
            // given
            Metric someMetric = CreateRandomMetric();

            BadRequestObjectResult expectedBadRequestObjectResult =
                BadRequest(validationException.InnerException);

            var expectedActionResult =
                new ActionResult<Metric>(expectedBadRequestObjectResult);

            this.metricServiceMock.Setup(service =>
                service.AddMetricAsync(It.IsAny<Metric>(), default))
                    .ThrowsAsync(validationException);

            // when
            ActionResult<Metric> actualActionResult =
                await this.metricsController.PostMetricAsync(someMetric);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.metricServiceMock.Verify(service =>
                service.AddMetricAsync(It.IsAny<Metric>(), default),
                    Times.Once);

            this.metricServiceMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(ServerExceptions))]
        public async Task ShouldReturnInternalServerErrorOnPostIfServerErrorOccurredAsync(
            Xeption validationException)
        {
            // given
            Metric someMetric = CreateRandomMetric();

            InternalServerErrorObjectResult expectedInternalServerErrorObjectResult =
                InternalServerError(validationException);

            var expectedActionResult =
                new ActionResult<Metric>(expectedInternalServerErrorObjectResult);

            this.metricServiceMock.Setup(service =>
                service.AddMetricAsync(It.IsAny<Metric>(), default))
                    .ThrowsAsync(validationException);

            // when
            ActionResult<Metric> actualActionResult =
                await this.metricsController.PostMetricAsync(someMetric);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.metricServiceMock.Verify(service =>
                service.AddMetricAsync(It.IsAny<Metric>(), default),
                    Times.Once);

            this.metricServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnConflictOnPostIfAlreadyExistsMetricErrorOccurredAsync()
        {
            // given
            Metric someMetric = CreateRandomMetric();
            var someInnerException = new Exception();
            string someMessage = GetRandomString();

            var alreadyExistsMetricServiceException =
                new AlreadyExistsMetricServiceException(
                    message: someMessage,
                    innerException: someInnerException,
                    data: someInnerException.Data);

            var metricServiceDependencyValidationException =
                new MetricServiceDependencyValidationException(
                    message: someMessage,
                    innerException: alreadyExistsMetricServiceException);

            ConflictObjectResult expectedConflictObjectResult =
                Conflict(alreadyExistsMetricServiceException);

            var expectedActionResult =
                new ActionResult<Metric>(expectedConflictObjectResult);

            this.metricServiceMock.Setup(service =>
                service.AddMetricAsync(It.IsAny<Metric>(), default))
                    .ThrowsAsync(metricServiceDependencyValidationException);

            // when
            ActionResult<Metric> actualActionResult =
                await this.metricsController.PostMetricAsync(someMetric);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.metricServiceMock.Verify(service =>
                service.AddMetricAsync(It.IsAny<Metric>(), default),
                    Times.Once);

            this.metricServiceMock.VerifyNoOtherCalls();
        }
    }
}
