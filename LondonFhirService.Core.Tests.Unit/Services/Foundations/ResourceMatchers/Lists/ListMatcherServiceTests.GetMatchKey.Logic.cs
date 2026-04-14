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
        public async Task ShouldReturnTitleAsMatchKeyOnGetMatchKeyAsync()
        {
            // given
            string randomTitle = GetRandomString();
            JsonElement resource = CreateListResourceWithTitle(randomTitle);
            Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();

            // when
            string actualMatchKey = await this.listMatcherService.GetMatchKeyAsync(
                resource,
                resourceIndex);

            // then
            actualMatchKey.Should().Be(randomTitle);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnNullMatchKeyOnGetMatchKeyWhenTitleIsMissingAsync()
        {
            // given
            JsonElement resource = CreateListResourceWithoutTitle();
            Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();

            // when
            string actualMatchKey = await this.listMatcherService.GetMatchKeyAsync(
                resource,
                resourceIndex);

            // then
            actualMatchKey.Should().BeNull();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
