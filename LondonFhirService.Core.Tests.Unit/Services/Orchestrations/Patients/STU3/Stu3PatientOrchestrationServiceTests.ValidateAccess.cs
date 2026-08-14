// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using ISL.Security.Client.Models.Foundations.Users;
using LondonFhirService.Core.Models.Brokers.ConsumerAccesses;
using LondonFhirService.Core.Models.Orchestrations.Accesses;
using LondonFhirService.Core.Models.Orchestrations.Patients.Exceptions;
using LondonFhirService.Core.Services.Orchestrations.Patients.STU3;
using Moq;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Core.Tests.Unit.Services.Orchestrations.Patients.STU3
{
    public partial class Stu3PatientOrchestrationServiceTests
    {
        [Fact]
        public async Task ShouldValidateAccessWhenAccessIsAllowedAsync()
        {
            // given
            string userId = GetRandomString();
            User randomUser = CreateRandomUser(userId);
            User outputUser = randomUser;
            Guid correlationId = Guid.NewGuid();
            string randomNhsNumber = GetRandomStringWithLengthOf(10);
            string inputNhsNumber = randomNhsNumber;
            ConsumerAccess randomConsumerAccess = CreateRandomConsumerAccess(isAccessAllowed: true);
            ConsumerAccess returnedConsumerAccess = randomConsumerAccess;
            string auditType = "STU3-Patient-GetStructuredRecordSerialised";
            string message = $"Parameters:  {{ nhsNumber = \"{inputNhsNumber}\" }}";

            JsonSerializerOptions options = new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };

            string currentUserJson = JsonSerializer.Serialize(outputUser, options);

            this.securityBrokerMock.Setup(broker =>
                broker.GetCurrentUserAsync())
                    .ReturnsAsync(outputUser);

            this.consumerAccessServiceMock.Setup(service =>
                service.CheckConsumerAccessAsync(
                    It.Is(SameValidateAccessRequestAs(userId, inputNhsNumber, correlationId)),
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(returnedConsumerAccess);

            // when
            await this.patientOrchestrationService.ValidateAccess(inputNhsNumber, correlationId);

            // then
            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    "Check Access Permissions",
                    message,
                    null,
                    correlationId.ToString()),
                        Times.Once);

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

            this.consumerAccessServiceMock.Verify(service =>
                service.CheckConsumerAccessAsync(
                    It.Is(SameValidateAccessRequestAs(userId, inputNhsNumber, correlationId)),
                    default),
                        Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    "Access",
                    "Access Allowed",

                    It.Is<string>(auditMessage => auditMessage.StartsWith(
                        $"{userId} is allowed to access patient with " +
                        $"NHS number {inputNhsNumber} via org codes: " +
                        $"{string.Join(", ", returnedConsumerAccess.AllowedViaOrganisations)}  " +
                        $"CorrelationId: {correlationId.ToString()}")),

                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.consumerAccessServiceMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldSkipValidateAccessWhenCheckAccessPermissionsIsOffAsync()
        {
            // given
            Guid correlationId = Guid.NewGuid();
            string randomNhsNumber = GetRandomStringWithLengthOf(10);
            string inputNhsNumber = randomNhsNumber;
            string auditType = "STU3-Patient-GetStructuredRecordSerialised";
            string message = $"Parameters:  {{ nhsNumber = \"{inputNhsNumber}\" }}";

            Stu3PatientOrchestrationService orchestrationService =
                CreateOrchestrationService(new AccessConfigurations { CheckAccessPermissions = false });

            // when
            await orchestrationService.ValidateAccess(inputNhsNumber, correlationId);

            // then
            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    "Access permission check skipped due to configuration (CheckAccessPermissions = false)",
                    message,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.securityBrokerMock.Verify(broker =>
                broker.GetCurrentUserAsync(),
                    Times.Never);

            this.consumerAccessServiceMock.Verify(service =>
                service.CheckConsumerAccessAsync(
                    It.IsAny<ValidateAccessRequest>(),
                    It.IsAny<CancellationToken>()),
                        Times.Never);

            this.consumerAccessServiceMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ShouldThrowValidationExceptionOnValidateAccessIfArgumentsAreInvalidAndLogItAsync(
            string invalidText)
        {
            // given
            string invalidNhsNumber = invalidText;
            Guid correlationId = Guid.Empty;

            var invalidArgumentPatientOrchestrationException =
                new InvalidArgumentPatientOrchestrationException(
                    message: "Invalid argument(s), please correct the errors and try again.");

            invalidArgumentPatientOrchestrationException.AddData(
                key: "NhsNumber",
                values: "Text is required");

            invalidArgumentPatientOrchestrationException.AddData(
                key: "CorrelationId",
                values: "Id is required");

            var expectedPatientOrchestrationValidationException =
                new PatientOrchestrationValidationException(
                    message: "Patient orchestration validation error occurred, please try again.",
                    innerException: invalidArgumentPatientOrchestrationException);

            // when
            ValueTask validateAccessTask =
                this.patientOrchestrationService.ValidateAccess(invalidNhsNumber, correlationId);

            PatientOrchestrationValidationException actualPatientOrchestrationValidationException =
                await Assert.ThrowsAsync<PatientOrchestrationValidationException>(
                    validateAccessTask.AsTask);

            // then
            actualPatientOrchestrationValidationException.Should()
                .BeEquivalentTo(expectedPatientOrchestrationValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedPatientOrchestrationValidationException))),
                        Times.Once);

            this.securityBrokerMock.Verify(broker =>
                broker.GetCurrentUserAsync(),
                    Times.Never);

            this.consumerAccessServiceMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnValidateAccessIfNoCurrentUserAndLogItAsync()
        {
            // given
            User noUser = null;
            Guid correlationId = Guid.NewGuid();
            string randomNhsNumber = GetRandomStringWithLengthOf(10);
            string inputNhsNumber = randomNhsNumber;
            string auditType = "STU3-Patient-GetStructuredRecordSerialised";
            string message = $"Parameters:  {{ nhsNumber = \"{inputNhsNumber}\" }}";

            JsonSerializerOptions options = new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };

            string currentUserJson = JsonSerializer.Serialize(noUser, options);

            var unauthorizedPatientOrchestrationException =
                new UnauthorizedPatientOrchestrationException("Current consumer is not a valid consumer.");

            var expectedPatientOrchestrationValidationException =
                new PatientOrchestrationValidationException(
                    message: "Patient orchestration validation error occurred, please try again.",
                    innerException: unauthorizedPatientOrchestrationException);

            this.securityBrokerMock.Setup(broker =>
                broker.GetCurrentUserAsync())
                    .ReturnsAsync(noUser);

            // when
            ValueTask validateAccessTask =
                this.patientOrchestrationService.ValidateAccess(inputNhsNumber, correlationId);

            PatientOrchestrationValidationException actualPatientOrchestrationValidationException =
                await Assert.ThrowsAsync<PatientOrchestrationValidationException>(
                    validateAccessTask.AsTask);

            // then
            actualPatientOrchestrationValidationException.Should()
                .BeEquivalentTo(expectedPatientOrchestrationValidationException);

            this.securityBrokerMock.Verify(broker =>
                broker.GetCurrentUserAsync(),
                    Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    "Check Access Permissions",
                    message,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    "Access",
                    "Check Access Permissons",
                    currentUserJson,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedPatientOrchestrationValidationException))),
                        Times.Once);

            this.consumerAccessServiceMock.Verify(service =>
                service.CheckConsumerAccessAsync(
                    It.IsAny<ValidateAccessRequest>(),
                    It.IsAny<CancellationToken>()),
                        Times.Never);

            this.consumerAccessServiceMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnValidateAccessIfAccessIsNotAllowedAndLogItAsync()
        {
            // given
            string userId = GetRandomString();
            User randomUser = CreateRandomUser(userId);
            User outputUser = randomUser;
            Guid correlationId = Guid.NewGuid();
            string randomNhsNumber = GetRandomStringWithLengthOf(10);
            string inputNhsNumber = randomNhsNumber;
            ConsumerAccess randomConsumerAccess = CreateRandomConsumerAccess(isAccessAllowed: false);
            ConsumerAccess returnedConsumerAccess = randomConsumerAccess;
            string auditType = "STU3-Patient-GetStructuredRecordSerialised";
            string message = $"Parameters:  {{ nhsNumber = \"{inputNhsNumber}\" }}";

            string reasons = string.Join(", ", returnedConsumerAccess.Reasons
                .Select(reason => $"{reason.Code}: {reason.Message}"));

            JsonSerializerOptions options = new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };

            string currentUserJson = JsonSerializer.Serialize(outputUser, options);

            var forbiddenPatientOrchestrationException =
                new ForbiddenPatientOrchestrationException(
                    "Current consumer is not permitted to access this patient.  " +
                    $"CorrelationId: {correlationId.ToString()}");

            var expectedPatientOrchestrationValidationException =
                new PatientOrchestrationValidationException(
                    message: "Patient orchestration validation error occurred, please try again.",
                    innerException: forbiddenPatientOrchestrationException);

            this.securityBrokerMock.Setup(broker =>
                broker.GetCurrentUserAsync())
                    .ReturnsAsync(outputUser);

            this.consumerAccessServiceMock.Setup(service =>
                service.CheckConsumerAccessAsync(
                    It.Is(SameValidateAccessRequestAs(userId, inputNhsNumber, correlationId)),
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(returnedConsumerAccess);

            // when
            ValueTask validateAccessTask =
                this.patientOrchestrationService.ValidateAccess(inputNhsNumber, correlationId);

            PatientOrchestrationValidationException actualPatientOrchestrationValidationException =
                await Assert.ThrowsAsync<PatientOrchestrationValidationException>(
                    validateAccessTask.AsTask);

            // then
            actualPatientOrchestrationValidationException.Should()
                .BeEquivalentTo(expectedPatientOrchestrationValidationException);

            this.securityBrokerMock.Verify(broker =>
                broker.GetCurrentUserAsync(),
                    Times.Once);

            this.consumerAccessServiceMock.Verify(service =>
                service.CheckConsumerAccessAsync(
                    It.Is(SameValidateAccessRequestAs(userId, inputNhsNumber, correlationId)),
                    default),
                        Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    "Check Access Permissions",
                    message,
                    null,
                    correlationId.ToString()),
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

                    It.Is<string>(auditMessage => auditMessage.StartsWith(
                        $"Access was denied as consumer with id {userId} is not permitted " +
                        $"to access patient with NHS number {inputNhsNumber}. Reasons: {reasons}  " +
                        $"CorrelationId: {correlationId.ToString()}")),

                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedPatientOrchestrationValidationException))),
                        Times.Once);

            this.consumerAccessServiceMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
