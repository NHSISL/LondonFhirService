// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.EpisodesOfCare
{
    public partial class EpisodeOfCareMatcherServiceTests
    {
        [Fact]
        public async Task ShouldGetMatchKeyForEpisodeOfCareWithPeriodStartAsync()
        {
            // given
            var periodStart = "2024-01-01";
            JsonElement resource = CreateEpisodeOfCareResourceWithPeriodStart(periodStart);
            Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();
            string expectedMatchKey = periodStart;

            // when
            string actualMatchKey = await this.episodeOfCareMatcherService
                .GetMatchKeyAsync(resource, resourceIndex);

            // then
            actualMatchKey.Should().Be(expectedMatchKey);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnNullMatchKeyWhenEpisodeOfCareHasNoPeriodAsync()
        {
            // given
            JsonElement resource = CreateEpisodeOfCareResourceWithNoPeriod();
            Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();

            // when
            string actualMatchKey = await this.episodeOfCareMatcherService
                .GetMatchKeyAsync(resource, resourceIndex);

            // then
            actualMatchKey.Should().BeNull();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnNullMatchKeyWhenEpisodeOfCareHasNoPeriodStartAsync()
        {
            // given
            JsonElement resource = CreateEpisodeOfCareResourceWithPeriodButNoStart();
            Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();

            // when
            string actualMatchKey = await this.episodeOfCareMatcherService
                .GetMatchKeyAsync(resource, resourceIndex);

            // then
            actualMatchKey.Should().BeNull();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
