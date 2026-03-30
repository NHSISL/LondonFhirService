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
    public void ShouldThrowValidationExceptionOnGetMatchKeyIfResourceIsNull()
    {
        // given
        JsonElement nullResource = default;
        Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();

        var nullConditionException =
            new NullConditionException(nameof(nullResource));

        var expectedConditionValidationException =
            new ConditionValidationException(
                message: "Allergy intolerance matcher validation error occurred, please fix the errors and try again.",
                innerException: nullConditionException);

        // when
        Action getMatchKeyAction = () =>
            this.conditionMatcherService.GetMatchKey(nullResource, resourceIndex);

        // then
        ConditionValidationException actualConditionValidationException =
            Assert.Throws<ConditionValidationException>(getMatchKeyAction);

        actualConditionValidationException.Should()
            .BeEquivalentTo(expectedConditionValidationException);
    }

    [Fact]
    public void ShouldThrowValidationExceptionOnGetMatchKeyIfResourceIndexIsNull()
    {
        // given
        JsonElement conditionResource =
            CreateConditionResource(
                snomedCode: "91936005",
                onsetDateTime: "2024-01-01");

        Dictionary<string, JsonElement> nullResourceIndex = null;

        var NullConditionException =
            new NullConditionException(nameof(nullResourceIndex));

        var expectedConditionValidationException =
            new ConditionValidationException(
                message: "allergy intolerance validation error occurred, please fix the errors and try again.",
                innerException: NullConditionException);

        // when
        Action getMatchKeyAction = () =>
            this.conditionMatcherService.GetMatchKey(conditionResource, nullResourceIndex);

        // then
        ConditionValidationException actualConditionValidationException =
            Assert.Throws<ConditionValidationException>(getMatchKeyAction);

        actualConditionValidationException.Should()
            .BeEquivalentTo(expectedConditionValidationException);
    }
}
