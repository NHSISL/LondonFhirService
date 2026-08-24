// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.Providers;
using Moq;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Core.Tests.Unit.Services.Orchestrations.FhirReconciliations.STU3
{
    public partial class Stu3FhirReconciliationServiceTests
    {
        [Fact]
        public async Task ShouldReturnThePrimaryProvidersBundleWhenItIsNotFirstOnReconcileSerialisedAsync()
        {
            // given
            // The primary is placed last on purpose. Selection used to be positional, so this
            // arrangement returned the secondary's record and discarded the primary's.
            string secondaryJson = SerializeBundle(CreateRandomBundle());
            string primaryJson = SerializeBundle(CreateRandomBundle());
            string primaryProviderName = GetRandomString();
            Provider randomPrimaryProvider = CreateRandomProvider(friendlyName: primaryProviderName);

            List<(string Provider, string Json)> inputBundles = new List<(string Provider, string Json)>
            {
                (GetRandomString(), secondaryJson),
                (primaryProviderName, primaryJson)
            };

            string expectedJson = primaryJson;
            string randomNhsNumber = GetRandomString();
            Guid correlationId = Guid.NewGuid();

            // when
            string actualJson = await this.fhirReconciliationService.ReconcileSerialisedAsync(
                bundles: inputBundles,
                nhsNumber: randomNhsNumber,
                primaryProvider: randomPrimaryProvider,
                correlationId: correlationId);

            // then
            actualJson.Should().BeEquivalentTo(expectedJson);
            actualJson.Should().NotBeEquivalentTo(secondaryJson);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldSkipEmptyBundlesOnReconcileSerialisedAsync()
        {
            // given
            string populatedJson = SerializeBundle(CreateRandomBundle());
            string primaryProviderName = GetRandomString();
            Provider randomPrimaryProvider = CreateRandomProvider(friendlyName: primaryProviderName);

            List<(string Provider, string Json)> inputBundles = new List<(string Provider, string Json)>
            {
                (GetRandomString(), null),
                (GetRandomString(), string.Empty),
                (primaryProviderName, populatedJson)
            };

            string expectedJson = populatedJson;
            string randomNhsNumber = GetRandomString();
            Guid correlationId = Guid.NewGuid();

            // when
            string actualJson = await this.fhirReconciliationService.ReconcileSerialisedAsync(
                bundles: inputBundles,
                nhsNumber: randomNhsNumber,
                primaryProvider: randomPrimaryProvider,
                correlationId: correlationId);

            // then
            actualJson.Should().BeEquivalentTo(expectedJson);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldFallBackAndWarnWhenPrimaryProviderReturnedNoRecordOnReconcileSerialisedAsync()
        {
            // given
            // The primary is present in the list but returned nothing, so a secondary's record is
            // substituted. That substitution has to be visible rather than silent.
            string secondaryProviderName = GetRandomString();
            string secondaryJson = SerializeBundle(CreateRandomBundle());
            string primaryProviderName = GetRandomString();
            Provider randomPrimaryProvider = CreateRandomProvider(friendlyName: primaryProviderName);

            List<(string Provider, string Json)> inputBundles = new List<(string Provider, string Json)>
            {
                (primaryProviderName, null),
                (secondaryProviderName, secondaryJson)
            };

            string randomNhsNumber = GetRandomString();
            Guid correlationId = Guid.NewGuid();

            string expectedWarning =
                $"Primary provider '{primaryProviderName}' returned no record; " +
                    $"returning '{secondaryProviderName}' instead.  " +
                    $"CorrelationId: {correlationId}";

            // when
            string actualJson = await this.fhirReconciliationService.ReconcileSerialisedAsync(
                bundles: inputBundles,
                nhsNumber: randomNhsNumber,
                primaryProvider: randomPrimaryProvider,
                correlationId: correlationId);

            // then
            actualJson.Should().BeEquivalentTo(secondaryJson);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogWarningAsync(expectedWarning),
                    Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
