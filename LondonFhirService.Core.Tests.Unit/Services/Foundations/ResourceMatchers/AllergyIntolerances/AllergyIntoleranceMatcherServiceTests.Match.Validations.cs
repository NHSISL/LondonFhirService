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
    public void ShouldThrowValidationExceptionOnMatchIfSource1ResourcesIsNull()
    {
        // given
        List<JsonElement> nullSource1Resources = null;
        List<JsonElement> source2Resources = [];
        Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
        Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();

        var nullAllergyIntoleranceException =
            new NullAllergyIntoleranceException(nameof(nullSource1Resources));

        var expectedAllergyIntoleranceValidationException =
            new AllergyIntoleranceValidationException(
                message: "allergy intolerance validation error occurred, please fix the errors and try again.",
                innerException: nullAllergyIntoleranceException);

        // when
        Action matchAction = () =>
            this.allergyIntoleranceMatcherService.Match(
                nullSource1Resources,
                source2Resources,
                source1ResourceIndex,
                source2ResourceIndex);

        // then
        AllergyIntoleranceValidationException actualAllergyIntoleranceValidationException =
            Assert.Throws<AllergyIntoleranceValidationException>(matchAction);

        actualAllergyIntoleranceValidationException.Should()
            .BeEquivalentTo(expectedAllergyIntoleranceValidationException);
    }

    [Fact]
    public void ShouldThrowValidationExceptionOnMatchIfSource2ResourceIndexIsNull()
    {
        // given
        List<JsonElement> source1Resources = [];
        List<JsonElement> source2Resources = [];
        Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
        Dictionary<string, JsonElement> nullSource2ResourceIndex = null;

        var NullAllergyIntoleranceException =
            new NullAllergyIntoleranceException(nameof(nullSource2ResourceIndex));

        var expectedAllergyIntoleranceValidationException =
            new AllergyIntoleranceValidationException(
                message: "allergy intolerance validation error occurred, please fix the errors and try again.",
                innerException: NullAllergyIntoleranceException);

        // when
        Action matchAction = () =>
            this.allergyIntoleranceMatcherService.Match(
                source1Resources,
                source2Resources,
                source1ResourceIndex,
                nullSource2ResourceIndex);

        // then
        AllergyIntoleranceValidationException actualAllergyIntoleranceValidationException =
            Assert.Throws<AllergyIntoleranceValidationException>(matchAction);

        actualAllergyIntoleranceValidationException.Should()
            .BeEquivalentTo(expectedAllergyIntoleranceValidationException);
    }
}
