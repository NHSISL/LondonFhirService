// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers.EpisodeOfCares.Exceptions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers.Exceptions;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.EpisodesOfCare
{
    public partial class EpisodeOfCareMatcherServiceTests
    {
        [Fact]
        public async Task ShouldThrowValidationExceptionOnGetMatchKeyIfResourceIsInvalidAsync()
        {
            // given
            JsonElement invalidResource = default;
            Dictionary<string, JsonElement> invalidResourceIndex = null;

            var invalidArgumentResourceMatcherException =
                new InvalidArgumentResourceMatcherException(
                    message:
                        "Resource matcher arguments are invalid. " +
                        "Please correct the errors and try again.");

            invalidArgumentResourceMatcherException.AddData(
                key: "resource",
                values: "Json element is invalid.");

            invalidArgumentResourceMatcherException.UpsertDataList(
                key: "resourceIndex",
                value: "Dictionary is required.");

            var expectedEpisodeOfCareMatcherServiceValidationException =
                new EpisodeOfCareMatcherServiceValidationException(
                    message: "Episode of care matcher validation errors occurred, " +
                        "please try again.",
                    innerException: invalidArgumentResourceMatcherException);

            // when
            ValueTask<string> getMatchKeyTask =
                this.episodeOfCareMatcherService.GetMatchKeyAsync(
                    invalidResource,
                    invalidResourceIndex);

            // then
            EpisodeOfCareMatcherServiceValidationException actualException =
                await Assert.ThrowsAsync<EpisodeOfCareMatcherServiceValidationException>(
                    getMatchKeyTask.AsTask);

            actualException.Should()
                .BeEquivalentTo(expectedEpisodeOfCareMatcherServiceValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedEpisodeOfCareMatcherServiceValidationException))),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}