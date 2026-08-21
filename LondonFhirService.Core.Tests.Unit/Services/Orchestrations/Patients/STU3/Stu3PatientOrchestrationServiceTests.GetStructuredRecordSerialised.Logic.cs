// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using FluentAssertions;
using ISL.Security.Client.Models.Foundations.Users;
using LondonFhirService.Core.Models.Brokers.ConsumerAccesses;
using LondonFhirService.Core.Models.Foundations.Providers;
using LondonFhirService.Core.Models.Orchestrations.Patients;
using Moq;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Core.Tests.Unit.Services.Orchestrations.Patients.STU3
{
    public partial class Stu3PatientOrchestrationServiceTests
    {
        [Fact]
        public async Task ShouldCallGetStructuredRecordAsync()
        {
            // given
            string randomId = GetRandomString();
            string inputNhsNumber = randomId;
            string inputDateOfBirth = DateTime.Now.ToString("yyyy-MM-dd");
            bool? inputDemographicsOnly = false;
            bool? inputActivePatientsOnly = true;
            CancellationToken cancellationToken = CancellationToken.None;
            List<(string Provider, string Json)> randomBundles = CreateRandomBundles();
            List<(string Provider, string Json)> expectedBundles = randomBundles;
            Guid correlationId = Guid.NewGuid();
            Provider randomPrimaryProvider = CreateRandomPrimaryProvider();
            Provider randomActiveProvider = CreateRandomActiveProvider();
            Provider randomInactiveProvider = CreateRandomInactiveProvider();
            string auditType = "STU3-Patient-GetStructuredRecordSerialised";
            string userId = GetRandomString();
            User randomUser = CreateRandomUser(userId);
            User outputUser = randomUser;
            ConsumerAccess randomConsumerAccess = CreateRandomConsumerAccess(isAccessAllowed: true);
            ConsumerAccess returnedConsumerAccess = randomConsumerAccess;

            string message =
                $"Parameters:  {{ nhsNumber = \"{inputNhsNumber}\", dateOfBirth = \"{inputDateOfBirth}\", " +
                $"demographicsOnly = \"{inputDemographicsOnly}\", " +
                $"includeInactivePatients = \"{inputActivePatientsOnly}\" }}";

            string accessMessage = $"Parameters:  {{ nhsNumber = \"{inputNhsNumber}\" }}";

            JsonSerializerOptions options = new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };

            string currentUserJson = JsonSerializer.Serialize(outputUser, options);

            IQueryable<Provider> allProviders = new List<Provider>
            {
                randomInactiveProvider,
                randomActiveProvider,
                randomPrimaryProvider
            }.AsQueryable();

            this.securityBrokerMock.Setup(broker =>
                broker.GetCurrentUserAsync())
                    .ReturnsAsync(outputUser);

            this.consumerAccessServiceMock.Setup(service =>
                service.CheckConsumerAccessAsync(
                    It.Is(SameValidateAccessRequestAs(userId, inputNhsNumber, correlationId)),
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(returnedConsumerAccess);

            this.providerServiceMock.Setup(service =>
                service.RetrieveAllProvidersAsync())
                    .ReturnsAsync(allProviders);

            List<Provider> activeProviders = new List<Provider>
            {
                randomPrimaryProvider,
                randomActiveProvider
            };

            this.patientServiceMock.Setup(service =>
                service.GetStructuredRecordSerialisedAsync(
                    activeProviders,
                    correlationId,
                    inputNhsNumber,
                    inputDateOfBirth,
                    inputDemographicsOnly,
                    inputActivePatientsOnly,
                    It.IsAny<Guid?>(),
                    cancellationToken))
                    .ReturnsAsync(randomBundles);

            // when
            StructuredRecordsResponse actualResponse =
                await this.patientOrchestrationService.GetStructuredRecordSerialisedAsync(
                    correlationId,
                    inputNhsNumber,
                    inputDateOfBirth,
                    inputDemographicsOnly,
                    inputActivePatientsOnly,
                    It.IsAny<Guid?>(),
                    cancellationToken);

            // then
            actualResponse.PrimaryProvider.Should().BeEquivalentTo(randomPrimaryProvider);
            actualResponse.Bundles.Should().BeEquivalentTo(expectedBundles);

            this.securityBrokerMock.Verify(broker =>
                broker.GetCurrentUserAsync(),
                    Times.Once);

            this.consumerAccessServiceMock.Verify(service =>
                service.CheckConsumerAccessAsync(
                    It.Is(SameValidateAccessRequestAs(userId, inputNhsNumber, correlationId)),
                    default),
                        Times.Once);

            this.providerServiceMock.Verify(service =>
                service.RetrieveAllProvidersAsync(),
                    Times.Once);

            this.patientServiceMock.Verify(service =>
                service.GetStructuredRecordSerialisedAsync(
                    activeProviders,
                    correlationId,
                    inputNhsNumber,
                    inputDateOfBirth,
                    inputDemographicsOnly,
                    inputActivePatientsOnly,
                    It.IsAny<Guid?>(),
                    cancellationToken),
                    Times.Once);

            this.auditAndMetricBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    "Orchestration Service Request Submitted",
                    message,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.auditAndMetricBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    "Check Access Permissions",
                    accessMessage,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.auditAndMetricBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    "Access",
                    "Check Access Permissons",
                    currentUserJson,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.auditAndMetricBrokerMock.Verify(broker =>
                broker.RecordAuditAsync(
                    "Access",
                    "Access Allowed",

                    It.Is<string>(auditMessage => auditMessage.StartsWith(
                        $"{userId} is allowed to access patient with " +
                        $"NHS number {inputNhsNumber} via org codes: " +
                        $"{string.Join(", ", returnedConsumerAccess.AllowedViaOrganisations)}  " +
                        $"CorrelationId: {correlationId.ToString()}")),

                    null,
                    correlationId.ToString(),
                    "Information",
                    default),
                        Times.Once);

            this.auditAndMetricBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    "Retrieve active providers and execute request",
                    message,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.auditAndMetricBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    It.Is<string>(title => title.StartsWith("Orchestration Service Request Completed")),
                    message,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            AcceptMetricSpans();
            this.providerServiceMock.VerifyNoOtherCalls();
            this.patientServiceMock.VerifyNoOtherCalls();
            this.consumerAccessServiceMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.auditAndMetricBrokerMock.VerifyNoOtherCalls();
        }
    }
}
