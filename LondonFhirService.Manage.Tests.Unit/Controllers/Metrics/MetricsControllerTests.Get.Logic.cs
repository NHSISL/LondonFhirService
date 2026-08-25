// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using Force.DeepCloner;
using LondonFhirService.Core.Models.Foundations.Metrics;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RESTFulSense.Clients.Extensions;

namespace LondonFhirService.Manage.Tests.Unit.Controllers.Metrics
{
    public partial class MetricsControllerTests
    {
        [Fact]
        public async Task ShouldReturnRecordOnGetByIdsAsync()
        {
            // given
            Metric randomMetric = CreateRandomMetric();
            Guid inputId = randomMetric.Id;
            Metric storageMetric = randomMetric;
            Metric expectedMetric = storageMetric.DeepClone();

            var expectedObjectResult =
                new OkObjectResult(expectedMetric);

            var expectedActionResult =
                new ActionResult<Metric>(expectedObjectResult);

            metricServiceMock
                .Setup(service => service.RetrieveMetricByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(storageMetric);

            // when
            ActionResult<Metric> actualActionResult = await metricsController.GetMetricByIdAsync(inputId);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            metricServiceMock
                .Verify(service => service.RetrieveMetricByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            metricServiceMock.VerifyNoOtherCalls();
        }
    }
}
