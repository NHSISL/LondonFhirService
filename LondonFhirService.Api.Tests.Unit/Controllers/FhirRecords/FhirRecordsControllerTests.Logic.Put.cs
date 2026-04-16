// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading.Tasks;
using Force.DeepCloner;
using LondonFhirService.Core.Models.Foundations.FhirRecords;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RESTFulSense.Clients.Extensions;

namespace LondonFhirService.Api.Tests.Unit.Controllers.FhirRecords
{
    public partial class FhirRecordsControllerTests
    {
        [Fact]
        public async Task ShouldReturnOkOnPutAsync()
        {
            // given
            FhirRecord randomFhirRecord = CreateRandomFhirRecord();
            FhirRecord inputFhirRecord = randomFhirRecord;
            FhirRecord storageFhirRecord = inputFhirRecord.DeepClone();
            FhirRecord expectedFhirRecord = storageFhirRecord.DeepClone();

            var expectedObjectResult =
                new OkObjectResult(expectedFhirRecord);

            var expectedActionResult =
                new ActionResult<FhirRecord>(expectedObjectResult);

            fhirRecordServiceMock
                .Setup(service => service.ModifyFhirRecordAsync(inputFhirRecord))
                    .ReturnsAsync(storageFhirRecord);

            // when
            ActionResult<FhirRecord> actualActionResult =
                await fhirRecordsController.PutFhirRecordAsync(randomFhirRecord);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            fhirRecordServiceMock
                .Verify(service => service.ModifyFhirRecordAsync(inputFhirRecord),
                    Times.Once);

            fhirRecordServiceMock.VerifyNoOtherCalls();
        }
    }
}
