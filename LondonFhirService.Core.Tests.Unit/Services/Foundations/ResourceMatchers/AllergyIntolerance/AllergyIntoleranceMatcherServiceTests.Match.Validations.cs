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
using LondonFhirService.Core.Models.Foundations.ResourceMatchers.Exceptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.AllergyIntolerances;

public partial class AllergyIntoleranceMatcherServiceTests
{
    [Fact]
    public async Task ShouldThrowValidationExceptionOnMatchIfSource1ResourcesIsInvalidAsync()
    {
        // given
        List<JsonElement> invalidSource1Resources = null;
        List<JsonElement> source2Resources = new();
        Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
        Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();

        var invalidArgumentResourceMatcherException =
            new InvalidArgumentResourceMatcherException(
                message: "Resource matcher arguments are invalid. Please correct the errors and try again.");

        invalidArgumentResourceMatcherException.UpsertDataList(
            key: "source1Resources",
            value: "List is required.");

        var expectedAllergyIntoleranceMatcherServiceValidationException =
            new AllergyIntoleranceMatcherServiceValidationException(
                message: "Allergy intolerance matcher validation errors occurred, " +
                    "please try again.",
                innerException: invalidArgumentResourceMatcherException);

        // when
        ValueTask<ResourceMatch> matchTask =
            this.allergyIntoleranceMatcherService.MatchAsync(
                invalidSource1Resources,
                source2Resources,
                source1ResourceIndex,
                source2ResourceIndex);

        // then
        AllergyIntoleranceMatcherServiceValidationException actualException =
            await Assert.ThrowsAsync<AllergyIntoleranceMatcherServiceValidationException>(
                matchTask.AsTask);

        actualException.Should()
            .BeEquivalentTo(expectedAllergyIntoleranceMatcherServiceValidationException);
    }

    [Fact]
    public async Task ShouldThrowValidationExceptionOnMatchIfSource2ResourcesIsInvalidAsync()
    {
        // given
        List<JsonElement> source1Resources = new();
        List<JsonElement> invalidSource2Resources = null;
        Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
        Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();

        var invalidArgumentResourceMatcherException =
            new InvalidArgumentResourceMatcherException(
                message: "Resource matcher arguments are invalid. Please correct the errors and try again.");

        invalidArgumentResourceMatcherException.AddData(
            key: "source2Resources",
            values: "List is required.");

        var expectedAllergyIntoleranceMatcherServiceValidationException =
            new AllergyIntoleranceMatcherServiceValidationException(
                message: "Allergy intolerance matcher validation errors occurred, " +
                    "please try again.",
                innerException: invalidArgumentResourceMatcherException);

        // when
        ValueTask<ResourceMatch> matchTask =
            this.allergyIntoleranceMatcherService.MatchAsync(
                source1Resources,
                invalidSource2Resources,
                source1ResourceIndex,
                source2ResourceIndex);

        // then
        AllergyIntoleranceMatcherServiceValidationException actualException =
            await Assert.ThrowsAsync<AllergyIntoleranceMatcherServiceValidationException>(
                matchTask.AsTask);

        actualException.Should()
            .BeEquivalentTo(expectedAllergyIntoleranceMatcherServiceValidationException);
    }

    [Fact]
    public async Task ShouldThrowValidationExceptionOnMatchIfSource1ResourceIndexIsInvalidAsync()
    {
        // given
        List<JsonElement> source1Resources = new();
        List<JsonElement> source2Resources = new();
        Dictionary<string, JsonElement> invalidSource1ResourceIndex = null;
        Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();

        var invalidArgumentResourceMatcherException =
            new InvalidArgumentResourceMatcherException(
                message: "Resource matcher arguments are invalid. Please correct the errors and try again.");

        invalidArgumentResourceMatcherException.AddData(
            key: "source1ResourceIndex",
            values: "Dictionary is required.");

        var expectedAllergyIntoleranceMatcherServiceValidationException =
            new AllergyIntoleranceMatcherServiceValidationException(
                message: "Allergy intolerance matcher validation errors occurred, " +
                    "please try again.",
                innerException: invalidArgumentResourceMatcherException);

        // when
        ValueTask<ResourceMatch> matchTask =
            this.allergyIntoleranceMatcherService.MatchAsync(
                source1Resources,
                source2Resources,
                invalidSource1ResourceIndex,
                source2ResourceIndex);

        // then
        AllergyIntoleranceMatcherServiceValidationException actualException =
            await Assert.ThrowsAsync<AllergyIntoleranceMatcherServiceValidationException>(
                matchTask.AsTask);

        actualException.Should()
            .BeEquivalentTo(expectedAllergyIntoleranceMatcherServiceValidationException);
    }

    [Fact]
    public async Task ShouldThrowValidationExceptionOnMatchIfSource2ResourceIndexIsInvalidAsync()
    {
        // given
        List<JsonElement> source1Resources = new();
        List<JsonElement> source2Resources = new();
        Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
        Dictionary<string, JsonElement> invalidSource2ResourceIndex = null;

        var invalidArgumentResourceMatcherException =
            new InvalidArgumentResourceMatcherException(
                message: "Resource matcher arguments are invalid. Please correct the errors and try again.");

        invalidArgumentResourceMatcherException.AddData(
            key: "source2ResourceIndex",
            values: "Dictionary is required.");

        var expectedAllergyIntoleranceMatcherServiceValidationException =
            new AllergyIntoleranceMatcherServiceValidationException(
                message: "Allergy intolerance matcher validation errors occurred, " +
                    "please try again.",
                innerException: invalidArgumentResourceMatcherException);

        // when
        ValueTask<ResourceMatch> matchTask =
            this.allergyIntoleranceMatcherService.MatchAsync(
                source1Resources,
                source2Resources,
                source1ResourceIndex,
                invalidSource2ResourceIndex);

        // then
        AllergyIntoleranceMatcherServiceValidationException actualException =
            await Assert.ThrowsAsync<AllergyIntoleranceMatcherServiceValidationException>(
                matchTask.AsTask);

        actualException.Should()
            .BeEquivalentTo(expectedAllergyIntoleranceMatcherServiceValidationException);
    }
}