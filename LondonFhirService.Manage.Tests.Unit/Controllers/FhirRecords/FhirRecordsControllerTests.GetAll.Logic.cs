// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Linq;
using System.Threading.Tasks;
using Force.DeepCloner;
using LondonFhirService.Core.Models.Foundations.FhirRecords;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RESTFulSense.Clients.Extensions;

namespace LondonFhirService.Manage.Tests.Unit.Controllers.FhirRecords
{
    public partial class FhirRecordsControllerTests
    {
        [Fact]
        public async Task ShouldReturnRecordsOnGetAsync()
        {
            // given
            IQueryable<FhirRecord> randomFhirRecords = CreateRandomFhirRecords();
            IQueryable<FhirRecord> storageFhirRecords = randomFhirRecords.DeepClone();
            IQueryable<FhirRecord> expectedFhirRecord = storageFhirRecords.DeepClone();

            var expectedObjectResult =
                new OkObjectResult(expectedFhirRecord);

            var expectedActionResult =
                new ActionResult<IQueryable<FhirRecord>>(expectedObjectResult);

            fhirRecordServiceMock
                .Setup(service => service.RetrieveAllFhirRecordsAsync())
                    .ReturnsAsync(storageFhirRecords);

            // when
            ActionResult<IQueryable<FhirRecord>> actualActionResult = await fhirRecordsController.Get();

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            fhirRecordServiceMock
               .Verify(service => service.RetrieveAllFhirRecordsAsync(),
                   Times.Once);

            fhirRecordServiceMock.VerifyNoOtherCalls();
        }
    }
}
