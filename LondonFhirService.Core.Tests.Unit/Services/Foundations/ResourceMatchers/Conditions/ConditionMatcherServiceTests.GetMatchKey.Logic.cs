// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using FluentAssertions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Conditions.Conditions;

public partial class ConditionMatcherServiceTests
{
    [Fact]
    public void ShouldReturnResourceType() =>
        this.conditionMatcherService.ResourceType.Should().Be("Condition");

    [Fact]
    public void ShouldGetMatchKey()
    {
        // given
        JsonElement conditionResource =
            CreateConditionResource(
                snomedCode: "91936005",
                onsetDateTime: "2024-01-01");

        Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();
        string expectedMatchKey = "91936005|2024-01-01";

        // when
        string? actualMatchKey = this.conditionMatcherService.GetMatchKey(
            conditionResource,
            resourceIndex);

        // then
        actualMatchKey.Should().Be(expectedMatchKey);
    }

    [Fact]
    public void ShouldReturnNullMatchKeyIfSnomedCodeDoesNotExist()
    {
        // given
        JsonElement conditionResource =
            CreateNonSnomedConditionResource(onsetDateTime: "2024-01-01");

        Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();

        // when
        string? actualMatchKey = this.conditionMatcherService.GetMatchKey(
            conditionResource,
            resourceIndex);

        // then
        actualMatchKey.Should().BeNull();
    }

    [Fact]
    public void ShouldReturnNullMatchKeyIfOnsetDateTimeDoesNotExist()
    {
        // given
        JsonElement conditionResource =
            CreateResourceWithoutOnsetDateTime(snomedCode: "91936005");

        Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();

        // when
        string? actualMatchKey = this.conditionMatcherService.GetMatchKey(
            conditionResource,
            resourceIndex);

        // then
        actualMatchKey.Should().BeNull();
    }
}
