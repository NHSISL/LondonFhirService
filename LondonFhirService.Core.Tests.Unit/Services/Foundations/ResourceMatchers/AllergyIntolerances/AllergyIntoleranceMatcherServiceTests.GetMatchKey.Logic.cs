// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using FluentAssertions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.AllergyIntolerances.AllergyIntolerances;

public partial class AllergyIntoleranceMatcherServiceTests
{
    [Fact]
    public void ShouldReturnResourceType() =>
        this.allergyIntoleranceMatcherService.ResourceType.Should().Be("AllergyIntolerance");

    [Fact]
    public void ShouldGetMatchKey()
    {
        // given
        JsonElement allergyIntoleranceResource =
            CreateAllergyIntoleranceResource(
                snomedCode: "91936005",
                onsetDateTime: "2024-01-01");

        Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();
        string expectedMatchKey = "91936005|2024-01-01";

        // when
        string? actualMatchKey = this.allergyIntoleranceMatcherService.GetMatchKey(
            allergyIntoleranceResource,
            resourceIndex);

        // then
        actualMatchKey.Should().Be(expectedMatchKey);
    }

    [Fact]
    public void ShouldReturnNullMatchKeyIfSnomedCodeDoesNotExist()
    {
        // given
        JsonElement allergyIntoleranceResource =
            CreateNonSnomedAllergyIntoleranceResource(onsetDateTime: "2024-01-01");

        Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();

        // when
        string? actualMatchKey = this.allergyIntoleranceMatcherService.GetMatchKey(
            allergyIntoleranceResource,
            resourceIndex);

        // then
        actualMatchKey.Should().BeNull();
    }

    [Fact]
    public void ShouldReturnNullMatchKeyIfOnsetDateTimeDoesNotExist()
    {
        // given
        JsonElement allergyIntoleranceResource =
            CreateResourceWithoutOnsetDateTime(snomedCode: "91936005");

        Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();

        // when
        string? actualMatchKey = this.allergyIntoleranceMatcherService.GetMatchKey(
            allergyIntoleranceResource,
            resourceIndex);

        // then
        actualMatchKey.Should().BeNull();
    }
}
