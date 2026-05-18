// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers.Exceptions;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.Locations;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Locations
{
    public partial class LocationMatcherServiceTests
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

            var failedResourceMatcherServiceException =
                new FailedResourceMatcherServiceException(
                    message: "Failed location matcher service occurred, please contact support",
                    innerException: serviceException);

            var expectedResourceMatcherServiceException =
                new ResourceMatcherServiceException(
                    message: "Location matcher service error occurred, contact support.",
                    innerException: failedResourceMatcherServiceException);

            var locationMatcherServiceMock = new Mock<LocationMatcherService>(loggingBrokerMock.Object)
                { CallBase = true };

            locationMatcherServiceMock.Setup(service =>
                service.ValidateOnMatchArguments(
                    invalidSource1Resources,
                    invalidSource2Resources,
                    invalidSource1ResourceIndex,
                    invalidSource2ResourceIndex))
                        .Throws(serviceException);

            // when
            ValueTask<ResourceMatch> matchTask =
                locationMatcherServiceMock.Object.MatchAsync(
                    invalidSource1Resources,
                    invalidSource2Resources,
                    invalidSource1ResourceIndex,
                    invalidSource2ResourceIndex);

            ResourceMatcherServiceException actualResourceMatcherServiceException =
                await Assert.ThrowsAsync<ResourceMatcherServiceException>(
                    matchTask.AsTask);

            // then
            actualResourceMatcherServiceException.Should()
                .BeEquivalentTo(expectedResourceMatcherServiceException);

            locationMatcherServiceMock.Verify(service =>
                service.ValidateOnMatchArguments(
                    invalidSource1Resources,
                    invalidSource2Resources,
                    invalidSource1ResourceIndex,
                    invalidSource2ResourceIndex),
                        Times.Once);

            locationMatcherServiceMock.Verify(service =>
                service.MatchAsync(
                    invalidSource1Resources,
                    invalidSource2Resources,
                    invalidSource1ResourceIndex,
                    invalidSource2ResourceIndex),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedResourceMatcherServiceException))),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            locationMatcherServiceMock.VerifyNoOtherCalls();
        }
    }
}
