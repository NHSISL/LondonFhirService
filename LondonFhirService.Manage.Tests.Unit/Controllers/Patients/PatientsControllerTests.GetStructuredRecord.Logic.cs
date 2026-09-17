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
    public partial class PatientsControllerTests
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
            CancellationToken inputCancellationToken = TestContext.Current.CancellationToken;

            this.patientServiceMock
                .Setup(service => service.GetStructuredRecordAsync(
                    inputStructuredRecordRequest,
                    inputCancellationToken))
                        .ReturnsAsync(retrievedStructuredRecord);

            // when
            ActionResult<string> actualActionResult =
                await this.patientsController.PostGetStructuredRecordAsync(
                    randomStructuredRecordRequest,
                    TestContext.Current.CancellationToken);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.patientServiceMock
                .Verify(service => service.GetStructuredRecordAsync(
                    inputStructuredRecordRequest,
                    inputCancellationToken),
                        Times.Once);

            this.patientServiceMock.VerifyNoOtherCalls();
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

            this.patientServiceMock
                .Setup(service => service.GetStructuredRecordAsync(
                    inputStructuredRecordRequest,
                    inputCancellationToken))
                        .ReturnsAsync(GetRandomString());

            // when
            await this.patientsController.PostGetStructuredRecordAsync(
                randomStructuredRecordRequest,
                inputCancellationToken);

            // then
            this.patientServiceMock
                .Verify(service => service.GetStructuredRecordAsync(
                    inputStructuredRecordRequest,
                    inputCancellationToken),
                        Times.Once);

            this.patientServiceMock.VerifyNoOtherCalls();
        }
    }
}
