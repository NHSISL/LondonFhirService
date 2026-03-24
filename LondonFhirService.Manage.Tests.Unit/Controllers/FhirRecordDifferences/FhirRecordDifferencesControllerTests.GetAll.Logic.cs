// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Linq;
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
        public async Task ShouldReturnRecordsOnGetAsync()
        {
            // given
            IQueryable<FhirRecordDifference> randomFhirRecordDifferences = CreateRandomFhirRecordDifferences();
            IQueryable<FhirRecordDifference> storageFhirRecordDifferences = randomFhirRecordDifferences.DeepClone();
            IQueryable<FhirRecordDifference> expectedFhirRecordDifference = storageFhirRecordDifferences.DeepClone();

            var expectedObjectResult =
                new OkObjectResult(expectedFhirRecordDifference);

            var expectedActionResult =
                new ActionResult<IQueryable<FhirRecordDifference>>(expectedObjectResult);

            fhirRecordDifferenceServiceMock
                .Setup(service => service.RetrieveAllFhirRecordDifferencesAsync())
                    .ReturnsAsync(storageFhirRecordDifferences);

            // when
            ActionResult<IQueryable<FhirRecordDifference>> actualActionResult = await fhirRecordDifferencesController.Get();

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            fhirRecordDifferenceServiceMock
               .Verify(service => service.RetrieveAllFhirRecordDifferencesAsync(),
                   Times.Once);

            fhirRecordDifferenceServiceMock.VerifyNoOtherCalls();
        }
    }
}
