// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.MedicationRequests
{
    public partial class MedicationRequestMatcherServiceTests
    {
        [Fact]
        public async Task ShouldGetMatchKeyWhenMedicationRequestHasDdsIdentifierAsync()
        {
            // given
            string inputDdsIdentifierValue = GetRandomDdsIdentifierValue();
            string randomId = GetRandomString();

            JsonElement medicationRequestResource = CreateMedicationRequestResource(
                ddsIdentifierValue: inputDdsIdentifierValue,
                id: randomId);

            Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();
            string expectedMatchKey = inputDdsIdentifierValue;

            // when
            string actualMatchKey =
                await this.medicationRequestMatcherService.GetMatchKeyAsync(medicationRequestResource, resourceIndex);

            // then
            actualMatchKey.Should().Be(expectedMatchKey);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
