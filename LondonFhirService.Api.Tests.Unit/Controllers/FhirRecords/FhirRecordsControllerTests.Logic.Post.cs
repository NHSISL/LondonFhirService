// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading.Tasks;
using Force.DeepCloner;
using LondonFhirService.Core.Models.Foundations.FhirRecords;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RESTFulSense.Clients.Extensions;
using RESTFulSense.Models;

namespace LondonFhirService.Api.Tests.Unit.Controllers.FhirRecords
{
    public partial class FhirRecordsControllerTests
    {
        [Fact]
        public async Task ShouldReturnCreatedOnPostAsync()
        {
            // given
            FhirRecord randomFhirRecord = CreateRandomFhirRecord();
            FhirRecord inputFhirRecord = randomFhirRecord;
            FhirRecord addedFhirRecord = inputFhirRecord.DeepClone();
            FhirRecord expectedFhirRecord = addedFhirRecord.DeepClone();

            var expectedObjectResult =
                new CreatedObjectResult(expectedFhirRecord);

            var expectedActionResult =
                new ActionResult<FhirRecord>(expectedObjectResult);

            fhirRecordServiceMock
                .Setup(service => service.AddFhirRecordAsync(inputFhirRecord))
                    .ReturnsAsync(addedFhirRecord);

            // when
            ActionResult<FhirRecord> actualActionResult =
                await fhirRecordsController.PostFhirRecordAsync(randomFhirRecord);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            fhirRecordServiceMock
                .Verify(service => service.AddFhirRecordAsync(inputFhirRecord),
                    Times.Once);

            fhirRecordServiceMock.VerifyNoOtherCalls();
        }
    }
}
