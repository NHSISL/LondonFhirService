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
    public void ShouldThrowServiceExceptionOnGetMatchKeyIfServiceErrorOccurs()
    {
        // given
        JsonElement malformedAllergyIntoleranceResource = CreateMalformedCodingResource();
        Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();
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
        Action getMatchKeyAction = () =>
            this.allergyIntoleranceMatcherService.GetMatchKey(malformedAllergyIntoleranceResource, resourceIndex);

        // then
        ResourceMatcherServiceException actualResourceMatcherServiceException =
            Assert.Throws<ResourceMatcherServiceException>(getMatchKeyAction);

        actualResourceMatcherServiceException.Should()
            .BeEquivalentTo(expectedResourceMatcherServiceException);
    }
}
