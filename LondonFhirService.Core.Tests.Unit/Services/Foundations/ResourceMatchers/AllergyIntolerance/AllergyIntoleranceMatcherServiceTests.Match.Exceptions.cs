// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers.AllergyIntolerances.Exceptions;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.AllergyIntolerances;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.AllergyIntolerances
{
    public partial class AllergyIntoleranceMatcherServiceTests
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

            var failedAllergyIntoleranceMatcherServiceException =
                new FailedAllergyIntoleranceMatcherServiceException(
                    message: "Failed allergy intolerance matcher service occurred, please contact support",
                    innerException: serviceException);

            var expectedAllergyIntoleranceMatcherServiceException =
                new AllergyIntoleranceMatcherServiceException(
                    message: "Allergy intolerance matcher service error occurred, contact support.",
                    innerException: failedAllergyIntoleranceMatcherServiceException);

            var allergyIntoleranceMatcherServiceMock = new Mock<AllergyIntoleranceMatcherService>(loggingBrokerMock.Object)
                { CallBase = true };

            allergyIntoleranceMatcherServiceMock.Setup(service =>
                service.ValidateOnMatchArguments(
                    invalidSource1Resources,
                    invalidSource2Resources,
                    invalidSource1ResourceIndex,
                    invalidSource2ResourceIndex))
                        .Throws(serviceException);

            // when
            ValueTask<ResourceMatch> matchTask =
                allergyIntoleranceMatcherServiceMock.Object.MatchAsync(
                    invalidSource1Resources,
                    invalidSource2Resources,
                    invalidSource1ResourceIndex,
                    invalidSource2ResourceIndex);

            AllergyIntoleranceMatcherServiceException actualAllergyIntoleranceMatcherServiceException =
                await Assert.ThrowsAsync<AllergyIntoleranceMatcherServiceException>(
                    matchTask.AsTask);

            // then
            actualAllergyIntoleranceMatcherServiceException.Should()
                .BeEquivalentTo(expectedAllergyIntoleranceMatcherServiceException);

            allergyIntoleranceMatcherServiceMock.Verify(service =>
                service.ValidateOnMatchArguments(
                    invalidSource1Resources,
                    invalidSource2Resources,
                    invalidSource1ResourceIndex,
                    invalidSource2ResourceIndex),
                        Times.Once);

            allergyIntoleranceMatcherServiceMock.Verify(service =>
                service.MatchAsync(
                    invalidSource1Resources,
                    invalidSource2Resources,
                    invalidSource1ResourceIndex,
                    invalidSource2ResourceIndex),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedAllergyIntoleranceMatcherServiceException))),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            allergyIntoleranceMatcherServiceMock.VerifyNoOtherCalls();
        }
    }
}