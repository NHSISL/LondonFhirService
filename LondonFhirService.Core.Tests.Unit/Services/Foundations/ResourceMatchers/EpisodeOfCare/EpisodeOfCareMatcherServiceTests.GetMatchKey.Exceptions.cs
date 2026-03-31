// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers.EpisodeOfCares.Exceptions;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.EpisodeOfCares;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.EpisodesOfCare
{
    public partial class EpisodeOfCareMatcherServiceTests
    {
        [Fact]
        public async Task ShouldThrowServiceExceptionOnGetMatchKeyIfServiceErrorOccursAndLogItAsync()
        {
            // given
            JsonElement resource = new();
            Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();
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
                new Mock<EpisodeOfCareMatcherService>(loggingBrokerMock.Object)
                { CallBase = true };

            episodeOfCareMatcherServiceMock.Setup(service =>
                service.ValidateOnGetMatchKeyArguments(
                    resource,
                    resourceIndex))
                        .Throws(serviceException);

            // when
            ValueTask<string> matchTask =
                episodeOfCareMatcherServiceMock.Object.GetMatchKeyAsync(
                    resource,
                    resourceIndex);

            EpisodeOfCareMatcherServiceException actualEpisodeOfCareMatcherServiceException =
                await Assert.ThrowsAsync<EpisodeOfCareMatcherServiceException>(
                    matchTask.AsTask);

            // then
            actualEpisodeOfCareMatcherServiceException.Should()
                .BeEquivalentTo(expectedEpisodeOfCareMatcherServiceException);

            episodeOfCareMatcherServiceMock.Verify(service =>
                service.ValidateOnGetMatchKeyArguments(
                    resource,
                    resourceIndex),
                        Times.Once);

            episodeOfCareMatcherServiceMock.Verify(service =>
                service.GetMatchKeyAsync(
                    resource,
                    resourceIndex),
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