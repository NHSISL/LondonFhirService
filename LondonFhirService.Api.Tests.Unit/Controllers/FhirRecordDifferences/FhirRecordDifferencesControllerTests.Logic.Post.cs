// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading.Tasks;
using Force.DeepCloner;
using LondonFhirService.Core.Models.Foundations.FhirRecordDifferences;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RESTFulSense.Clients.Extensions;
using RESTFulSense.Models;

namespace LondonFhirService.Api.Tests.Unit.Controllers.FhirRecordDifferences
{
    public partial class FhirRecordDifferencesControllerTests
    {
        [Fact]
        public async Task ShouldReturnCreatedOnPostAsync()
        {
            // given
            FhirRecordDifference randomFhirRecordDifference = CreateRandomFhirRecordDifference();
            FhirRecordDifference inputFhirRecordDifference = randomFhirRecordDifference;
            FhirRecordDifference addedFhirRecordDifference = inputFhirRecordDifference.DeepClone();
            FhirRecordDifference expectedFhirRecordDifference = addedFhirRecordDifference.DeepClone();

            var expectedObjectResult =
                new CreatedObjectResult(expectedFhirRecordDifference);

            var expectedActionResult =
                new ActionResult<FhirRecordDifference>(expectedObjectResult);

            fhirRecordDifferenceServiceMock
                .Setup(service =>
                    service.AddFhirRecordDifferenceAsync(inputFhirRecordDifference))
                        .ReturnsAsync(addedFhirRecordDifference);

            // when
            ActionResult<FhirRecordDifference> actualActionResult =
                await fhirRecordDifferencesController.PostFhirRecordDifferenceAsync(
                    randomFhirRecordDifference);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            fhirRecordDifferenceServiceMock
                .Verify(service =>
                    service.AddFhirRecordDifferenceAsync(inputFhirRecordDifference),
                        Times.Once);

            fhirRecordDifferenceServiceMock.VerifyNoOtherCalls();
        }
    }
}
