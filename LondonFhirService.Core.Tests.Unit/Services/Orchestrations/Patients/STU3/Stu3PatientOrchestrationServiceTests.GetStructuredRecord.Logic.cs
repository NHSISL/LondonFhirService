// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Force.DeepCloner;
using Hl7.Fhir.Model;
using LondonFhirService.Core.Models.Foundations.Providers;
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
            DateTime? inputDateOfBirth = DateTime.Now;
            bool? inputDemographicsOnly = false;
            bool? inputActivePatientsOnly = true;
            CancellationToken cancellationToken = CancellationToken.None;
            List<Bundle> randomBundles = CreateRandomBundles();
            Bundle randomBundle = CreateRandomBundle();
            Bundle expectedBundle = randomBundle.DeepClone();
            Guid correlationId = Guid.NewGuid();
            Provider randomPrimaryProvider = CreateRandomPrimaryProvider();
            Provider randomActiveProvider = CreateRandomActiveProvider();
            Provider randomInactiveProvider = CreateRandomInactiveProvider();
            string auditType = "STU3-Patient-GetStructuredRecord";

            string message =
                $"Parameters:  {{ nhsNumber = \"{inputNhsNumber}\", dateOfBirth = \"{inputDateOfBirth}\", " +
                $"demographicsOnly = \"{inputDemographicsOnly}\", " +
                $"includeInactivePatients = \"{inputActivePatientsOnly}\" }}";

            IQueryable<Provider> allProviders = new List<Provider>
            {
                randomInactiveProvider,
                randomActiveProvider,
                randomPrimaryProvider
            }.AsQueryable();

            this.providerServiceMock.Setup(service =>
                service.RetrieveAllProvidersAsync())
                    .ReturnsAsync(allProviders);

            List<string> activeProviderNames = new List<string>
            {
                randomPrimaryProvider.FullyQualifiedName,
                randomActiveProvider.FullyQualifiedName
            };

            this.patientServiceMock.Setup(service =>
                service.GetStructuredRecordAsync(
                    activeProviderNames,
                    correlationId,
                    inputNhsNumber,
                    inputDateOfBirth,
                    inputDemographicsOnly,
                    inputActivePatientsOnly,
                    cancellationToken))
                    .ReturnsAsync(randomBundles);

            this.fhirReconciliationServiceMock.Setup(service =>
                service.ReconcileAsync(
                    randomBundles,
                    randomPrimaryProvider.FullyQualifiedName))
                    .ReturnsAsync(expectedBundle);

            // when
            Bundle actualBundle = await this.patientOrchestrationService.GetStructuredRecordAsync(
                correlationId,
                inputNhsNumber,
                inputDateOfBirth,
                inputDemographicsOnly,
                inputActivePatientsOnly,
                cancellationToken);

            // then
            actualBundle.Should().BeEquivalentTo(expectedBundle);

            this.providerServiceMock.Verify(service =>
                service.RetrieveAllProvidersAsync(),
                    Times.Once);

            this.patientServiceMock.Verify(service =>
                service.GetStructuredRecordAsync(
                    activeProviderNames,
                    correlationId,
                    inputNhsNumber,
                    inputDateOfBirth,
                    inputDemographicsOnly,
                    inputActivePatientsOnly,
                    cancellationToken),
                    Times.Once);

            this.fhirReconciliationServiceMock.Verify(service =>
                service.ReconcileAsync(
                    randomBundles,
                    randomPrimaryProvider.FullyQualifiedName),
                    Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    "Orchestration Service Request Submitted",
                    message,
                    string.Empty,
                    correlationId.ToString()),
                        Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    "Retrieve active providers and execute request",
                    message,
                    string.Empty,
                    correlationId.ToString()),
                        Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    "Reconcile bundles",
                    message,
                    string.Empty,
                    correlationId.ToString()),
                        Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    "Orchestration Service Request Completed",
                    message,
                    string.Empty,
                    correlationId.ToString()),
                        Times.Once);

            this.providerServiceMock.VerifyNoOtherCalls();
            this.patientServiceMock.VerifyNoOtherCalls();
            this.fhirReconciliationServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
        }
    }
}
