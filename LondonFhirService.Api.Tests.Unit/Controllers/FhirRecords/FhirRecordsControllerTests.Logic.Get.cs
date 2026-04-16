// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
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
        public async Task ShouldReturnRecordOnGetByIdAsync()
        {
            // given
            FhirRecord randomFhirRecord = CreateRandomFhirRecord();
            Guid inputId = randomFhirRecord.Id;
            FhirRecord storageFhirRecord = randomFhirRecord;
            FhirRecord expectedFhirRecord = storageFhirRecord.DeepClone();

            var expectedObjectResult =
                new OkObjectResult(expectedFhirRecord);

            var expectedActionResult =
                new ActionResult<FhirRecord>(expectedObjectResult);

            fhirRecordServiceMock
                .Setup(service => service.RetrieveFhirRecordByIdAsync(It.IsAny<Guid>()))
                    .ReturnsAsync(storageFhirRecord);

            // when
            ActionResult<FhirRecord> actualActionResult =
                await fhirRecordsController.GetFhirRecordByIdAsync(inputId);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            fhirRecordServiceMock
                .Verify(service => service.RetrieveFhirRecordByIdAsync(It.IsAny<Guid>()),
                    Times.Once);

            fhirRecordServiceMock.VerifyNoOtherCalls();
        }
    }
}
