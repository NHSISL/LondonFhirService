// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text.Json;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers.Exceptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.AllergyIntolerances;

public partial class AllergyIntoleranceMatcherServiceTests
{
    [Fact]
    public void ShouldThrowServiceExceptionOnMatchIfServiceErrorOccurs()
    {
        // given
        JsonElement malformedAllergyIntoleranceResource = CreateMalformedCodingResource();
        List<JsonElement> source1Resources = [malformedAllergyIntoleranceResource];
        List<JsonElement> source2Resources = [];
        Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
        Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();
        var invalidOperationException = new InvalidOperationException(
            "The requested operation requires an element of type 'Array', but the target element has type 'Object'.");

        var failedResourceMatcherServiceException =
            new FailedResourceMatcherServiceException(
                message: "Failed resource matcher service error occurred, contact support.",
                innerException: invalidOperationException);

        var expectedResourceMatcherServiceException =
            new ResourceMatcherServiceException(
                message: "Resource matcher service error occurred, contact support.",
                innerException: failedResourceMatcherServiceException);

        // when
        Action matchAction = () =>
            this.allergyIntoleranceMatcherService.Match(
                source1Resources,
                source2Resources,
                source1ResourceIndex,
                source2ResourceIndex);

        // then
        ResourceMatcherServiceException actualResourceMatcherServiceException =
            Assert.Throws<ResourceMatcherServiceException>(matchAction);

        actualResourceMatcherServiceException.Should()
            .BeEquivalentTo(expectedResourceMatcherServiceException);
    }
}
