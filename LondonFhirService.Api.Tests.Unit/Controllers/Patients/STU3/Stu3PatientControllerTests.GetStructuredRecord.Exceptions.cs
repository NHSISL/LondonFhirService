// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using FluentAssertions;
using Hl7.Fhir.Model;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RESTFulSense.Clients.Extensions;
using RESTFulSense.Models;
using Xeptions;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Api.Tests.Unit.Controllers.Patients.STU3
{
    public partial class Stu3PatientControllerTests
    {
        [Theory]
        [MemberData(nameof(ValidationExceptions))]
        public async Task ShouldReturnBadRequestOnGetStructuredRecordIfValidationErrorOccurredAsync(
            Xeption validationException)
        {
            // given
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;
            DateTimeOffset randomDateOfBirth = GetRandomDateTimeOffset();
            string inputDateOfBirth = randomDateOfBirth.ToString("yyyy-MM-dd");
            bool inputDemographicsOnly = false;
            bool inputIncludeInactivePatients = false;
            CancellationToken cancellationToken = CancellationToken.None;
            Guid correlationId = Guid.NewGuid();

            Parameters randomParameters = CreateRandomGetStructuredRecordParameters(
                nhsNumber: inputNhsNumber,
                dateOfBirth: inputDateOfBirth,
                demographicsOnly: inputDemographicsOnly,
                includeInactivePatients: inputIncludeInactivePatients);

            Parameters inputParameters = randomParameters;

            BadRequestObjectResult expectedBadRequestObjectResult =
                BadRequest(validationException.InnerException);

            var expectedActionResult = new ActionResult<Bundle>(expectedBadRequestObjectResult);

            this.correlationBrokerMock.Setup(broker =>
                broker.GetCorrelationIdAsync())
                    .ReturnsAsync(correlationId);

            this.patientCoordinationServiceMock.Setup(coordination =>
                coordination.GetStructuredRecordSerialisedAsync(
                    correlationId,
                    inputNhsNumber,
                    inputDateOfBirth,
                    inputDemographicsOnly,
                    inputIncludeInactivePatients,
                    cancellationToken))
                    .ThrowsAsync(validationException);

            // when
            ActionResult<Bundle> actualActionResult =
                await this.patientController.GetStructuredRecord(inputParameters, cancellationToken);

            // then
            actualActionResult.Should().BeEquivalentTo(expectedActionResult);

            this.patientCoordinationServiceMock.Verify(coordination =>
                coordination.GetStructuredRecordSerialisedAsync(
                    correlationId,
                    inputNhsNumber,
                    inputDateOfBirth,
                    inputDemographicsOnly,
                    inputIncludeInactivePatients,
                    cancellationToken),
                        Times.Once);

            this.correlationBrokerMock.Verify(broker =>
                broker.GetCorrelationIdAsync(),
                    Times.Once);

            this.patientCoordinationServiceMock.VerifyNoOtherCalls();
            this.correlationBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(ServerExceptions))]
        public async Task ShouldReturnInternalServerErrorOnGetStructuredRecordIfServerErrorOccurredAsync(
            Xeption serverException)
        {
            // given
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;
            DateTimeOffset randomDateOfBirth = GetRandomDateTimeOffset();
            string inputDateOfBirth = randomDateOfBirth.ToString("yyyy-MM-dd");
            bool inputDemographicsOnly = false;
            bool inputIncludeInactivePatients = false;
            CancellationToken cancellationToken = CancellationToken.None;
            Guid correlationId = Guid.NewGuid();

            Parameters randomParameters = CreateRandomGetStructuredRecordParameters(
                nhsNumber: inputNhsNumber,
                dateOfBirth: inputDateOfBirth,
                demographicsOnly: inputDemographicsOnly,
                includeInactivePatients: inputIncludeInactivePatients);

            Parameters inputParameters = randomParameters;

            InternalServerErrorObjectResult expectedInternalServerErrorObjectResult =
                InternalServerError(new Xeption(message: serverException.InnerException.Message));

            var expectedActionResult = new ActionResult<Bundle>(expectedInternalServerErrorObjectResult);

            this.correlationBrokerMock.Setup(broker =>
                broker.GetCorrelationIdAsync())
                    .ReturnsAsync(correlationId);

            this.patientCoordinationServiceMock.Setup(coordination =>
                coordination.GetStructuredRecordSerialisedAsync(
                    correlationId,
                    inputNhsNumber,
                    inputDateOfBirth,
                    inputDemographicsOnly,
                    inputIncludeInactivePatients,
                    cancellationToken))
                        .ThrowsAsync(serverException);

            // when
            ActionResult<Bundle> actualActionResult =
                await this.patientController.GetStructuredRecord(inputParameters, cancellationToken);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.patientCoordinationServiceMock.Verify(coordination =>
                coordination.GetStructuredRecordSerialisedAsync(
                    correlationId,
                    inputNhsNumber,
                    inputDateOfBirth,
                    inputDemographicsOnly,
                    inputIncludeInactivePatients,
                    cancellationToken),
                        Times.Once);

            this.correlationBrokerMock.Verify(broker =>
                broker.GetCorrelationIdAsync(),
                    Times.Once);

            this.patientCoordinationServiceMock.VerifyNoOtherCalls();
            this.correlationBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(CancellationExceptions))]
        public async Task ShouldPassCancellationThroughOnGetStructuredRecordAsync(
            Exception cancellationException)
        {
            // given
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;
            DateTimeOffset randomDateOfBirth = GetRandomDateTimeOffset();
            string inputDateOfBirth = randomDateOfBirth.ToString("yyyy-MM-dd");
            bool inputDemographicsOnly = false;
            bool inputIncludeInactivePatients = false;
            CancellationToken cancellationToken = CancellationToken.None;
            Guid correlationId = Guid.NewGuid();

            Parameters inputParameters = CreateRandomGetStructuredRecordParameters(
                nhsNumber: inputNhsNumber,
                dateOfBirth: inputDateOfBirth,
                demographicsOnly: inputDemographicsOnly,
                includeInactivePatients: inputIncludeInactivePatients);

            this.correlationBrokerMock.Setup(broker =>
                broker.GetCorrelationIdAsync())
                    .ReturnsAsync(correlationId);

            this.patientCoordinationServiceMock.Setup(coordination =>
                coordination.GetStructuredRecordSerialisedAsync(
                    correlationId,
                    inputNhsNumber,
                    inputDateOfBirth,
                    inputDemographicsOnly,
                    inputIncludeInactivePatients,
                    cancellationToken))
                    .ThrowsAsync(cancellationException);

            // when
            Func<Task> getStructuredRecord = async () =>
                await this.patientController.GetStructuredRecord(inputParameters, cancellationToken);

            // then
            // Not mapped to a status code here. The request timeout middleware answers a timed
            // out request with 504, and a client that hung up is not sent anything at all.
            (await getStructuredRecord.Should().ThrowAsync<OperationCanceledException>())
                .Which.Should().BeSameAs(cancellationException);

            this.patientCoordinationServiceMock.Verify(coordination =>
                coordination.GetStructuredRecordSerialisedAsync(
                    correlationId,
                    inputNhsNumber,
                    inputDateOfBirth,
                    inputDemographicsOnly,
                    inputIncludeInactivePatients,
                    cancellationToken),
                        Times.Once);

            this.correlationBrokerMock.Verify(broker =>
                broker.GetCorrelationIdAsync(),
                    Times.Once);

            this.patientCoordinationServiceMock.VerifyNoOtherCalls();
            this.correlationBrokerMock.VerifyNoOtherCalls();
        }
    }
}
