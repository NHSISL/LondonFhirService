// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Manage.Models.Foundations.Patients;
using LondonFhirService.Manage.Models.Foundations.Patients.Exceptions;
using Moq;

namespace LondonFhirService.Manage.Tests.Unit.Services.Foundations.Patients
{
    public partial class PatientServiceTests
    {
        [Theory]
        [MemberData(nameof(DependencyExceptions))]
        public async Task ShouldThrowDependencyExceptionOnGetStructuredRecordIfTransportFailsAsync(
            Exception dependencyException)
        {
            // given
            StructuredRecordRequest randomStructuredRecordRequest =
                CreateRandomStructuredRecordRequest();

            StructuredRecordRequest inputStructuredRecordRequest = randomStructuredRecordRequest;

            var failedPatientDependencyException =
                new FailedPatientDependencyException(
                    message: "Failed patient dependency error occurred, contact support.",
                    innerException: dependencyException,
                    data: dependencyException.Data);

            var expectedPatientServiceDependencyException =
                new PatientServiceDependencyException(
                    message: "Patient dependency error occurred, contact support.",
                    innerException: failedPatientDependencyException);

            this.httpBrokerMock.Setup(broker =>
                broker.PostFormUrlEncodedContentAsync(
                    It.IsAny<string>(),
                    It.IsAny<IDictionary<string, string>>(),
                    It.IsAny<CancellationToken>()))
                        .ThrowsAsync(dependencyException);

            // when
            ValueTask<string> getStructuredRecordTask =
                this.patientService.GetStructuredRecord(
                    inputStructuredRecordRequest,
                    TestContext.Current.CancellationToken);

            PatientServiceDependencyException actualPatientServiceDependencyException =
                await Assert.ThrowsAsync<PatientServiceDependencyException>(
                    testCode: getStructuredRecordTask.AsTask);

            // then
            actualPatientServiceDependencyException.Should()
                .BeEquivalentTo(expectedPatientServiceDependencyException);

            this.httpBrokerMock.Verify(broker =>
                broker.PostFormUrlEncodedContentAsync(
                    this.patientConfiguration.AuthUrl,
                    It.IsAny<IDictionary<string, string>>(),
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.httpBrokerMock.VerifyNoOtherCalls();
        }

        /// <summary>
        /// The same failure on the second call, which goes down a different path - it happens
        /// after a token has already been obtained, so a catch that only covered the token
        /// exchange would let this one out as an unhandled HttpRequestException.
        /// </summary>
        [Theory]
        [MemberData(nameof(DependencyExceptions))]
        public async Task ShouldThrowDependencyExceptionOnGetStructuredRecordIfRecordCallFailsAsync(
            Exception dependencyException)
        {
            // given
            StructuredRecordRequest randomStructuredRecordRequest =
                CreateRandomStructuredRecordRequest();

            StructuredRecordRequest inputStructuredRecordRequest = randomStructuredRecordRequest;
            string randomAccessToken = GetRandomString();
            string tokenResponse = CreateTokenResponse(randomAccessToken);

            var failedPatientDependencyException =
                new FailedPatientDependencyException(
                    message: "Failed patient dependency error occurred, contact support.",
                    innerException: dependencyException,
                    data: dependencyException.Data);

            var expectedPatientServiceDependencyException =
                new PatientServiceDependencyException(
                    message: "Patient dependency error occurred, contact support.",
                    innerException: failedPatientDependencyException);

            this.httpBrokerMock.Setup(broker =>
                broker.PostFormUrlEncodedContentAsync(
                    It.IsAny<string>(),
                    It.IsAny<IDictionary<string, string>>(),
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(tokenResponse);

            this.httpBrokerMock.Setup(broker =>
                broker.PostJsonContentAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                        .ThrowsAsync(dependencyException);

            // when
            ValueTask<string> getStructuredRecordTask =
                this.patientService.GetStructuredRecord(
                    inputStructuredRecordRequest,
                    TestContext.Current.CancellationToken);

            PatientServiceDependencyException actualPatientServiceDependencyException =
                await Assert.ThrowsAsync<PatientServiceDependencyException>(
                    testCode: getStructuredRecordTask.AsTask);

            // then
            actualPatientServiceDependencyException.Should()
                .BeEquivalentTo(expectedPatientServiceDependencyException);

            this.httpBrokerMock.Verify(broker =>
                broker.PostJsonContentAsync(
                    this.patientConfiguration.GetStructuredRecordUrl,
                    It.IsAny<string>(),
                    randomAccessToken,
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.httpBrokerMock.Verify(broker =>
                broker.PostFormUrlEncodedContentAsync(
                    It.IsAny<string>(),
                    It.IsAny<IDictionary<string, string>>(),
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.httpBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(TimeoutExceptions))]
        public async Task ShouldThrowDependencyExceptionOnGetStructuredRecordIfTimesOutAsync(
            Exception timeoutException)
        {
            // given
            StructuredRecordRequest randomStructuredRecordRequest =
                CreateRandomStructuredRecordRequest();

            StructuredRecordRequest inputStructuredRecordRequest = randomStructuredRecordRequest;

            var timedOutPatientServiceException =
                new TimedOutPatientServiceException(
                    message: "Patient request timed out, please try again.",
                    innerException: timeoutException,
                    data: timeoutException.Data);

            var expectedPatientServiceDependencyException =
                new PatientServiceDependencyException(
                    message: "Patient dependency error occurred, contact support.",
                    innerException: timedOutPatientServiceException);

            this.httpBrokerMock.Setup(broker =>
                broker.PostFormUrlEncodedContentAsync(
                    It.IsAny<string>(),
                    It.IsAny<IDictionary<string, string>>(),
                    It.IsAny<CancellationToken>()))
                        .ThrowsAsync(timeoutException);

            // when
            ValueTask<string> getStructuredRecordTask =
                this.patientService.GetStructuredRecord(
                    inputStructuredRecordRequest,
                    TestContext.Current.CancellationToken);

            PatientServiceDependencyException actualPatientServiceDependencyException =
                await Assert.ThrowsAsync<PatientServiceDependencyException>(
                    testCode: getStructuredRecordTask.AsTask);

            // then
            actualPatientServiceDependencyException.Should()
                .BeEquivalentTo(expectedPatientServiceDependencyException);

            this.httpBrokerMock.Verify(broker =>
                broker.PostFormUrlEncodedContentAsync(
                    this.patientConfiguration.AuthUrl,
                    It.IsAny<IDictionary<string, string>>(),
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.httpBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(CancellationExceptions))]
        public async Task ShouldThrowDependencyExceptionOnGetStructuredRecordIfCancelledAsync(
            Exception cancellationException)
        {
            // given
            StructuredRecordRequest randomStructuredRecordRequest =
                CreateRandomStructuredRecordRequest();

            StructuredRecordRequest inputStructuredRecordRequest = randomStructuredRecordRequest;

            var cancelledPatientServiceException =
                new CancelledPatientServiceException(
                    message: "Patient request was cancelled, please try again.",
                    innerException: cancellationException,
                    data: cancellationException.Data);

            var expectedPatientServiceDependencyException =
                new PatientServiceDependencyException(
                    message: "Patient dependency error occurred, contact support.",
                    innerException: cancelledPatientServiceException);

            this.httpBrokerMock.Setup(broker =>
                broker.PostFormUrlEncodedContentAsync(
                    It.IsAny<string>(),
                    It.IsAny<IDictionary<string, string>>(),
                    It.IsAny<CancellationToken>()))
                        .ThrowsAsync(cancellationException);

            // when
            ValueTask<string> getStructuredRecordTask =
                this.patientService.GetStructuredRecord(
                    inputStructuredRecordRequest,
                    TestContext.Current.CancellationToken);

            PatientServiceDependencyException actualPatientServiceDependencyException =
                await Assert.ThrowsAsync<PatientServiceDependencyException>(
                    testCode: getStructuredRecordTask.AsTask);

            // then
            actualPatientServiceDependencyException.Should()
                .BeEquivalentTo(expectedPatientServiceDependencyException);

            this.httpBrokerMock.Verify(broker =>
                broker.PostFormUrlEncodedContentAsync(
                    this.patientConfiguration.AuthUrl,
                    It.IsAny<IDictionary<string, string>>(),
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.httpBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyExceptionOnGetStructuredRecordIfTokenIsAlreadyCancelledAsync()
        {
            // given
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();
            CancellationToken cancelledToken = cancellationTokenSource.Token;

            StructuredRecordRequest randomStructuredRecordRequest =
                CreateRandomStructuredRecordRequest();

            StructuredRecordRequest inputStructuredRecordRequest = randomStructuredRecordRequest;
            var operationCanceledException = new OperationCanceledException(cancelledToken);

            var cancelledPatientServiceException =
                new CancelledPatientServiceException(
                    message: "Patient request was cancelled, please try again.",
                    innerException: operationCanceledException,
                    data: operationCanceledException.Data);

            var expectedPatientServiceDependencyException =
                new PatientServiceDependencyException(
                    message: "Patient dependency error occurred, contact support.",
                    innerException: cancelledPatientServiceException);

            // when
            ValueTask<string> getStructuredRecordTask =
                this.patientService.GetStructuredRecord(
                    inputStructuredRecordRequest,
                    cancelledToken);

            PatientServiceDependencyException actualPatientServiceDependencyException =
                await Assert.ThrowsAsync<PatientServiceDependencyException>(
                    testCode: getStructuredRecordTask.AsTask);

            // then
            actualPatientServiceDependencyException.Should()
                .BeEquivalentTo(expectedPatientServiceDependencyException);

            // Neither call is made. A caller that has already given up should not cost a token
            // exchange, let alone a patient lookup.
            this.httpBrokerMock.VerifyNoOtherCalls();
        }

        /// <summary>
        /// The token is checked before the request is, so an abandoned call is reported as
        /// cancelled rather than as whatever happens to be wrong with its arguments.
        /// </summary>
        [Fact]
        public async Task ShouldThrowDependencyExceptionBeforeValidationOnGetStructuredRecordIfTokenIsAlreadyCancelledAsync()
        {
            // given
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();
            CancellationToken cancelledToken = cancellationTokenSource.Token;
            StructuredRecordRequest nullStructuredRecordRequest = null;
            var operationCanceledException = new OperationCanceledException(cancelledToken);

            var cancelledPatientServiceException =
                new CancelledPatientServiceException(
                    message: "Patient request was cancelled, please try again.",
                    innerException: operationCanceledException,
                    data: operationCanceledException.Data);

            var expectedPatientServiceDependencyException =
                new PatientServiceDependencyException(
                    message: "Patient dependency error occurred, contact support.",
                    innerException: cancelledPatientServiceException);

            // when
            ValueTask<string> getStructuredRecordTask =
                this.patientService.GetStructuredRecord(
                    nullStructuredRecordRequest,
                    cancelledToken);

            PatientServiceDependencyException actualPatientServiceDependencyException =
                await Assert.ThrowsAsync<PatientServiceDependencyException>(
                    testCode: getStructuredRecordTask.AsTask);

            // then
            actualPatientServiceDependencyException.Should()
                .BeEquivalentTo(expectedPatientServiceDependencyException);

            this.httpBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyExceptionOnGetStructuredRecordIfTokenResponseIsMalformedJsonAsync()
        {
            // given
            StructuredRecordRequest randomStructuredRecordRequest =
                CreateRandomStructuredRecordRequest();

            StructuredRecordRequest inputStructuredRecordRequest = randomStructuredRecordRequest;
            string malformedTokenResponse = "{\"access_token\": ";

            this.httpBrokerMock.Setup(broker =>
                broker.PostFormUrlEncodedContentAsync(
                    It.IsAny<string>(),
                    It.IsAny<IDictionary<string, string>>(),
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(malformedTokenResponse);

            // when
            ValueTask<string> getStructuredRecordTask =
                this.patientService.GetStructuredRecord(
                    inputStructuredRecordRequest,
                    TestContext.Current.CancellationToken);

            PatientServiceDependencyException actualPatientServiceDependencyException =
                await Assert.ThrowsAsync<PatientServiceDependencyException>(
                    testCode: getStructuredRecordTask.AsTask);

            // then
            actualPatientServiceDependencyException.InnerException
                .Should().BeOfType<FailedPatientDependencyException>();

            actualPatientServiceDependencyException.InnerException.InnerException
                .Should().BeAssignableTo<JsonException>();

            this.httpBrokerMock.Verify(broker =>
                broker.PostJsonContentAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                        Times.Never);

            this.httpBrokerMock.Verify(broker =>
                broker.PostFormUrlEncodedContentAsync(
                    It.IsAny<string>(),
                    It.IsAny<IDictionary<string, string>>(),
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.httpBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnGetStructuredRecordIfServiceErrorOccursAsync()
        {
            // given
            StructuredRecordRequest randomStructuredRecordRequest =
                CreateRandomStructuredRecordRequest();

            StructuredRecordRequest inputStructuredRecordRequest = randomStructuredRecordRequest;
            var serviceException = new Exception();

            var failedPatientServiceException =
                new FailedPatientServiceException(
                    message: "Failed patient service error occurred, contact support.",
                    innerException: serviceException);

            var expectedPatientServiceException =
                new PatientServiceException(
                    message: "Patient service error occurred, contact support.",
                    innerException: failedPatientServiceException);

            this.httpBrokerMock.Setup(broker =>
                broker.PostFormUrlEncodedContentAsync(
                    It.IsAny<string>(),
                    It.IsAny<IDictionary<string, string>>(),
                    It.IsAny<CancellationToken>()))
                        .ThrowsAsync(serviceException);

            // when
            ValueTask<string> getStructuredRecordTask =
                this.patientService.GetStructuredRecord(
                    inputStructuredRecordRequest,
                    TestContext.Current.CancellationToken);

            PatientServiceException actualPatientServiceException =
                await Assert.ThrowsAsync<PatientServiceException>(
                    testCode: getStructuredRecordTask.AsTask);

            // then
            actualPatientServiceException.Should()
                .BeEquivalentTo(expectedPatientServiceException);

            this.httpBrokerMock.Verify(broker =>
                broker.PostFormUrlEncodedContentAsync(
                    this.patientConfiguration.AuthUrl,
                    It.IsAny<IDictionary<string, string>>(),
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.httpBrokerMock.VerifyNoOtherCalls();
        }
    }
}
