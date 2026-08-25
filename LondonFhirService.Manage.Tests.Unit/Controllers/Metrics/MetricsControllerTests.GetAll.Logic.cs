// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Linq;
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
        public async Task ShouldReturnRecordsOnGetAsync()
        {
            // given
            IQueryable<Metric> randomMetrics = CreateRandomMetrics();
            IQueryable<Metric> storageMetrics = randomMetrics.DeepClone();
            IQueryable<Metric> expectedMetric = storageMetrics.DeepClone();

            var expectedObjectResult =
                new OkObjectResult(expectedMetric);

            var expectedActionResult =
                new ActionResult<IQueryable<Metric>>(expectedObjectResult);

            metricServiceMock
                .Setup(service => service.RetrieveAllMetricsAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(storageMetrics);

            // when
            ActionResult<IQueryable<Metric>> actualActionResult = await metricsController.Get();

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            metricServiceMock
               .Verify(service => service.RetrieveAllMetricsAsync(It.IsAny<CancellationToken>()),
                   Times.Once);

            metricServiceMock.VerifyNoOtherCalls();
        }
    }
}
