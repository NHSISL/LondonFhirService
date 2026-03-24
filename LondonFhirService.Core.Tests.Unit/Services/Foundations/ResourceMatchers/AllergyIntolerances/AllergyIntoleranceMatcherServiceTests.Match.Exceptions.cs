// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text.Json;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.AllergyIntolerances.AllergyIntolerances.Exceptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.AllergyIntolerances.AllergyIntolerances;

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

        var failedAllergyIntoleranceServiceException =
            new FailedAllergyIntolerancesServiceException(
                message: "Failed allergy intolerance service error occurred, contact support.",
                innerException: invalidOperationException);

        var expectedAllergyIntoleranceServiceException =
            new AllergyIntoleranceServiceException(
                message: "Allergy intolerance service error occurred, contact support.",
                innerException: failedAllergyIntoleranceServiceException);

        // when
        Action matchAction = () =>
            this.allergyIntoleranceMatcherService.Match(
                source1Resources,
                source2Resources,
                source1ResourceIndex,
                source2ResourceIndex);

        // then
        AllergyIntoleranceServiceException actualAllergyIntoleranceServiceException =
            Assert.Throws<AllergyIntoleranceServiceException>(matchAction);

        actualAllergyIntoleranceServiceException.Should()
            .BeEquivalentTo(expectedAllergyIntoleranceServiceException);
    }
}
