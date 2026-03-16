// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using Force.DeepCloner;
using LondonFhirService.Manage.Models.Foundations.FhirRecords;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RESTFulSense.Clients.Extensions;

namespace LondonFhirService.Manage.Tests.Unit.Controllers.FhirRecords
{
    public partial class FhirRecordsControllerTests
    {
        [Fact]
        public async Task ShouldRemoveRecordOnDeleteByIdsAsync()
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
                .Setup(service => service.RemoveFhirRecordByIdAsync(It.IsAny<Guid>()))
                    .ReturnsAsync(storageFhirRecord);

            // when
            ActionResult<FhirRecord> actualActionResult = await fhirRecordsController.DeleteFhirRecordByIdAsync(inputId);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            fhirRecordServiceMock
                .Verify(service => service.RemoveFhirRecordByIdAsync(It.IsAny<Guid>()),
                    Times.Once);

            fhirRecordServiceMock.VerifyNoOtherCalls();
        }
    }
}
