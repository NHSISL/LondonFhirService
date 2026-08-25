// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.Metrics;
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
        [MemberData(nameof(ServerExceptions))]
        public async Task ShouldReturnInternalServerErrorOnGetIfServerErrorOccurredAsync(
            Xeption serverException)
        {
            // given
            InternalServerErrorObjectResult expectedInternalServerErrorObjectResult =
                InternalServerError(serverException);

            var expectedActionResult =
                new ActionResult<IQueryable<Metric>>(expectedInternalServerErrorObjectResult);

            this.metricServiceMock.Setup(service =>
                service.RetrieveAllMetricsAsync(It.IsAny<CancellationToken>()))
                    .ThrowsAsync(serverException);

            // when
            ActionResult<IQueryable<Metric>> actualActionResult =
                await this.metricsController.Get();

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.metricServiceMock.Verify(service =>
                service.RetrieveAllMetricsAsync(It.IsAny<CancellationToken>()),
                    Times.Once);

            this.metricServiceMock.VerifyNoOtherCalls();
        }
    }
}
