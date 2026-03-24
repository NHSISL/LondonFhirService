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
    public void ShouldThrowValidationExceptionOnGetMatchKeyIfResourceIsNull()
    {
        // given
        JsonElement nullResource = default;
        Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();

        var nullAllergyIntoleranceException =
            new NullAllergyIntoleranceException(nameof(nullResource));

        var expectedAllergyIntoleranceValidationException =
            new AllergyIntoleranceValidationException(
                message: "Allergy intolerance matcher validation error occurred, please fix the errors and try again.",
                innerException: nullAllergyIntoleranceException);

        // when
        Action getMatchKeyAction = () =>
            this.allergyIntoleranceMatcherService.GetMatchKey(nullResource, resourceIndex);

        // then
        AllergyIntoleranceValidationException actualAllergyIntoleranceValidationException =
            Assert.Throws<AllergyIntoleranceValidationException>(getMatchKeyAction);

        actualAllergyIntoleranceValidationException.Should()
            .BeEquivalentTo(expectedAllergyIntoleranceValidationException);
    }

    [Fact]
    public void ShouldThrowValidationExceptionOnGetMatchKeyIfResourceIndexIsNull()
    {
        // given
        JsonElement allergyIntoleranceResource =
            CreateAllergyIntoleranceResource(
                snomedCode: "91936005",
                onsetDateTime: "2024-01-01");

        Dictionary<string, JsonElement> nullResourceIndex = null;

        var NullAllergyIntoleranceException =
            new NullAllergyIntoleranceException(nameof(nullResourceIndex));

        var expectedAllergyIntoleranceValidationException =
            new AllergyIntoleranceValidationException(
                message: "allergy intolerance validation error occurred, please fix the errors and try again.",
                innerException: NullAllergyIntoleranceException);

        // when
        Action getMatchKeyAction = () =>
            this.allergyIntoleranceMatcherService.GetMatchKey(allergyIntoleranceResource, nullResourceIndex);

        // then
        AllergyIntoleranceValidationException actualAllergyIntoleranceValidationException =
            Assert.Throws<AllergyIntoleranceValidationException>(getMatchKeyAction);

        actualAllergyIntoleranceValidationException.Should()
            .BeEquivalentTo(expectedAllergyIntoleranceValidationException);
    }
}
