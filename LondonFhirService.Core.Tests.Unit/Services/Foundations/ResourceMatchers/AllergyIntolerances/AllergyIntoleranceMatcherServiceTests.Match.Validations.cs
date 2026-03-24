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
    public void ShouldThrowValidationExceptionOnMatchIfSource1ResourcesIsNull()
    {
        // given
        List<JsonElement> nullSource1Resources = null;
        List<JsonElement> source2Resources = [];
        Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
        Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();

        var nullResourceMatcherException =
            new NullResourceMatcherException(nameof(nullSource1Resources));

        var expectedResourceMatcherValidationException =
            new ResourceMatcherValidationException(
                message: "Resource matcher validation error occurred, please fix the errors and try again.",
                innerException: nullResourceMatcherException);

        // when
        Action matchAction = () =>
            this.allergyIntoleranceMatcherService.Match(
                nullSource1Resources,
                source2Resources,
                source1ResourceIndex,
                source2ResourceIndex);

        // then
        ResourceMatcherValidationException actualResourceMatcherValidationException =
            Assert.Throws<ResourceMatcherValidationException>(matchAction);

        actualResourceMatcherValidationException.Should()
            .BeEquivalentTo(expectedResourceMatcherValidationException);
    }

    [Fact]
    public void ShouldThrowValidationExceptionOnMatchIfSource2ResourceIndexIsNull()
    {
        // given
        List<JsonElement> source1Resources = [];
        List<JsonElement> source2Resources = [];
        Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
        Dictionary<string, JsonElement> nullSource2ResourceIndex = null;

        var nullResourceMatcherException =
            new NullResourceMatcherException(nameof(nullSource2ResourceIndex));

        var expectedResourceMatcherValidationException =
            new ResourceMatcherValidationException(
                message: "Resource matcher validation error occurred, please fix the errors and try again.",
                innerException: nullResourceMatcherException);

        // when
        Action matchAction = () =>
            this.allergyIntoleranceMatcherService.Match(
                source1Resources,
                source2Resources,
                source1ResourceIndex,
                nullSource2ResourceIndex);

        // then
        ResourceMatcherValidationException actualResourceMatcherValidationException =
            Assert.Throws<ResourceMatcherValidationException>(matchAction);

        actualResourceMatcherValidationException.Should()
            .BeEquivalentTo(expectedResourceMatcherValidationException);
    }
}
