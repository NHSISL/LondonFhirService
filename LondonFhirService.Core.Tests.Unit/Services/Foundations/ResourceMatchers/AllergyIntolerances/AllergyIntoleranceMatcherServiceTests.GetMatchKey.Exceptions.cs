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
    public void ShouldThrowServiceExceptionOnGetMatchKeyIfServiceErrorOccurs()
    {
        // given
        JsonElement malformedAllergyIntoleranceResource = CreateMalformedCodingResource();
        Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();
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
        Action getMatchKeyAction = () =>
            this.allergyIntoleranceMatcherService.GetMatchKey(malformedAllergyIntoleranceResource, resourceIndex);

        // then
        AllergyIntoleranceServiceException actualAllergyIntoleranceServiceException =
            Assert.Throws<AllergyIntoleranceServiceException>(getMatchKeyAction);

        actualAllergyIntoleranceServiceException.Should()
            .BeEquivalentTo(expectedAllergyIntoleranceServiceException);
    }
}
