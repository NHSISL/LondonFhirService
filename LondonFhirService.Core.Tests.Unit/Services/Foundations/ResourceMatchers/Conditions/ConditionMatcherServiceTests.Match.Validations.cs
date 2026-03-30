// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text.Json;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.Conditions.Conditions.Exceptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Conditions.Conditions;

public partial class ConditionMatcherServiceTests
{
    [Fact]
    public void ShouldThrowValidationExceptionOnMatchIfSource1ResourcesIsNull()
    {
        // given
        List<JsonElement> nullSource1Resources = null;
        List<JsonElement> source2Resources = [];
        Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
        Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();

        var nullConditionException =
            new NullConditionException(nameof(nullSource1Resources));

        var expectedConditionValidationException =
            new ConditionValidationException(
                message: "allergy intolerance validation error occurred, please fix the errors and try again.",
                innerException: nullConditionException);

        // when
        Action matchAction = () =>
            this.conditionMatcherService.Match(
                nullSource1Resources,
                source2Resources,
                source1ResourceIndex,
                source2ResourceIndex);

        // then
        ConditionValidationException actualConditionValidationException =
            Assert.Throws<ConditionValidationException>(matchAction);

        actualConditionValidationException.Should()
            .BeEquivalentTo(expectedConditionValidationException);
    }

    [Fact]
    public void ShouldThrowValidationExceptionOnMatchIfSource2ResourceIndexIsNull()
    {
        // given
        List<JsonElement> source1Resources = [];
        List<JsonElement> source2Resources = [];
        Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
        Dictionary<string, JsonElement> nullSource2ResourceIndex = null;

        var NullConditionException =
            new NullConditionException(nameof(nullSource2ResourceIndex));

        var expectedConditionValidationException =
            new ConditionValidationException(
                message: "allergy intolerance validation error occurred, please fix the errors and try again.",
                innerException: NullConditionException);

        // when
        Action matchAction = () =>
            this.conditionMatcherService.Match(
                source1Resources,
                source2Resources,
                source1ResourceIndex,
                nullSource2ResourceIndex);

        // then
        ConditionValidationException actualConditionValidationException =
            Assert.Throws<ConditionValidationException>(matchAction);

        actualConditionValidationException.Should()
            .BeEquivalentTo(expectedConditionValidationException);
    }
}
