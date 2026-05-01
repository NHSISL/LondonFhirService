// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers.Exceptions;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Locations
{
    public partial class LocationMatcherServiceTests
    {
        [Fact]
        public async Task ShouldThrowValidationExceptionOnMatchIfArgumentsAreInvalidAsync()
        {
            // given
            List<JsonElement> invalidSource1Resources = null;
            List<JsonElement> invalidSource2Resources = null;
            Dictionary<string, JsonElement> invalidSource1ResourceIndex = null;
            Dictionary<string, JsonElement> invalidSource2ResourceIndex = null;

            var invalidArgumentResourceMatcherException =
                new InvalidArgumentResourceMatcherException(
                    message: "Resource matcher arguments are invalid. Please correct the errors and try again.");

            invalidArgumentResourceMatcherException.AddData(
                key: "source1Resources",
                values: "List is required.");

            invalidArgumentResourceMatcherException.AddData(
                key: "source2Resources",
                values: "List is required.");

            invalidArgumentResourceMatcherException.AddData(
                key: "source1ResourceIndex",
                values: "Dictionary is required.");

            invalidArgumentResourceMatcherException.AddData(
                key: "source2ResourceIndex",
                values: "Dictionary is required.");

            var expectedResourceMatcherServiceValidationException =
                new ResourceMatcherServiceValidationException(
                    message: "Location matcher validation errors occurred, " +
                        "please try again.",
                    innerException: invalidArgumentResourceMatcherException);

            // when
            ValueTask<ResourceMatch> matchTask =
                this.locationMatcherService.MatchAsync(
                    invalidSource1Resources,
                    invalidSource2Resources,
                    invalidSource1ResourceIndex,
                    invalidSource2ResourceIndex);

            // then
            ResourceMatcherServiceValidationException actualException =
                await Assert.ThrowsAsync<ResourceMatcherServiceValidationException>(
                    matchTask.AsTask);

            actualException.Should()
                .BeEquivalentTo(expectedResourceMatcherServiceValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedResourceMatcherServiceValidationException))),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
