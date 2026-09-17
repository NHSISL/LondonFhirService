// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using FluentAssertions;
using Moq;
using RESTFulSense.Clients.Extensions;

using LondonFhirService.Core.Brokers.Correlations;
using LondonFhirService.Manage.Models.Foundations.Patients;

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
            string randomCorrelationId = GetRandomCorrelationId();

            var retrievedStructuredRecordResponse = new StructuredRecordResponse
            {
                PayloadText = randomStructuredRecord,
                CorrelationId = randomCorrelationId
            };

            string expectedStructuredRecord = randomStructuredRecord;

            // A ContentResult carrying text/plain, not an OkObjectResult. Ok(string) is
            // content-negotiated, so a caller asking only for application/json would have had the
            // provider's payload serialised as a quoted JSON string - which is not the verbatim
            // body this endpoint promises.
            var expectedContentResult = new ContentResult
            {
                Content = expectedStructuredRecord,
                ContentType = "text/plain",
                StatusCode = null
            };

            var expectedActionResult = new ActionResult<string>(expectedContentResult);
            CancellationToken inputCancellationToken = TestContext.Current.CancellationToken;

            this.patientServiceMock
                .Setup(service => service.GetStructuredRecordAsync(
                    inputStructuredRecordRequest,
                    inputCancellationToken))
                        .ReturnsAsync(retrievedStructuredRecordResponse);

            // when
            ActionResult<string> actualActionResult =
                await this.patientsController.PostGetStructuredRecordAsync(
                    randomStructuredRecordRequest,
                    TestContext.Current.CancellationToken);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            // The payload goes in the body and the id goes on a header, so the page can follow the
            // call into its comparisons without the payload being wrapped to carry it.
            this.patientsController.Response.Headers[CorrelationBroker.CorrelationIdHeaderName]
                .ToString().Should().Be(randomCorrelationId);

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
                        .ReturnsAsync(new StructuredRecordResponse
                        {
                            PayloadText = GetRandomString(),
                            CorrelationId = GetRandomCorrelationId()
                        });

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
