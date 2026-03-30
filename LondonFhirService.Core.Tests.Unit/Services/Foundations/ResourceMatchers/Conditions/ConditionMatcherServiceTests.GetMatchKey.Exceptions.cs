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
    public void ShouldThrowServiceExceptionOnGetMatchKeyIfServiceErrorOccurs()
    {
        // given
        JsonElement malformedConditionResource = CreateMalformedCodingResource();
        Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();
        var invalidOperationException = new InvalidOperationException(
            "The requested operation requires an element of type 'Array', but the target element has type 'Object'.");

        var failedConditionServiceException =
            new FailedConditionsServiceException(
                message: "Failed allergy intolerance service error occurred, contact support.",
                innerException: invalidOperationException);

        var expectedConditionServiceException =
            new ConditionServiceException(
                message: "Allergy intolerance service error occurred, contact support.",
                innerException: failedConditionServiceException);

        // when
        Action getMatchKeyAction = () =>
            this.conditionMatcherService.GetMatchKey(malformedConditionResource, resourceIndex);

        // then
        ConditionServiceException actualConditionServiceException =
            Assert.Throws<ConditionServiceException>(getMatchKeyAction);

        actualConditionServiceException.Should()
            .BeEquivalentTo(expectedConditionServiceException);
    }
}
