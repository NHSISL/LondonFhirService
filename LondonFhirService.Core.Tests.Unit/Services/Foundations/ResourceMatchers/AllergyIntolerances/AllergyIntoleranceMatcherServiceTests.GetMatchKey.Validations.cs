// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using FluentAssertions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.AllergyIntolerances;

public partial class AllergyIntoleranceMatcherServiceTests
{
    [Fact]
    public void ShouldThrowValidationExceptionOnGetMatchKeyIfResourceIsNull()
    {
        // given
        JsonElement nullResource = default;
        Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();

        var nullResourceMatcherException =
            new NullResourceMatcherException(nameof(nullResource));

        var expectedResourceMatcherValidationException =
            new ResourceMatcherValidationException(
                message: "Resource matcher validation error occurred, please fix the errors and try again.",
                innerException: nullResourceMatcherException);

        // when
        Action getMatchKeyAction = () =>
            this.allergyIntoleranceMatcherService.GetMatchKey(nullResource, resourceIndex);

        // then
        ResourceMatcherValidationException actualResourceMatcherValidationException =
            Assert.Throws<ResourceMatcherValidationException>(getMatchKeyAction);

        actualResourceMatcherValidationException.Should()
            .BeEquivalentTo(expectedResourceMatcherValidationException);
    }

    [Fact]
    public void ShouldThrowValidationExceptionOnGetMatchKeyIfResourceIndexIsNull()
    {
        // given
        JsonElement allergyIntoleranceResource =
            CreateAllergyIntoleranceResource(
                snomedCode: "91936005",
                onsetDateTime: "2024-01-01");

        Dictionary<string, JsonElement> nullResourceIndex = null;

        var nullResourceMatcherException =
            new NullResourceMatcherException(nameof(nullResourceIndex));

        var expectedResourceMatcherValidationException =
            new ResourceMatcherValidationException(
                message: "Resource matcher validation error occurred, please fix the errors and try again.",
                innerException: nullResourceMatcherException);

        // when
        Action getMatchKeyAction = () =>
            this.allergyIntoleranceMatcherService.GetMatchKey(allergyIntoleranceResource, nullResourceIndex);

        // then
        ResourceMatcherValidationException actualResourceMatcherValidationException =
            Assert.Throws<ResourceMatcherValidationException>(getMatchKeyAction);

        actualResourceMatcherValidationException.Should()
            .BeEquivalentTo(expectedResourceMatcherValidationException);
    }
}
