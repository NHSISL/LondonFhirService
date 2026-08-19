// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.Providers;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Core.Tests.Unit.Services.Orchestrations.FhirReconciliations.STU3
{
    public partial class Stu3FhirReconciliationServiceTests
    {
        [Fact]
        public async Task ShouldReturnFirstPopulatedBundleOnReconcileSerialisedAsync()
        {
            // given
            List<(string Provider, string Json)> randomBundles = CreateRandomBundles();
            List<(string Provider, string Json)> inputBundles = randomBundles;
            string expectedJson = inputBundles[0].Json;
            string randomNhsNumber = GetRandomString();
            Provider randomPrimaryProvider = CreateRandomProvider();
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
        public async Task ShouldSkipEmptyBundlesOnReconcileSerialisedAsync()
        {
            // given
            string populatedJson = SerializeBundle(CreateRandomBundle());

            List<(string Provider, string Json)> inputBundles = new List<(string Provider, string Json)>
            {
                (GetRandomString(), null),
                (GetRandomString(), string.Empty),
                (GetRandomString(), populatedJson)
            };

            string expectedJson = populatedJson;
            string randomNhsNumber = GetRandomString();
            Provider randomPrimaryProvider = CreateRandomProvider();
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
    }
}
