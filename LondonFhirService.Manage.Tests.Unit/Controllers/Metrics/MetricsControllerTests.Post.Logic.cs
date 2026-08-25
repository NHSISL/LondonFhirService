// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;
using Force.DeepCloner;
using LondonFhirService.Core.Models.Foundations.Metrics;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RESTFulSense.Clients.Extensions;
using RESTFulSense.Models;

namespace LondonFhirService.Manage.Tests.Unit.Controllers.Metrics
{
    public partial class MetricsControllerTests
    {
        [Fact]
        public async Task ShouldReturnCreatedOnPostAsync()
        {
            // given
            Metric randomMetric = CreateRandomMetric();
            Metric inputMetric = randomMetric;
            Metric addedMetric = inputMetric.DeepClone();
            Metric expectedMetric = addedMetric.DeepClone();

            var expectedObjectResult =
                new CreatedObjectResult(expectedMetric);

            var expectedActionResult =
                new ActionResult<Metric>(expectedObjectResult);

            metricServiceMock
                .Setup(service => service.AddMetricAsync(
                    inputMetric,
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(addedMetric);

            // when
            ActionResult<Metric> actualActionResult = await metricsController.PostMetricAsync(randomMetric);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            metricServiceMock
                .Verify(service => service.AddMetricAsync(
                    inputMetric,
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            metricServiceMock.VerifyNoOtherCalls();
        }
    }
}
