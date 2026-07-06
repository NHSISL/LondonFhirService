// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using ISL.Security.Client.Models.Foundations.Users;
using LondonFhirService.Core.Models.Brokers.ConsumerAccesses;
using LondonFhirService.Core.Models.Orchestrations.Accesses.Exceptions;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Orchestrations.Accesses
{
    public partial class AccessOrchestrationServiceTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ShouldThrowValidationExceptionOnValidateAccessIfNhsNumberIsInvalidAndLogItAsync(
            string invalidNhsNumber)
        {
            // given
            Guid randomCorrelationId = GetRandomGuid();

            var invalidArgumentAccessOrchestrationException =
                new InvalidArgumentAccessOrchestrationException(
                    message: "Invalid argument(s), please correct the errors and try again.");

            invalidArgumentAccessOrchestrationException.AddData(
                key: "nhsNumber",
                values: "Text is invalid");

            var expectedAccessOrchestrationValidationException =
                new AccessOrchestrationValidationException(
                    message: "Access orchestration validation error occurred, fix the errors and try again.",
                    innerException: invalidArgumentAccessOrchestrationException);

            // when
            ValueTask validateAccessTask =
                this.accessOrchestrationService.ValidateAccess(invalidNhsNumber, randomCorrelationId);

            AccessOrchestrationValidationException actualAccessOrchestrationValidationException =
                await Assert.ThrowsAsync<AccessOrchestrationValidationException>(
                    testCode: validateAccessTask.AsTask);

            // then
            actualAccessOrchestrationValidationException.Should()
                .BeEquivalentTo(expectedAccessOrchestrationValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedAccessOrchestrationValidationException))),
                        Times.Once);

            this.securityBrokerMock.VerifyNoOtherCalls();
            this.consumerAccessServiceMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.hashBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnValidateAccessIfCorrelationIdIsInvalidAndLogItAsync()
        {
            // given
            string randomNhsNumber = GetRandomString();
            Guid invalidCorrelationId = Guid.Empty;

            var invalidArgumentAccessOrchestrationException =
                new InvalidArgumentAccessOrchestrationException(
                    message: "Invalid argument(s), please correct the errors and try again.");

            invalidArgumentAccessOrchestrationException.AddData(
                key: "correlationId",
                values: "Id is invalid");

            var expectedAccessOrchestrationValidationException =
                new AccessOrchestrationValidationException(
                    message: "Access orchestration validation error occurred, fix the errors and try again.",
                    innerException: invalidArgumentAccessOrchestrationException);

            // when
            ValueTask validateAccessTask =
                this.accessOrchestrationService.ValidateAccess(randomNhsNumber, invalidCorrelationId);

            AccessOrchestrationValidationException actualAccessOrchestrationValidationException =
                await Assert.ThrowsAsync<AccessOrchestrationValidationException>(
                    testCode: validateAccessTask.AsTask);

            // then
            actualAccessOrchestrationValidationException.Should()
                .BeEquivalentTo(expectedAccessOrchestrationValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedAccessOrchestrationValidationException))),
                        Times.Once);

            this.securityBrokerMock.VerifyNoOtherCalls();
            this.consumerAccessServiceMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.hashBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnValidateAccessIfCurrentUserIsNullAndLogItAsync()
        {
            // given
            string randomNhsNumber = GetRandomString();
            Guid randomCorrelationId = GetRandomGuid();
            User nullUser = null;

            var unauthorizedAccessOrchestrationException =
                new UnauthorizedAccessOrchestrationException("Current consumer is not a valid consumer.");

            var expectedAccessOrchestrationValidationException =
                new AccessOrchestrationValidationException(
                    message: "Access orchestration validation error occurred, fix the errors and try again.",
                    innerException: unauthorizedAccessOrchestrationException);

            this.securityBrokerMock.Setup(broker =>
                broker.GetCurrentUserAsync())
                    .ReturnsAsync(nullUser);

            // when
            ValueTask validateAccessTask =
                this.accessOrchestrationService.ValidateAccess(randomNhsNumber, randomCorrelationId);

            AccessOrchestrationValidationException actualAccessOrchestrationValidationException =
                await Assert.ThrowsAsync<AccessOrchestrationValidationException>(
                    testCode: validateAccessTask.AsTask);

            // then
            actualAccessOrchestrationValidationException.Should()
                .BeEquivalentTo(expectedAccessOrchestrationValidationException);

            this.securityBrokerMock.Verify(broker =>
                broker.GetCurrentUserAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedAccessOrchestrationValidationException))),
                        Times.Once);

            this.securityBrokerMock.VerifyNoOtherCalls();
            this.consumerAccessServiceMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.hashBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnValidateAccessIfConsumerAccessIsNullAndLogItAsync()
        {
            // given
            string randomUserId = GetRandomString();
            User randomUser = CreateRandomUser(randomUserId);
            string randomNhsNumber = GetRandomString();
            Guid randomCorrelationId = GetRandomGuid();
            ConsumerAccess nullConsumerAccess = null;
            JsonSerializerOptions options = CreateJsonSerializerOptions();
            string currentUserJson = JsonSerializer.Serialize(randomUser, options);

            var unauthorizedAccessOrchestrationException =
                new UnauthorizedAccessOrchestrationException(
                    $"Current consumer with id `{randomUserId}` is not a valid consumer.");

            var expectedAccessOrchestrationValidationException =
                new AccessOrchestrationValidationException(
                    message: "Access orchestration validation error occurred, fix the errors and try again.",
                    innerException: unauthorizedAccessOrchestrationException);

            this.securityBrokerMock.Setup(broker =>
                broker.GetCurrentUserAsync())
                    .ReturnsAsync(randomUser);

            this.consumerAccessServiceMock.Setup(service =>
                service.CheckConsumerAccessAsync(
                    randomNhsNumber,
                    randomCorrelationId,
                    CancellationToken.None))
                        .ReturnsAsync(nullConsumerAccess);

            // when
            ValueTask validateAccessTask =
                this.accessOrchestrationService.ValidateAccess(randomNhsNumber, randomCorrelationId);

            AccessOrchestrationValidationException actualAccessOrchestrationValidationException =
                await Assert.ThrowsAsync<AccessOrchestrationValidationException>(
                    testCode: validateAccessTask.AsTask);

            // then
            actualAccessOrchestrationValidationException.Should()
                .BeEquivalentTo(expectedAccessOrchestrationValidationException);

            this.securityBrokerMock.Verify(broker =>
                broker.GetCurrentUserAsync(),
                    Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    "Access",
                    "Check Access Permissons",
                    currentUserJson,
                    null,
                    randomCorrelationId.ToString()),
                        Times.Once);

            this.consumerAccessServiceMock.Verify(service =>
                service.CheckConsumerAccessAsync(
                    randomNhsNumber,
                    randomCorrelationId,
                    CancellationToken.None),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedAccessOrchestrationValidationException))),
                        Times.Once);

            this.securityBrokerMock.VerifyNoOtherCalls();
            this.consumerAccessServiceMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.hashBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnValidateAccessIfAccessIsNotAllowedAndLogItAsync()
        {
            // given
            string randomUserId = GetRandomString();
            User randomUser = CreateRandomUser(randomUserId);
            string randomNhsNumber = GetRandomString();
            Guid randomCorrelationId = GetRandomGuid();
            JsonSerializerOptions options = CreateJsonSerializerOptions();
            string currentUserJson = JsonSerializer.Serialize(randomUser, options);

            var reasons = new List<AccessReason>
            {
                new AccessReason { Code = GetRandomString(), Message = GetRandomString() },
                new AccessReason { Code = GetRandomString(), Message = GetRandomString() }
            };

            ConsumerAccess notAllowedConsumerAccess = CreateRandomConsumerAccess(isAccessAllowed: false);
            notAllowedConsumerAccess.Reasons = reasons;

            var unauthorizedAccessOrchestrationException =
                new UnauthorizedAccessOrchestrationException(
                    $"Current consumer with id `{randomUserId}` is not allowed access.");

            foreach (AccessReason reason in reasons)
            {
                unauthorizedAccessOrchestrationException.Data.Add(
                    key: reason.Code,
                    value: reason.Message);
            }

            var expectedAccessOrchestrationValidationException =
                new AccessOrchestrationValidationException(
                    message: "Access orchestration validation error occurred, fix the errors and try again.",
                    innerException: unauthorizedAccessOrchestrationException);

            this.securityBrokerMock.Setup(broker =>
                broker.GetCurrentUserAsync())
                    .ReturnsAsync(randomUser);

            this.consumerAccessServiceMock.Setup(service =>
                service.CheckConsumerAccessAsync(
                    randomNhsNumber,
                    randomCorrelationId,
                    CancellationToken.None))
                        .ReturnsAsync(notAllowedConsumerAccess);

            // when
            ValueTask validateAccessTask =
                this.accessOrchestrationService.ValidateAccess(randomNhsNumber, randomCorrelationId);

            AccessOrchestrationValidationException actualAccessOrchestrationValidationException =
                await Assert.ThrowsAsync<AccessOrchestrationValidationException>(
                    testCode: validateAccessTask.AsTask);

            // then
            actualAccessOrchestrationValidationException.Should()
                .BeEquivalentTo(expectedAccessOrchestrationValidationException);

            this.securityBrokerMock.Verify(broker =>
                broker.GetCurrentUserAsync(),
                    Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    "Access",
                    "Check Access Permissons",
                    currentUserJson,
                    null,
                    randomCorrelationId.ToString()),
                        Times.Once);

            this.consumerAccessServiceMock.Verify(service =>
                service.CheckConsumerAccessAsync(
                    randomNhsNumber,
                    randomCorrelationId,
                    CancellationToken.None),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedAccessOrchestrationValidationException))),
                        Times.Once);

            this.securityBrokerMock.VerifyNoOtherCalls();
            this.consumerAccessServiceMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.hashBrokerMock.VerifyNoOtherCalls();
        }
    }
}
