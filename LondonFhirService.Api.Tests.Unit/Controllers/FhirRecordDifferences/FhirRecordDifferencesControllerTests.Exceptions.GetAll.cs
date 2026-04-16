// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Linq;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.FhirRecordDifferences;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RESTFulSense.Clients.Extensions;
using RESTFulSense.Models;
using Xeptions;

namespace LondonFhirService.Api.Tests.Unit.Controllers.FhirRecordDifferences
{
    public partial class FhirRecordDifferencesControllerTests
    {
        [Theory]
        [MemberData(nameof(ServerExceptions))]
        public async Task ShouldReturnInternalServerErrorOnGetIfServerErrorOccurredAsync(
            Xeption serverException)
        {
            // given
            IQueryable<FhirRecordDifference> someFhirRecordDifferences =
                CreateRandomFhirRecordDifferences();

            InternalServerErrorObjectResult expectedInternalServerErrorObjectResult =
                InternalServerError(serverException);

            var expectedActionResult =
                new ActionResult<IQueryable<FhirRecordDifference>>(
                    expectedInternalServerErrorObjectResult);

            this.fhirRecordDifferenceServiceMock.Setup(service =>
                service.RetrieveAllFhirRecordDifferencesAsync())
                    .ThrowsAsync(serverException);

            // when
            ActionResult<IQueryable<FhirRecordDifference>> actualActionResult =
                await this.fhirRecordDifferencesController.Get();

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.fhirRecordDifferenceServiceMock.Verify(service =>
                service.RetrieveAllFhirRecordDifferencesAsync(),
                    Times.Once);

            this.fhirRecordDifferenceServiceMock.VerifyNoOtherCalls();
        }
    }
}
