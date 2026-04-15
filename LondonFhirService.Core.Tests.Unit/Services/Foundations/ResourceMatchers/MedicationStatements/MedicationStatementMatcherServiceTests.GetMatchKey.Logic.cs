// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.MedicationStatements
{
    public partial class MedicationStatementMatcherServiceTests
    {
        [Fact]
        public async Task ShouldGetMatchKeyUsingMedicationCodeableConceptAsync()
        {
            // given
            string randomSnomedCode = "123456789";
            string randomDateAsserted = "2024-06-15";
            string inputSnomedCode = randomSnomedCode;
            string inputDateAsserted = randomDateAsserted;

            JsonElement inputResource =
                CreateMedicationStatementWithCodeableConcept(
                    inputSnomedCode,
                    inputDateAsserted);

            Dictionary<string, JsonElement> inputResourceIndex = CreateResourceIndex();
            string expectedMatchKey = $"{inputSnomedCode}|{inputDateAsserted}";

            // when
            string actualMatchKey =
                await this.medicationStatementMatcherService.GetMatchKeyAsync(
                    inputResource,
                    inputResourceIndex);

            // then
            actualMatchKey.Should().Be(expectedMatchKey);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldGetMatchKeyUsingMedicationReferenceAsync()
        {
            // given
            string randomSnomedCode = "987654321";
            string randomDateAsserted = "2024-07-20";
            string randomReference = "Medication/med-001";
            string inputSnomedCode = randomSnomedCode;
            string inputDateAsserted = randomDateAsserted;
            string inputReference = randomReference;

            JsonElement medicationResource = CreateMedicationForIndex(inputSnomedCode);

            JsonElement inputResource =
                CreateMedicationStatementWithReference(
                    inputReference,
                    inputDateAsserted);

            Dictionary<string, JsonElement> inputResourceIndex = new();
            inputResourceIndex.Add(inputReference, medicationResource);
            string expectedMatchKey = $"{inputSnomedCode}|{inputDateAsserted}";

            // when
            string actualMatchKey =
                await this.medicationStatementMatcherService.GetMatchKeyAsync(
                    inputResource,
                    inputResourceIndex);

            // then
            actualMatchKey.Should().Be(expectedMatchKey);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnNullGetMatchKeyWhenMedicationHasNoSnomedCodingAsync()
        {
            // given
            string randomDateAsserted = "2024-06-15";
            string inputDateAsserted = randomDateAsserted;

            JsonElement inputResource =
                CreateNonSnomedMedicationStatement(inputDateAsserted);

            Dictionary<string, JsonElement> inputResourceIndex = CreateResourceIndex();
            string noMatchKey = null;

            // when
            string actualMatchKey =
                await this.medicationStatementMatcherService.GetMatchKeyAsync(
                    inputResource,
                    inputResourceIndex);

            // then
            actualMatchKey.Should().Be(noMatchKey);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnNullOnGetMatchKeyWhenDateAssertedIsMissingAsync()
        {
            // given
            string randomSnomedCode = "123456789";
            string inputSnomedCode = randomSnomedCode;

            JsonElement inputResource =
                CreateMedicationStatementWithCodeableConceptWithoutDateAsserted(
                    inputSnomedCode);

            Dictionary<string, JsonElement> inputResourceIndex = CreateResourceIndex();
            string noMatchKey = null;

            // when
            string actualMatchKey =
                await this.medicationStatementMatcherService.GetMatchKeyAsync(
                    inputResource,
                    inputResourceIndex);

            // then
            actualMatchKey.Should().Be(noMatchKey);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldFallBackToMedicationCodeableConceptWhenReferenceNotInIndexAsync()
        {
            // given
            string randomSnomedCode = "111222333";
            string randomDateAsserted = "2024-08-10";
            string randomReference = "Medication/med-missing";
            string inputSnomedCode = randomSnomedCode;
            string inputDateAsserted = randomDateAsserted;

            JsonElement inputResource =
                CreateMedicationStatementWithReferenceAndCodeableConcept(
                    randomReference,
                    inputSnomedCode,
                    inputDateAsserted);

            Dictionary<string, JsonElement> inputResourceIndex = CreateResourceIndex();
            string expectedMatchKey = $"{inputSnomedCode}|{inputDateAsserted}";

            // when
            string actualMatchKey =
                await this.medicationStatementMatcherService.GetMatchKeyAsync(
                    inputResource,
                    inputResourceIndex);

            // then
            actualMatchKey.Should().Be(expectedMatchKey);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
