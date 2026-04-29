// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Lists
{
    public partial class ListMatcherServiceTests
    {
        [Fact]
        public async Task ShouldReturnMatchKeyWhenResourceHasTitleAsync()
        {
            // given
            string randomTitle = GetRandomTitle();

            JsonElement randomResource = CreateListResourceWithTitle(randomTitle);
            Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();

            // when
            string actualMatchKey =
                await this.listMatcherService.GetMatchKeyAsync(
                    randomResource,
                    resourceIndex);

            // then
            actualMatchKey.Should().Be(randomTitle);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnNullKeyWhenResourceHasNoTitleAsync()
        {
            // given
            JsonElement resourceWithoutTitle = CreateListResourceWithoutTitle();
            Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();

            // when
            string actualMatchKey =
                await this.listMatcherService.GetMatchKeyAsync(
                    resourceWithoutTitle,
                    resourceIndex);

            // then
            actualMatchKey.Should().BeNull();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
