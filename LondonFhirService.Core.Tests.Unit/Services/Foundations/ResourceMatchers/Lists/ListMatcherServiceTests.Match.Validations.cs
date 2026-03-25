// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text.Json;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers.Exceptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Lists;

public partial class ListMatcherServiceTests
{
    [Fact]
    public void ShouldThrowValidationExceptionOnMatchIfArgumentsAreInvalid()
    {
        List<JsonElement> invalidSource1Resources = null;
        List<JsonElement> invalidSource2Resources = null;
        Dictionary<string, JsonElement> invalidSource1ResourceIndex = null;
        Dictionary<string, JsonElement> invalidSource2ResourceIndex = null;

        var invalidListMatcherServiceException =
            new InvalidListMatcherServiceException();

        invalidListMatcherServiceException.UpsertDataList(nameof(invalidSource1Resources), "Value is required");
        invalidListMatcherServiceException.UpsertDataList(nameof(invalidSource2Resources), "Value is required");
        invalidListMatcherServiceException.UpsertDataList(nameof(invalidSource1ResourceIndex), "Value is required");
        invalidListMatcherServiceException.UpsertDataList(nameof(invalidSource2ResourceIndex), "Value is required");

        var expectedListMatcherServiceValidationException =
            new ListMatcherServiceValidationException(
                message: "List matcher service validation error occurred, please fix the errors and try again.",
                innerException: invalidListMatcherServiceException);

        Action matchAction = () =>
            this.listMatcherService.Match(
                invalidSource1Resources,
                invalidSource2Resources,
                invalidSource1ResourceIndex,
                invalidSource2ResourceIndex);

        ListMatcherServiceValidationException actualListMatcherServiceValidationException =
            Assert.Throws<ListMatcherServiceValidationException>(matchAction);

        actualListMatcherServiceValidationException.Should()
            .BeEquivalentTo(expectedListMatcherServiceValidationException);
    }
}