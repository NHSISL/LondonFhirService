// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
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
        public async Task ShouldRemoveRecordOnDeleteByIdAsync()
        {
            // given
            FhirRecordDifference randomFhirRecordDifference = CreateRandomFhirRecordDifference();
            Guid inputId = randomFhirRecordDifference.Id;
            FhirRecordDifference storageFhirRecordDifference = randomFhirRecordDifference;
            FhirRecordDifference expectedFhirRecordDifference = storageFhirRecordDifference.DeepClone();

            var expectedObjectResult =
                new OkObjectResult(expectedFhirRecordDifference);

            var expectedActionResult =
                new ActionResult<FhirRecordDifference>(expectedObjectResult);

            fhirRecordDifferenceServiceMock
                .Setup(service =>
                    service.RemoveFhirRecordDifferenceByIdAsync(inputId))
                        .ReturnsAsync(storageFhirRecordDifference);

            // when
            ActionResult<FhirRecordDifference> actualActionResult =
                await fhirRecordDifferencesController.DeleteFhirRecordDifferenceByIdAsync(inputId);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            fhirRecordDifferenceServiceMock
                .Verify(service =>
                    service.RemoveFhirRecordDifferenceByIdAsync(inputId),
                        Times.Once);

            fhirRecordDifferenceServiceMock.VerifyNoOtherCalls();
        }
    }
}
