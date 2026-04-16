// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading.Tasks;
using Force.DeepCloner;
using LondonFhirService.Core.Models.Foundations.FhirRecordDifferences;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RESTFulSense.Clients.Extensions;

namespace LondonFhirService.Api.Tests.Unit.Controllers.FhirRecordDifferences
{
    public partial class FhirRecordDifferencesControllerTests
    {
        [Fact]
        public async Task ShouldReturnOkOnPutAsync()
        {
            // given
            FhirRecordDifference randomFhirRecordDifference = CreateRandomFhirRecordDifference();
            FhirRecordDifference inputFhirRecordDifference = randomFhirRecordDifference;
            FhirRecordDifference storageFhirRecordDifference = inputFhirRecordDifference.DeepClone();
            FhirRecordDifference expectedFhirRecordDifference = storageFhirRecordDifference.DeepClone();

            var expectedObjectResult =
                new OkObjectResult(expectedFhirRecordDifference);

            var expectedActionResult =
                new ActionResult<FhirRecordDifference>(expectedObjectResult);

            fhirRecordDifferenceServiceMock
                .Setup(service =>
                    service.ModifyFhirRecordDifferenceAsync(inputFhirRecordDifference))
                        .ReturnsAsync(storageFhirRecordDifference);

            // when
            ActionResult<FhirRecordDifference> actualActionResult =
                await fhirRecordDifferencesController.PutFhirRecordDifferenceAsync(
                    randomFhirRecordDifference);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            fhirRecordDifferenceServiceMock
                .Verify(service =>
                    service.ModifyFhirRecordDifferenceAsync(inputFhirRecordDifference),
                        Times.Once);

            fhirRecordDifferenceServiceMock.VerifyNoOtherCalls();
        }
    }
}
