// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using Force.DeepCloner;
using LondonFhirService.Manage.Models.Foundations.FhirRecordDifferences;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RESTFulSense.Clients.Extensions;

namespace LondonFhirService.Manage.Tests.Unit.Controllers.FhirRecordDifferences
{
    public partial class FhirRecordDifferencesControllerTests
    {
        [Fact]
        public async Task ShouldReturnRecordOnGetByIdsAsync()
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
                .Setup(service => service.RetrieveFhirRecordDifferenceByIdAsync(It.IsAny<Guid>()))
                    .ReturnsAsync(storageFhirRecordDifference);

            // when
            ActionResult<FhirRecordDifference> actualActionResult = await fhirRecordDifferencesController.GetFhirRecordDifferenceByIdAsync(inputId);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            fhirRecordDifferenceServiceMock
                .Verify(service => service.RetrieveFhirRecordDifferenceByIdAsync(It.IsAny<Guid>()),
                    Times.Once);

            fhirRecordDifferenceServiceMock.VerifyNoOtherCalls();
        }
    }
}
