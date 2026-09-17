// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Manage.Models.Foundations.Patients;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RESTFulSense.Clients.Extensions;

namespace LondonFhirService.Manage.Tests.Unit.Controllers.Patients
{
    public partial class PatientControllerTests
    {
        [Fact]
        public async Task ShouldReturnOkOnPostGetStructuredRecordAsync()
        {
            // given
            StructuredRecordRequest randomStructuredRecordRequest =
                CreateRandomStructuredRecordRequest();

            StructuredRecordRequest inputStructuredRecordRequest = randomStructuredRecordRequest;
            string randomStructuredRecord = GetRandomString();
            string retrievedStructuredRecord = randomStructuredRecord;
            string expectedStructuredRecord = retrievedStructuredRecord;
            var expectedObjectResult = new OkObjectResult(expectedStructuredRecord);
            var expectedActionResult = new ActionResult<string>(expectedObjectResult);

            patientServiceMock
                .Setup(service => service.GetStructuredRecord(
                    inputStructuredRecordRequest,
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(retrievedStructuredRecord);

            // when
            ActionResult<string> actualActionResult =
                await patientController.PostGetStructuredRecordAsync(
                    randomStructuredRecordRequest,
                    TestContext.Current.CancellationToken);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            patientServiceMock
                .Verify(service => service.GetStructuredRecord(
                    inputStructuredRecordRequest,
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            patientServiceMock.VerifyNoOtherCalls();
        }

        /// <summary>
        /// The request's own token reaches the service, so a browser that navigates away mid
        /// request aborts the provider call rather than leaving it running to completion.
        /// </summary>
        [Fact]
        public async Task ShouldPassCancellationTokenThroughOnPostGetStructuredRecordAsync()
        {
            // given
            StructuredRecordRequest randomStructuredRecordRequest =
                CreateRandomStructuredRecordRequest();

            StructuredRecordRequest inputStructuredRecordRequest = randomStructuredRecordRequest;
            using var cancellationTokenSource = new CancellationTokenSource();
            CancellationToken inputCancellationToken = cancellationTokenSource.Token;

            patientServiceMock
                .Setup(service => service.GetStructuredRecord(
                    It.IsAny<StructuredRecordRequest>(),
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(GetRandomString());

            // when
            await patientController.PostGetStructuredRecordAsync(
                randomStructuredRecordRequest,
                inputCancellationToken);

            // then
            patientServiceMock
                .Verify(service => service.GetStructuredRecord(
                    inputStructuredRecordRequest,
                    inputCancellationToken),
                        Times.Once);

            patientServiceMock.VerifyNoOtherCalls();
        }
    }
}
