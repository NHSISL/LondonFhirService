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

        /// <summary>
        /// Cancellation reaches the caller as itself. It is not a failure of this service or of
        /// anything it depends on - it is the caller withdrawing - so wrapping it would both
        /// misreport it and stop an upstream caller recognising its own cancellation.
        /// </summary>
        [Theory]
        [MemberData(nameof(CancellationExceptions))]
        public async Task ShouldPropagateCancellationOnGetStructuredRecordIfCancelledAsync(
            Exception cancellationException)
        {
            // given
            StructuredRecordRequest randomStructuredRecordRequest =
                CreateRandomStructuredRecordRequest();

            StructuredRecordRequest inputStructuredRecordRequest = randomStructuredRecordRequest;

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

            OperationCanceledException actualOperationCanceledException =
                await Assert.ThrowsAnyAsync<OperationCanceledException>(
                    testCode: getStructuredRecordTask.AsTask);

            // then
            // The very same instance, not an equivalent one - anything else means it was caught
            // and rebuilt somewhere on the way out.
            actualOperationCanceledException.Should().BeSameAs(cancellationException);

            this.httpBrokerMock.Verify(broker =>
                broker.PostFormUrlEncodedContentAsync(
                    this.patientConfiguration.AuthUrl,
                    It.IsAny<IDictionary<string, string>>(),
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.httpBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldPropagateCancellationOnGetStructuredRecordIfTokenIsAlreadyCancelledAsync()
        {
            // given
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();
            CancellationToken cancelledToken = cancellationTokenSource.Token;

            StructuredRecordRequest randomStructuredRecordRequest =
                CreateRandomStructuredRecordRequest();

            StructuredRecordRequest inputStructuredRecordRequest = randomStructuredRecordRequest;

            // when
            ValueTask<string> getStructuredRecordTask =
                this.patientService.GetStructuredRecord(
                    inputStructuredRecordRequest,
                    cancelledToken);

            OperationCanceledException actualOperationCanceledException =
                await Assert.ThrowsAnyAsync<OperationCanceledException>(
                    testCode: getStructuredRecordTask.AsTask);

            // then
            actualOperationCanceledException.CancellationToken.Should().Be(cancelledToken);

            // Neither call is made. A caller that has already given up should not cost a token
            // exchange, let alone a patient lookup.
            this.httpBrokerMock.VerifyNoOtherCalls();
        }

        /// <summary>
        /// The token is checked before the request is, so an abandoned call surfaces as
        /// cancellation rather than as whatever happens to be wrong with its arguments.
        /// </summary>
        [Fact]
        public async Task ShouldPropagateCancellationBeforeValidationOnGetStructuredRecordIfTokenIsAlreadyCancelledAsync()
        {
            // given
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();
            CancellationToken cancelledToken = cancellationTokenSource.Token;
            StructuredRecordRequest nullStructuredRecordRequest = null;

            // when
            ValueTask<string> getStructuredRecordTask =
                this.patientService.GetStructuredRecord(
                    nullStructuredRecordRequest,
                    cancelledToken);

            OperationCanceledException actualOperationCanceledException =
                await Assert.ThrowsAnyAsync<OperationCanceledException>(
                    testCode: getStructuredRecordTask.AsTask);

            // then
            // Cancellation, not the PatientServiceValidationException a null request would
            // otherwise produce.
            actualOperationCanceledException.CancellationToken.Should().Be(cancelledToken);

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
