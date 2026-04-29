// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.FamilyMemberHistories
{
    public partial class FamilyMemberHistoryMatcherServiceTests
    {
        [Fact]
        public async Task ShouldGetMatchKeyWhenFamilyMemberHistoryHasDdsIdentifierAsync()
        {
            // given
            string inputDdsIdentifierValue = GetRandomDdsIdentifierValue();
            string randomId = GetRandomString();

            JsonElement familyMemberHistoryResource = CreateFamilyMemberHistoryResource(
                ddsIdentifierValue: inputDdsIdentifierValue,
                id: randomId);

            Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();
            string expectedMatchKey = inputDdsIdentifierValue;

            // when
            string actualMatchKey =
                await this.familyMemberHistoryMatcherService.GetMatchKeyAsync(familyMemberHistoryResource, resourceIndex);

            // then
            actualMatchKey.Should().Be(expectedMatchKey);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
