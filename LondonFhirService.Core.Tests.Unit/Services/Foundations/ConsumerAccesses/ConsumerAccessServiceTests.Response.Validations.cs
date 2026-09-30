// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
using LondonFhirService.Core.Models.Brokers.ConsumerAccesses;
using LondonFhirService.Core.Models.Foundations.ConsumerAccesses.Exceptions;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ConsumerAccesses
{
    /// <summary>
    /// The response belongs to a third party, so it is checked here rather than dereferenced
    /// upstream. A 2xx carrying the literal JSON null deserialises to null, and an explicit null
    /// list overwrites the model's initialisers - either one used to surface as a
    /// NullReferenceException in the orchestration, which lost the compliance audit for that
    /// access decision on the way past.
    /// </summary>
    public partial class ConsumerAccessServiceTests
    {
        // A 403 whose body is null is not an access decision, so it is a refusal of this service
        // rather than of the consumer - see ConsumerAccessServiceTests.Response.Exceptions.
        [Theory]
        [InlineData(HttpStatusCode.OK)]
        public async Task ShouldThrowValidationExceptionOnCheckConsumerAccessIfResponseIsNullAndLogItAsync(
            HttpStatusCode answeredStatusCode)
        {
            // given
            ValidateAccessRequest randomValidateAccessRequest = CreateRandomValidateAccessRequest();
            ValidateAccessRequest inputValidateAccessRequest = randomValidateAccessRequest;
            ConsumerAccess nullConsumerAccess = null;

            ConsumerAccessResponse returnedConsumerAccessResponse =
                CreateConsumerAccessResponse(answeredStatusCode, nullConsumerAccess);

            var nullConsumerAccessServiceException =
                new NullConsumerAccessServiceException(
                    message: "Consumer access response is null.");

            var expectedConsumerAccessServiceValidationException =
                new ConsumerAccessServiceValidationException(
                    message: "ConsumerAccess validation error occurred, please fix errors and try again.",
                    innerException: nullConsumerAccessServiceException);

            this.consumerAccessBrokerMock.Setup(broker =>
                broker.CheckConsumerAccessAsync(
                    It.IsAny<ValidateAccessRequest>(),
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(returnedConsumerAccessResponse);

            // when
            ValueTask<ConsumerAccess> checkConsumerAccessTask =
                this.consumerAccessService.CheckConsumerAccessAsync(
                    inputValidateAccessRequest, TestContext.Current.CancellationToken);

            ConsumerAccessServiceValidationException actualConsumerAccessServiceValidationException =
                await Assert.ThrowsAsync<ConsumerAccessServiceValidationException>(
                    testCode: checkConsumerAccessTask.AsTask);

            // then
            // Localised here, so an unusable access response is a ConsumerAccess validation
            // failure rather than a NullReferenceException surfacing three layers up.
            actualConsumerAccessServiceValidationException.Should()
                .BeEquivalentTo(expectedConsumerAccessServiceValidationException);

            this.consumerAccessBrokerMock.Verify(broker =>
                broker.CheckConsumerAccessAsync(
                    inputValidateAccessRequest, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConsumerAccessServiceValidationException))),
                        Times.Once);

            this.consumerAccessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnEmptyListsOnCheckConsumerAccessIfResponseListsAreNullAsync()
        {
            // given
            ValidateAccessRequest randomValidateAccessRequest = CreateRandomValidateAccessRequest();
            ValidateAccessRequest inputValidateAccessRequest = randomValidateAccessRequest;
            ConsumerAccess randomConsumerAccess = CreateRandomConsumerAccess();
            randomConsumerAccess.Reasons = null;
            randomConsumerAccess.AllowedViaOrganisations = null;
            randomConsumerAccess.AllowedViaInformationSharingAgreements = null;
            ConsumerAccess returnedConsumerAccess = randomConsumerAccess;

            ConsumerAccessResponse returnedConsumerAccessResponse =
                CreateConsumerAccessResponse(HttpStatusCode.OK, returnedConsumerAccess);

            // Snapshotted before the call, because the service normalises the response in place -
            // reading the expectation back off the same instance afterwards would assert nothing.
            ConsumerAccess expectedConsumerAccess = returnedConsumerAccess.DeepClone();
            expectedConsumerAccess.Reasons = new List<AccessReason>();
            expectedConsumerAccess.AllowedViaOrganisations = new List<string>();
            expectedConsumerAccess.AllowedViaInformationSharingAgreements = new List<string>();
            using var cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            this.consumerAccessBrokerMock.Setup(broker =>
                broker.CheckConsumerAccessAsync(inputValidateAccessRequest, cancellationToken))
                    .ReturnsAsync(returnedConsumerAccessResponse);

            // when
            ConsumerAccess actualConsumerAccess = await this.consumerAccessService
                .CheckConsumerAccessAsync(inputValidateAccessRequest, cancellationToken);

            // then
            // Empty rather than null: callers enumerate these to build the audit of the access
            // decision, and an absent list is not the same thing as an unusable response.
            actualConsumerAccess.Reasons.Should().NotBeNull().And.BeEmpty();
            actualConsumerAccess.AllowedViaOrganisations.Should().NotBeNull().And.BeEmpty();
            actualConsumerAccess.AllowedViaInformationSharingAgreements.Should().NotBeNull().And.BeEmpty();

            // Nothing else about the decision is touched on the way through.
            actualConsumerAccess.Should().BeEquivalentTo(expectedConsumerAccess);

            this.consumerAccessBrokerMock.Verify(broker =>
                broker.CheckConsumerAccessAsync(inputValidateAccessRequest, cancellationToken),
                    Times.Once);

            this.consumerAccessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnCheckConsumerAccessIfBrokerResponseIsNullAndLogItAsync()
        {
            // given
            ValidateAccessRequest randomValidateAccessRequest = CreateRandomValidateAccessRequest();
            ValidateAccessRequest inputValidateAccessRequest = randomValidateAccessRequest;
            ConsumerAccessResponse nullConsumerAccessResponse = null;

            var nullConsumerAccessServiceException =
                new NullConsumerAccessServiceException(
                    message: "Consumer access response is null.");

            var expectedConsumerAccessServiceValidationException =
                new ConsumerAccessServiceValidationException(
                    message: "ConsumerAccess validation error occurred, please fix errors and try again.",
                    innerException: nullConsumerAccessServiceException);

            this.consumerAccessBrokerMock.Setup(broker =>
                broker.CheckConsumerAccessAsync(
                    It.IsAny<ValidateAccessRequest>(),
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(nullConsumerAccessResponse);

            // when
            ValueTask<ConsumerAccess> checkConsumerAccessTask =
                this.consumerAccessService.CheckConsumerAccessAsync(
                    inputValidateAccessRequest, TestContext.Current.CancellationToken);

            ConsumerAccessServiceValidationException actualConsumerAccessServiceValidationException =
                await Assert.ThrowsAsync<ConsumerAccessServiceValidationException>(
                    testCode: checkConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessServiceValidationException.Should()
                .BeEquivalentTo(expectedConsumerAccessServiceValidationException);

            this.consumerAccessBrokerMock.Verify(broker =>
                broker.CheckConsumerAccessAsync(
                    inputValidateAccessRequest, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConsumerAccessServiceValidationException))),
                        Times.Once);

            this.consumerAccessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task
            ShouldThrowDependencyValidationExceptionOnCheckConsumerAccessIfConsumerIsUnknownAndLogItAsync()
        {
            // given
            ValidateAccessRequest randomValidateAccessRequest = CreateRandomValidateAccessRequest();
            ValidateAccessRequest inputValidateAccessRequest = randomValidateAccessRequest;
            string randomDetail = GetRandomString();
            string randomCorrelationId = Guid.NewGuid().ToString("N");

            ConsumerAccessResponse returnedConsumerAccessResponse =
                CreateProblemResponse(
                    HttpStatusCode.Unauthorized,
                    errorCode: "ConsumerUnknown",
                    detail: randomDetail,
                    correlationId: randomCorrelationId);

            var unauthorizedConsumerAccessServiceException =
                new UnauthorizedConsumerAccessServiceException(
                    message: "Consumer access service does not recognise the consumer.");

            AddExpectedResponseData(
                unauthorizedConsumerAccessServiceException,
                HttpStatusCode.Unauthorized,
                errorCode: "ConsumerUnknown",
                detail: randomDetail,
                correlationId: randomCorrelationId,
                contentType: "application/problem+json",
                wwwAuthenticate: string.Empty);

            var expectedConsumerAccessServiceDependencyValidationException =
                new ConsumerAccessServiceDependencyValidationException(
                    message: "ConsumerAccess dependency validation error occurred, please fix errors and try again.",
                    innerException: unauthorizedConsumerAccessServiceException);

            this.consumerAccessBrokerMock.Setup(broker =>
                broker.CheckConsumerAccessAsync(
                    It.IsAny<ValidateAccessRequest>(),
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(returnedConsumerAccessResponse);

            // when
            ValueTask<ConsumerAccess> checkConsumerAccessTask =
                this.consumerAccessService.CheckConsumerAccessAsync(
                    inputValidateAccessRequest, TestContext.Current.CancellationToken);

            ConsumerAccessServiceDependencyValidationException
                actualConsumerAccessServiceDependencyValidationException =
                    await Assert.ThrowsAsync<ConsumerAccessServiceDependencyValidationException>(
                        testCode: checkConsumerAccessTask.AsTask);

            // then
            // A 401 with errorCode ConsumerUnknown is the dependency saying it does not know this
            // consumer - an answer about the caller, not a fault in the dependency. It is the only
            // 401 categorised apart from a critical dependency failure, so the orchestration can
            // send it down the same unauthorized path an unidentified caller takes. Every other
            // 401 is this service's own credentials being refused - see Response.Exceptions.
            actualConsumerAccessServiceDependencyValidationException.Should()
                .BeEquivalentTo(expectedConsumerAccessServiceDependencyValidationException);

            this.consumerAccessBrokerMock.Verify(broker =>
                broker.CheckConsumerAccessAsync(
                    inputValidateAccessRequest, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConsumerAccessServiceDependencyValidationException))),
                        Times.Once);

            this.consumerAccessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
