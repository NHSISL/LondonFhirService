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
    public void ShouldThrowServiceExceptionOnMatchIfServiceErrorOccurs()
    {
        JsonElement malformedListResource = CreateMalformedTitleResource();
        List<JsonElement> source1Resources = [malformedListResource];
        List<JsonElement> source2Resources = [];
        Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
        Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();

        var invalidOperationException = new InvalidOperationException(
            "The requested operation requires an element of type 'String', but the target element has type 'Object'.");

        var failedListMatcherServiceException =
            new FailedListMatcherServiceException(
                message: "Failed List matcher service error occurred, contact support.",
                innerException: invalidOperationException);

        var expectedListMatcherServiceException =
            new ListMatcherServiceException(
                message: "List matcher service error occurred, contact support.",
                innerException: failedListMatcherServiceException);

        Action matchAction = () =>
            this.listMatcherService.Match(
                source1Resources,
                source2Resources,
                source1ResourceIndex,
                source2ResourceIndex);

        ListMatcherServiceException actualListMatcherServiceException =
            Assert.Throws<ListMatcherServiceException>(matchAction);

        actualListMatcherServiceException.Should()
            .BeEquivalentTo(expectedListMatcherServiceException);
    }
}