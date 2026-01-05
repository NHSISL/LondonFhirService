// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
using ISL.Security.Client.Models.Foundations.Users;
using LondonFhirService.Core.Models.Foundations.Consumers;
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
        public async Task ShouldThrowValidationExceptionOnValidateAccess(string invalidText)
        {
            // given
            Guid correlationId = Guid.Empty;

            var invalidArgumentAccessOrchestrationException =
                new InvalidArgumentAccessOrchestrationException(
                    message: "Invalid argument access orchestration exception, " +
                        "please correct the errors and try again.");

            invalidArgumentAccessOrchestrationException.AddData(
                key: "nhsNumber",
                values: "Text is invalid");

            invalidArgumentAccessOrchestrationException.AddData(
                key: "correlationId",
                values: "Id is invalid");

            var expectedAccessOrchestrationValidationException =
                new AccessOrchestrationValidationException(
                    message: "Access orchestration validation error occurred, " +
                        "fix the errors and try again.",
                    innerException: invalidArgumentAccessOrchestrationException);

            this.securityBrokerMock.Setup(broker =>
                broker.GetCurrentUserAsync())
                    .ThrowsAsync(invalidArgumentAccessOrchestrationException);

            // when
            ValueTask validateAccessTask = accessOrchestrationService.ValidateAccess(invalidText, correlationId);

            AccessOrchestrationValidationException actualAccessOrchestrationValidationException =
                await Assert.ThrowsAsync<AccessOrchestrationValidationException>(
                    testCode: validateAccessTask.AsTask);

            // then
            actualAccessOrchestrationValidationException
                .Should().BeEquivalentTo(expectedAccessOrchestrationValidationException);

            this.loggingBrokerMock.Verify(broker =>
               broker.LogErrorAsync(It.Is(SameExceptionAs(
                   expectedAccessOrchestrationValidationException))),
                       Times.Once);

            this.consumerServiceMock.VerifyNoOtherCalls();
            this.consumerAccessServiceMock.VerifyNoOtherCalls();
            this.pdsDataServiceMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowUnauthorisedExceptionOnValidateAccessWhenNoMatchingConsumer()
        {
            // given
            string userId = GetRandomString();
            User randomUser = CreateRandomUser(userId);
            User outputUser = randomUser;
            Consumer randomConsumer = CreateRandomConsumer();
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            DateTimeOffset validActiveFromDate = randomDateTimeOffset.AddDays(-2);
            DateTimeOffset validActiveToDate = randomDateTimeOffset.AddDays(2);
            randomConsumer.ActiveFrom = validActiveFromDate;
            randomConsumer.ActiveTo = validActiveToDate;
            Consumer inputConsumer = randomConsumer.DeepClone();
            Guid correlationId = Guid.NewGuid();

            JsonSerializerOptions options = new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };

            string currentUserJson = JsonSerializer.Serialize(outputUser, options);

            IQueryable<Consumer> storageConsumers =
                new List<Consumer> { inputConsumer }.AsQueryable();

            Guid randomGuid = Guid.NewGuid();
            string randomNhsNumber = GetRandomStringWithLength(5);
            string inputNhsNumber = randomNhsNumber;

            var unauthorizedAccessOrchestrationException =
                new UnauthorizedAccessOrchestrationException($"Current consumer with id `{outputUser.UserId}` is not a valid consumer.");

            var expectedAccessOrchestrationValidationException =
                new AccessOrchestrationValidationException(
                    message: "Access orchestration validation error occurred, " +
                        "fix the errors and try again.",
                    innerException: unauthorizedAccessOrchestrationException);

            this.securityBrokerMock.Setup(broker =>
                broker.GetCurrentUserAsync())
                    .ReturnsAsync(outputUser);

            this.consumerServiceMock.Setup(service =>
                service.RetrieveAllConsumersAsync())
                    .ReturnsAsync(storageConsumers);

            // when
            ValueTask validateAccessTask = accessOrchestrationService.ValidateAccess(inputNhsNumber, correlationId);

            AccessOrchestrationValidationException actualAccessOrchestrationValidationException =
                await Assert.ThrowsAsync<AccessOrchestrationValidationException>(
                    testCode: validateAccessTask.AsTask);

            // then
            actualAccessOrchestrationValidationException
                .Should().BeEquivalentTo(expectedAccessOrchestrationValidationException);

            this.securityBrokerMock.Verify(broker =>
                broker.GetCurrentUserAsync(),
                    Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    "Access",
                    "Check Access Permissons",
                    currentUserJson,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.consumerServiceMock.Verify(service =>
                service.RetrieveAllConsumersAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
               broker.LogErrorAsync(It.Is(SameExceptionAs(
                   expectedAccessOrchestrationValidationException))),
                       Times.Once);

            this.consumerServiceMock.VerifyNoOtherCalls();
            this.consumerAccessServiceMock.VerifyNoOtherCalls();
            this.pdsDataServiceMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowForbiddenExceptionOnValidateAccessWhenInactiveConsumer()
        {
            // given
            string userId = GetRandomString();
            User randomUser = CreateRandomUser(userId);
            User outputUser = randomUser;
            Consumer randomConsumer = CreateRandomConsumer(userId);
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            DateTimeOffset validActiveFromDate = randomDateTimeOffset.AddDays(-2);
            DateTimeOffset invalidActiveToDate = randomDateTimeOffset.AddDays(-2);
            randomConsumer.ActiveFrom = validActiveFromDate;
            randomConsumer.ActiveTo = invalidActiveToDate;
            Consumer inputConsumer = randomConsumer.DeepClone();
            Guid correlationId = Guid.NewGuid();

            JsonSerializerOptions options = new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };

            string currentUserJson = JsonSerializer.Serialize(outputUser, options);

            IQueryable<Consumer> storageConsumers =
                new List<Consumer> { inputConsumer }.AsQueryable();

            Guid randomGuid = Guid.NewGuid();
            string randomNhsNumber = GetRandomStringWithLength(5);
            string inputNhsNumber = randomNhsNumber;

            var forbiddenAccessOrchestrationException =
                new ForbiddenAccessOrchestrationException(
                   "Current consumer is not active or does not have a valid access window.");

            var expectedAccessOrchestrationValidationException =
                new AccessOrchestrationValidationException(
                    message: "Access orchestration validation error occurred, " +
                        "fix the errors and try again.",
                    innerException: forbiddenAccessOrchestrationException);

            this.securityBrokerMock.Setup(broker =>
                broker.GetCurrentUserAsync())
                    .ReturnsAsync(outputUser);

            this.consumerServiceMock.Setup(service =>
                service.RetrieveAllConsumersAsync())
                    .ReturnsAsync(storageConsumers);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            // when
            ValueTask validateAccessTask = accessOrchestrationService.ValidateAccess(inputNhsNumber, correlationId);

            AccessOrchestrationValidationException actualAccessOrchestrationValidationException =
                await Assert.ThrowsAsync<AccessOrchestrationValidationException>(
                    testCode: validateAccessTask.AsTask);

            // then
            actualAccessOrchestrationValidationException
                .Should().BeEquivalentTo(expectedAccessOrchestrationValidationException);

            this.securityBrokerMock.Verify(broker =>
                broker.GetCurrentUserAsync(),
                    Times.Once);

            this.consumerServiceMock.Verify(service =>
                service.RetrieveAllConsumersAsync(),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    "Access",
                    "Check Access Permissons",
                    currentUserJson,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    "Access",
                    "Access Forbidden",

                    $"Access was forbidden as consumer with id {inputConsumer.Id} " +
                        $"is not active / does not have valid access window " +
                            $"(ActiveFrom: {inputConsumer.ActiveFrom}, ActiveTo: {inputConsumer.ActiveTo})",
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
              broker.LogErrorAsync(It.Is(SameExceptionAs(
                  expectedAccessOrchestrationValidationException))),
                      Times.Once);

            this.consumerAccessServiceMock.Verify(service =>
                service.RetrieveAllActiveOrganisationsUserHasAccessToAsync(inputConsumer.Id),
                    Times.Never);

            this.pdsDataServiceMock.Verify(service =>
                service.OrganisationsHaveAccessToThisPatient(inputNhsNumber, It.IsAny<List<string>>()),
                    Times.Never);

            this.consumerServiceMock.VerifyNoOtherCalls();
            this.consumerAccessServiceMock.VerifyNoOtherCalls();
            this.pdsDataServiceMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowForbiddenExceptionOnValidateAccessWhenOrganisationsNoAccessToPatient()
        {
            // given
            string userId = GetRandomString();
            User randomUser = CreateRandomUser(userId);
            User outputUser = randomUser;
            Consumer randomConsumer = CreateRandomConsumer(userId);
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            DateTimeOffset validActiveFromDate = randomDateTimeOffset.AddDays(-2);
            DateTimeOffset validActiveToDate = randomDateTimeOffset.AddDays(2);
            randomConsumer.ActiveFrom = validActiveFromDate;
            randomConsumer.ActiveTo = validActiveToDate;
            Consumer inputConsumer = randomConsumer.DeepClone();
            Guid correlationId = Guid.NewGuid();

            JsonSerializerOptions options = new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };

            string currentUserJson = JsonSerializer.Serialize(outputUser, options);

            IQueryable<Consumer> storageConsumers =
                new List<Consumer> { inputConsumer }.AsQueryable();

            Guid randomGuid = Guid.NewGuid();
            string randomNhsNumber = GetRandomStringWithLength(5);
            string inputNhsNumber = randomNhsNumber;

            string userOrganisation = GetRandomStringWithLength(5);

            List<string> userOrganisations =
                new List<string> { userOrganisation };

            var forbiddenAccessOrchestrationException =
                new ForbiddenAccessOrchestrationException(
                   "None of the organisations the consumer has access to are permitted to access this patient.");

            var expectedAccessOrchestrationValidationException =
                new AccessOrchestrationValidationException(
                    message: "Access orchestration validation error occurred, " +
                        "fix the errors and try again.",
                    innerException: forbiddenAccessOrchestrationException);

            this.securityBrokerMock.Setup(broker =>
                broker.GetCurrentUserAsync())
                    .ReturnsAsync(outputUser);

            this.consumerServiceMock.Setup(service =>
                service.RetrieveAllConsumersAsync())
                    .ReturnsAsync(storageConsumers);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.consumerAccessServiceMock.Setup(service =>
                service.RetrieveAllActiveOrganisationsUserHasAccessToAsync(inputConsumer.Id))
                    .ReturnsAsync(userOrganisations);

            this.pdsDataServiceMock.Setup(service =>
                service.OrganisationsHaveAccessToThisPatient(inputNhsNumber, userOrganisations))
                    .ReturnsAsync(false);

            // when
            ValueTask validateAccessTask = accessOrchestrationService.ValidateAccess(inputNhsNumber, correlationId);

            AccessOrchestrationValidationException actualAccessOrchestrationValidationException =
                await Assert.ThrowsAsync<AccessOrchestrationValidationException>(
                    testCode: validateAccessTask.AsTask);

            // then
            actualAccessOrchestrationValidationException
                .Should().BeEquivalentTo(expectedAccessOrchestrationValidationException);

            this.securityBrokerMock.Verify(broker =>
                broker.GetCurrentUserAsync(),
                    Times.Once);

            this.consumerServiceMock.Verify(service =>
                service.RetrieveAllConsumersAsync(),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.consumerAccessServiceMock.Verify(service =>
                service.RetrieveAllActiveOrganisationsUserHasAccessToAsync(inputConsumer.Id),
                    Times.Once);

            this.pdsDataServiceMock.Verify(service =>
                service.OrganisationsHaveAccessToThisPatient(inputNhsNumber, userOrganisations),
                    Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    "Access",
                    "Check Access Permissons",
                    currentUserJson,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    "Access",
                    "Access Forbidden",
                    $"Access was denied as none of the organisations the consumer with id " +
                        $"{inputConsumer.Id} has access to are permitted to access patient with " +
                        $"NHS number {inputNhsNumber}.",
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
              broker.LogErrorAsync(It.Is(SameExceptionAs(
                  expectedAccessOrchestrationValidationException))),
                      Times.Once);

            this.consumerServiceMock.VerifyNoOtherCalls();
            this.consumerAccessServiceMock.VerifyNoOtherCalls();
            this.pdsDataServiceMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
