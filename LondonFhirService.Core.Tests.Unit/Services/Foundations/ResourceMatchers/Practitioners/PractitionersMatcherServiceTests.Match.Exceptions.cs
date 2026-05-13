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
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.Practitioners;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Practitioners
{
    public partial class PractitionerMatcherServiceTests
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
                    message: "Failed practitioner matcher service occurred, please contact support",
                    innerException: serviceException);

            var expectedResourceMatcherServiceException =
                new ResourceMatcherServiceException(
                    message: "Practitioner matcher service error occurred, contact support.",
                    innerException: failedResourceMatcherServiceException);

            var practitionerMatcherServiceMock = new Mock<PractitionerMatcherService>(loggingBrokerMock.Object)
                { CallBase = true };

            practitionerMatcherServiceMock.Setup(service =>
                service.ValidateOnMatchArguments(
                    invalidSource1Resources,
                    invalidSource2Resources,
                    invalidSource1ResourceIndex,
                    invalidSource2ResourceIndex))
                        .Throws(serviceException);

            // when
            ValueTask<ResourceMatch> matchTask =
                practitionerMatcherServiceMock.Object.MatchAsync(
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

            practitionerMatcherServiceMock.Verify(service =>
                service.ValidateOnMatchArguments(
                    invalidSource1Resources,
                    invalidSource2Resources,
                    invalidSource1ResourceIndex,
                    invalidSource2ResourceIndex),
                        Times.Once);

            practitionerMatcherServiceMock.Verify(service =>
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
            practitionerMatcherServiceMock.VerifyNoOtherCalls();
        }
    }
}
