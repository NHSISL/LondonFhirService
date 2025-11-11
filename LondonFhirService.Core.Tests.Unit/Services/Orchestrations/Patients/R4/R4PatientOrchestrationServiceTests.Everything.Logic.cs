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

namespace LondonFhirService.Core.Tests.Unit.Services.Orchestrations.Patients.R4
{
    public partial class R4PatientOrchestrationServiceTests
    {
        [Fact]
        public async Task ShouldCallEverythingAsync()
        {
            // given
            string randomId = GetRandomString();
            string inputId = randomId;
            DateTimeOffset? inputStart = GetRandomDateTimeOffset();
            DateTimeOffset? inputEnd = GetRandomDateTimeOffset();
            string inputTypeFilter = GetRandomString();
            DateTimeOffset? inputSince = GetRandomDateTimeOffset();
            int? inputCount = GetRandomNumber();
            CancellationToken cancellationToken = CancellationToken.None;
            List<Bundle> randomBundles = CreateRandomBundles();
            Bundle randomBundle = CreateRandomBundle();
            Bundle expectedBundle = randomBundle.DeepClone();

            Provider randomPrimaryProvider = CreateRandomPrimaryProvider();
            Provider randomActiveProvider = CreateRandomActiveProvider();
            Provider randomInactiveProvider = CreateRandomInactiveProvider();

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
                randomPrimaryProvider.Name,
                randomActiveProvider.Name
            };

            this.patientServiceMock.Setup(service =>
                service.Everything(
                    activeProviderNames,
                    inputId,
                    inputStart,
                    inputEnd,
                    inputTypeFilter,
                    inputSince,
                    inputCount,
                    cancellationToken))
                    .ReturnsAsync(randomBundles);

            this.fhirReconciliationServiceMock.Setup(service =>
                service.Reconcile(
                    randomBundles,
                    randomPrimaryProvider.Name))
                    .ReturnsAsync(expectedBundle);

            // when
            Bundle actualBundle = await this.patientOrchestrationService.Everything(
                inputId,
                inputStart,
                inputEnd,
                inputTypeFilter,
                inputSince,
                inputCount,
                cancellationToken);

            // then
            actualBundle.Should().BeEquivalentTo(expectedBundle);

            this.providerServiceMock.Verify(service =>
                service.RetrieveAllProvidersAsync(),
                    Times.Once);

            this.patientServiceMock.Verify(service =>
                service.Everything(
                    activeProviderNames,
                    inputId,
                    inputStart,
                    inputEnd,
                    inputTypeFilter,
                    inputSince,
                    inputCount,
                    cancellationToken),
                    Times.Once);

            this.fhirReconciliationServiceMock.Verify(service =>
                service.Reconcile(
                    randomBundles,
                    randomPrimaryProvider.Name),
                    Times.Once);
        }
    }
}
