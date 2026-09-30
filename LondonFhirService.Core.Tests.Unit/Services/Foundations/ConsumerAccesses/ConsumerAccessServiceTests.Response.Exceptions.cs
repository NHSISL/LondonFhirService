// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Brokers.ConsumerAccesses;
using LondonFhirService.Core.Models.Foundations.ConsumerAccesses.Exceptions;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ConsumerAccesses
{
    /// <summary>
    /// The 401s and 403s that are not about the consumer at all. ConsumerAccessService refuses
    /// this service's own credentials with the same two statuses it uses for its answers about a
    /// consumer, and tells them apart only by the errorCode on an application/problem+json body.
    /// A refusal of this service is a configuration fault - a wrong scope, an expired or
    /// misaddressed token, a missing app role - so it has to be loud: a critical dependency
    /// failure, logged as such and answered with a 500, never read as an unknown consumer and
    /// answered with a 404, where it would look like every consumer had vanished at once.
    /// </summary>
    public partial class ConsumerAccessServiceTests
    {
        private const string CheckScopeAdvice =
            "Check ConsumerAccessConfiguration:scope and this service's managed identity / app registration.";

        private const string GrantRoleAdvice =
            "Grant its identity the required app role on the Consumer Access Service app registration.";

        [Theory]
        [InlineData("BearerTokenMissing", "No bearer token was supplied.")]
        [InlineData("BearerTokenInvalid", "The token has expired.")]
        [InlineData("BearerTokenInvalid", "The audience is invalid; expected 'api://consumer-access'.")]
        [InlineData("AnErrorCodeThisServiceDoesNotKnow", "Something new went wrong.")]
        [InlineData(null, "A problem with no error code.")]
        [InlineData("BearerTokenInvalid", null)]
        public async Task
            ShouldThrowCriticalDependencyExceptionOnCheckConsumerAccessIfCredentialsAreRejectedAndLogItAsync(
                string errorCode,
                string detail)
        {
            // given
            ValidateAccessRequest randomValidateAccessRequest = CreateRandomValidateAccessRequest();
            ValidateAccessRequest inputValidateAccessRequest = randomValidateAccessRequest;
            string randomCorrelationId = Guid.NewGuid().ToString("N");

            ConsumerAccessResponse returnedConsumerAccessResponse =
                CreateProblemResponse(
                    HttpStatusCode.Unauthorized,
                    errorCode,
                    detail,
                    correlationId: randomCorrelationId);

            var failedConsumerAccessAuthenticationException =
                new FailedConsumerAccessAuthenticationException(
                    message:
                        "Consumer Access Service rejected this service's bearer token " +
                        $"({errorCode ?? "no error code"}: {detail ?? "no detail"}). {CheckScopeAdvice}");

            AddExpectedResponseData(
                failedConsumerAccessAuthenticationException,
                HttpStatusCode.Unauthorized,
                errorCode,
                detail,
                randomCorrelationId,
                contentType: "application/problem+json",
                wwwAuthenticate: string.Empty);

            var expectedConsumerAccessServiceDependencyException =
                new ConsumerAccessServiceDependencyException(
                    message: "ConsumerAccess dependency error occurred, contact support.",
                    innerException: failedConsumerAccessAuthenticationException);

            this.consumerAccessBrokerMock.Setup(broker =>
                broker.CheckConsumerAccessAsync(
                    It.IsAny<ValidateAccessRequest>(),
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(returnedConsumerAccessResponse);

            // when
            ValueTask<ConsumerAccess> checkConsumerAccessTask =
                this.consumerAccessService.CheckConsumerAccessAsync(
                    inputValidateAccessRequest, TestContext.Current.CancellationToken);

            ConsumerAccessServiceDependencyException actualConsumerAccessServiceDependencyException =
                await Assert.ThrowsAsync<ConsumerAccessServiceDependencyException>(
                    testCode: checkConsumerAccessTask.AsTask);

            // then
            // Only errorCode ConsumerUnknown is a 401 about the consumer. Every other one - a
            // missing token, a rejected one, a code this service has not been taught, no code at
            // all - is this service's credentials being refused, and says so in the message.
            actualConsumerAccessServiceDependencyException.Should()
                .BeEquivalentTo(expectedConsumerAccessServiceDependencyException);

            this.consumerAccessBrokerMock.Verify(broker =>
                broker.CheckConsumerAccessAsync(
                    inputValidateAccessRequest, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedConsumerAccessServiceDependencyException))),
                        Times.Once);

            this.consumerAccessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(null, "", "Bearer error=\"invalid_token\"")]
        [InlineData(null, "", "")]
        [InlineData("text/html", "<html><body>401 Unauthorized</body></html>", "")]
        [InlineData("application/problem+json", "not json", "")]
        [InlineData("application/json", "{\"errorCode\":\"ConsumerUnknown\"}", "")]
        public async Task
            ShouldThrowCriticalDependencyExceptionOnCheckConsumerAccessIfUnauthorizedWithoutProblemAndLogItAsync(
                string contentType,
                string content,
                string wwwAuthenticate)
        {
            // given
            ValidateAccessRequest randomValidateAccessRequest = CreateRandomValidateAccessRequest();
            ValidateAccessRequest inputValidateAccessRequest = randomValidateAccessRequest;

            var returnedConsumerAccessResponse = new ConsumerAccessResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                ContentType = contentType,
                WwwAuthenticate = wwwAuthenticate,
                Content = content
            };

            string expectedContentType = string.IsNullOrEmpty(contentType) ? "none" : contentType;
            string expectedWwwAuthenticate = string.IsNullOrEmpty(wwwAuthenticate) ? "none" : wwwAuthenticate;

            var failedConsumerAccessAuthenticationException =
                new FailedConsumerAccessAuthenticationException(
                    message:
                        "Consumer Access Service answered 401 without a readable problem details body " +
                        $"(content type: {expectedContentType}; WWW-Authenticate: {expectedWwwAuthenticate}), " +
                        "so the refusal did not come from its access decision - most likely a gateway or its " +
                        $"authentication rejected this service's bearer token. {CheckScopeAdvice}");

            AddExpectedResponseData(
                failedConsumerAccessAuthenticationException,
                HttpStatusCode.Unauthorized,
                errorCode: null,
                detail: null,
                correlationId: null,
                contentType,
                wwwAuthenticate);

            var expectedConsumerAccessServiceDependencyException =
                new ConsumerAccessServiceDependencyException(
                    message: "ConsumerAccess dependency error occurred, contact support.",
                    innerException: failedConsumerAccessAuthenticationException);

            this.consumerAccessBrokerMock.Setup(broker =>
                broker.CheckConsumerAccessAsync(
                    It.IsAny<ValidateAccessRequest>(),
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(returnedConsumerAccessResponse);

            // when
            ValueTask<ConsumerAccess> checkConsumerAccessTask =
                this.consumerAccessService.CheckConsumerAccessAsync(
                    inputValidateAccessRequest, TestContext.Current.CancellationToken);

            ConsumerAccessServiceDependencyException actualConsumerAccessServiceDependencyException =
                await Assert.ThrowsAsync<ConsumerAccessServiceDependencyException>(
                    testCode: checkConsumerAccessTask.AsTask);

            // then
            // A 401 with no problem details on it - empty, a gateway's HTML page, unreadable, or
            // not application/problem+json - never came from the access decision. The message
            // says what arrived instead, WWW-Authenticate challenge included, because that is
            // all a gateway's bare 401 has to say about why.
            actualConsumerAccessServiceDependencyException.Should()
                .BeEquivalentTo(expectedConsumerAccessServiceDependencyException);

            this.consumerAccessBrokerMock.Verify(broker =>
                broker.CheckConsumerAccessAsync(
                    inputValidateAccessRequest, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedConsumerAccessServiceDependencyException))),
                        Times.Once);

            this.consumerAccessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData("InsufficientPermissions", "The 'ConsumerAccess.Check' app role is required.")]
        [InlineData("AnErrorCodeThisServiceDoesNotKnow", "Something new went wrong.")]
        [InlineData(null, "A problem with no error code.")]
        [InlineData("InsufficientPermissions", null)]
        public async Task ShouldThrowCriticalDependencyExceptionOnCheckConsumerAccessIfPermissionIsMissingAndLogItAsync(
            string errorCode,
            string detail)
        {
            // given
            ValidateAccessRequest randomValidateAccessRequest = CreateRandomValidateAccessRequest();
            ValidateAccessRequest inputValidateAccessRequest = randomValidateAccessRequest;
            string randomCorrelationId = Guid.NewGuid().ToString("N");

            ConsumerAccessResponse returnedConsumerAccessResponse =
                CreateProblemResponse(
                    HttpStatusCode.Forbidden,
                    errorCode,
                    detail,
                    correlationId: randomCorrelationId);

            var failedConsumerAccessAuthorizationException =
                new FailedConsumerAccessAuthorizationException(
                    message:
                        "This service authenticated to Consumer Access Service but lacks the required permission " +
                        $"({errorCode ?? "no error code"}: {detail ?? "no detail"}). {GrantRoleAdvice}");

            AddExpectedResponseData(
                failedConsumerAccessAuthorizationException,
                HttpStatusCode.Forbidden,
                errorCode,
                detail,
                randomCorrelationId,
                contentType: "application/problem+json",
                wwwAuthenticate: string.Empty);

            var expectedConsumerAccessServiceDependencyException =
                new ConsumerAccessServiceDependencyException(
                    message: "ConsumerAccess dependency error occurred, contact support.",
                    innerException: failedConsumerAccessAuthorizationException);

            this.consumerAccessBrokerMock.Setup(broker =>
                broker.CheckConsumerAccessAsync(
                    It.IsAny<ValidateAccessRequest>(),
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(returnedConsumerAccessResponse);

            // when
            ValueTask<ConsumerAccess> checkConsumerAccessTask =
                this.consumerAccessService.CheckConsumerAccessAsync(
                    inputValidateAccessRequest, TestContext.Current.CancellationToken);

            ConsumerAccessServiceDependencyException actualConsumerAccessServiceDependencyException =
                await Assert.ThrowsAsync<ConsumerAccessServiceDependencyException>(
                    testCode: checkConsumerAccessTask.AsTask);

            // then
            // A 403 problem is the dependency refusing this service, not this consumer: the token
            // was good, the identity behind it is missing a role. Never an access decision, so
            // never audited as one.
            actualConsumerAccessServiceDependencyException.Should()
                .BeEquivalentTo(expectedConsumerAccessServiceDependencyException);

            this.consumerAccessBrokerMock.Verify(broker =>
                broker.CheckConsumerAccessAsync(
                    inputValidateAccessRequest, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedConsumerAccessServiceDependencyException))),
                        Times.Once);

            this.consumerAccessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData("application/json", "{}", "")]
        [InlineData("application/json", "null", "")]
        [InlineData("application/json", "not json", "")]
        [InlineData("text/html", "<html><body>403 Forbidden</body></html>", "")]
        [InlineData(null, "", "Bearer error=\"insufficient_scope\"")]
        [InlineData("application/problem+json", "not json", "")]
        public async Task
            ShouldThrowCriticalDependencyExceptionOnCheckConsumerAccessIfForbiddenWithoutDecisionAndLogItAsync(
                string contentType,
                string content,
                string wwwAuthenticate)
        {
            // given
            ValidateAccessRequest randomValidateAccessRequest = CreateRandomValidateAccessRequest();
            ValidateAccessRequest inputValidateAccessRequest = randomValidateAccessRequest;

            var returnedConsumerAccessResponse = new ConsumerAccessResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                ContentType = contentType,
                WwwAuthenticate = wwwAuthenticate,
                Content = content
            };

            string expectedContentType = string.IsNullOrEmpty(contentType) ? "none" : contentType;
            string expectedWwwAuthenticate = string.IsNullOrEmpty(wwwAuthenticate) ? "none" : wwwAuthenticate;

            var failedConsumerAccessAuthorizationException =
                new FailedConsumerAccessAuthorizationException(
                    message:
                        "Consumer Access Service answered 403 with neither an access decision nor a readable " +
                        $"problem details body (content type: {expectedContentType}; WWW-Authenticate: " +
                        $"{expectedWwwAuthenticate}), so it refused this service rather than the consumer - most " +
                        $"likely a gateway or its authorization. {GrantRoleAdvice}");

            AddExpectedResponseData(
                failedConsumerAccessAuthorizationException,
                HttpStatusCode.Forbidden,
                errorCode: null,
                detail: null,
                correlationId: null,
                contentType,
                wwwAuthenticate);

            var expectedConsumerAccessServiceDependencyException =
                new ConsumerAccessServiceDependencyException(
                    message: "ConsumerAccess dependency error occurred, contact support.",
                    innerException: failedConsumerAccessAuthorizationException);

            this.consumerAccessBrokerMock.Setup(broker =>
                broker.CheckConsumerAccessAsync(
                    It.IsAny<ValidateAccessRequest>(),
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(returnedConsumerAccessResponse);

            // when
            ValueTask<ConsumerAccess> checkConsumerAccessTask =
                this.consumerAccessService.CheckConsumerAccessAsync(
                    inputValidateAccessRequest, TestContext.Current.CancellationToken);

            ConsumerAccessServiceDependencyException actualConsumerAccessServiceDependencyException =
                await Assert.ThrowsAsync<ConsumerAccessServiceDependencyException>(
                    testCode: checkConsumerAccessTask.AsTask);

            // then
            // Only an application/json Access body - one that carries isAccessAllowed - makes a
            // 403 a refusal of the consumer. Anything else refused this service, and reading it as
            // a denial would audit a decision nobody made.
            actualConsumerAccessServiceDependencyException.Should()
                .BeEquivalentTo(expectedConsumerAccessServiceDependencyException);

            this.consumerAccessBrokerMock.Verify(broker =>
                broker.CheckConsumerAccessAsync(
                    inputValidateAccessRequest, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedConsumerAccessServiceDependencyException))),
                        Times.Once);

            this.consumerAccessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
