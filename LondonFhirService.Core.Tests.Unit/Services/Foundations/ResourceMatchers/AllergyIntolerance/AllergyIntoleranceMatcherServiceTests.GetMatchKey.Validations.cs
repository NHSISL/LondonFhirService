// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers.AllergyIntolerances.Exceptions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers.Exceptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.AllergyIntolerances;

public partial class AllergyIntoleranceMatcherServiceTests
{
    [Fact]
    public async Task ShouldThrowValidationExceptionOnGetMatchKeyIfResourceIsInvalidAsync()
    {
        // given
        JsonElement invalidResource = default;
        Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();

        var invalidArgumentResourceMatcherException =
            new InvalidArgumentResourceMatcherException(
                message: "Resource matcher arguments are invalid. Please correct the errors and try again.");

        invalidArgumentResourceMatcherException.UpsertDataList(
            key: nameof(invalidResource),
            value: "Json element is invalid.");

        var expectedAllergyIntoleranceMatcherServiceValidationException =
            new AllergyIntoleranceMatcherServiceValidationException(
                message: "Allergy intolerance service matcher validation error occurred, " +
                    "please fix the errors and try again.",
                innerException: invalidArgumentResourceMatcherException);

        // when
        ValueTask<string> getMatchKeyTask =
            this.allergyIntoleranceMatcherService.GetMatchKeyAsync(
                invalidResource,
                resourceIndex);

        // then
        AllergyIntoleranceMatcherServiceValidationException actualException =
            await Assert.ThrowsAsync<AllergyIntoleranceMatcherServiceValidationException>(
                getMatchKeyTask.AsTask);

        actualException.Should().BeEquivalentTo(expectedAllergyIntoleranceMatcherServiceValidationException);
    }

    [Fact]
    public async Task ShouldThrowValidationExceptionOnGetMatchKeyIfResourceIndexIsInvalidAsync()
    {
        // given
        JsonElement resource =
            CreateAllergyIntoleranceResource(
                snomedCode: "91936005",
                onsetDateTime: "2024-01-01");

        Dictionary<string, JsonElement> invalidResourceIndex = null;

        var invalidArgumentResourceMatcherException =
            new InvalidArgumentResourceMatcherException(
                message: "Resource matcher arguments are invalid. Please correct the errors and try again.");

        invalidArgumentResourceMatcherException.UpsertDataList(
            key: nameof(invalidResourceIndex),
            value: "Dictionary is required.");

        var expectedAllergyIntoleranceMatcherServiceValidationException =
            new AllergyIntoleranceMatcherServiceValidationException(
                message: "Allergy intolerance service matcher validation error occurred, " +
                    "please fix the errors and try again.",
                innerException: invalidArgumentResourceMatcherException);

        // when
        ValueTask<string> getMatchKeyTask =
            this.allergyIntoleranceMatcherService.GetMatchKeyAsync(
                resource,
                invalidResourceIndex);

        // then
        AllergyIntoleranceMatcherServiceValidationException actualException =
            await Assert.ThrowsAsync<AllergyIntoleranceMatcherServiceValidationException>(
                getMatchKeyTask.AsTask);

        actualException.Should().BeEquivalentTo(expectedAllergyIntoleranceMatcherServiceValidationException);
    }
}
