// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers.EpisodeOfCares.Exceptions;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.EpisodeOfCares;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.EpisodesOfCare
{
    public partial class EpisodeOfCareMatcherServiceTests
    {
        [Fact]
        public async Task ShouldThrowServiceExceptionOnMatchIfServiceErrorOccursAndLogItAsync()
        {
            // given
            List<JsonElement> invalidSource1Resources = new();
            List<JsonElement> invalidSource2Resources = new();
            Dictionary<string, JsonElement> invalidSource1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> invalidSource2ResourceIndex = CreateResourceIndex();
            var serviceException = new Exception();

            var failedEpisodeOfCareMatcherServiceException =
                new FailedEpisodeOfCareMatcherServiceException(
                    message: "Failed episode of care matcher service occurred, please contact support",
                    innerException: serviceException);

            var expectedEpisodeOfCareMatcherServiceException =
                new EpisodeOfCareMatcherServiceException(
                    message: "Episode of care matcher service error occurred, contact support.",
                    innerException: failedEpisodeOfCareMatcherServiceException);

            var episodeOfCareMatcherServiceMock = 
                new Mock<EpisodeOfCareMatcherService>(loggingBrokerMock.Object) { CallBase = true };

            episodeOfCareMatcherServiceMock.Setup(service =>
                service.ValidateOnMatchArguments(
                    invalidSource1Resources,
                    invalidSource2Resources,
                    invalidSource1ResourceIndex,
                    invalidSource2ResourceIndex))
                        .Throws(serviceException);

            // when
            ValueTask<ResourceMatch> matchTask =
                episodeOfCareMatcherServiceMock.Object.MatchAsync(
                    invalidSource1Resources,
                    invalidSource2Resources,
                    invalidSource1ResourceIndex,
                    invalidSource2ResourceIndex);

            EpisodeOfCareMatcherServiceException actualEpisodeOfCareMatcherServiceException =
                await Assert.ThrowsAsync<EpisodeOfCareMatcherServiceException>(
                    matchTask.AsTask);

            // then
            actualEpisodeOfCareMatcherServiceException.Should()
                .BeEquivalentTo(expectedEpisodeOfCareMatcherServiceException);

            episodeOfCareMatcherServiceMock.Verify(service =>
                service.ValidateOnMatchArguments(
                    invalidSource1Resources,
                    invalidSource2Resources,
                    invalidSource1ResourceIndex,
                    invalidSource2ResourceIndex),
                        Times.Once);

            episodeOfCareMatcherServiceMock.Verify(service =>
                service.MatchAsync(
                    invalidSource1Resources,
                    invalidSource2Resources,
                    invalidSource1ResourceIndex,
                    invalidSource2ResourceIndex),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedEpisodeOfCareMatcherServiceException))),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            episodeOfCareMatcherServiceMock.VerifyNoOtherCalls();
        }
    }
}