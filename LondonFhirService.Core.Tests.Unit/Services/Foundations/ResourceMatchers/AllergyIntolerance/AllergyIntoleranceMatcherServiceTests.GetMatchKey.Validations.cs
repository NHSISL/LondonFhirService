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
        Dictionary<string, JsonElement> invalidResourceIndex = null;

        var invalidArgumentResourceMatcherException =
            new InvalidArgumentResourceMatcherException(
                message:
                    "Resource matcher arguments are invalid. " +
                    "Please correct the errors and try again.");

        invalidArgumentResourceMatcherException.AddData(
            key: "resource",
            values: "Json element is invalid.");

        invalidArgumentResourceMatcherException.UpsertDataList(
            key: "resourceIndex",
            value: "Dictionary is required.");

        var expectedAllergyIntoleranceMatcherServiceValidationException =
            new AllergyIntoleranceMatcherServiceValidationException(
                message: "Allergy intolerance matcher validation errors occurred, " +
                    "please try again.",
                innerException: invalidArgumentResourceMatcherException);

        // when
        ValueTask<string> getMatchKeyTask =
            this.allergyIntoleranceMatcherService.GetMatchKeyAsync(
                invalidResource,
                invalidResourceIndex);

        // then
        AllergyIntoleranceMatcherServiceValidationException actualException =
            await Assert.ThrowsAsync<AllergyIntoleranceMatcherServiceValidationException>(
                getMatchKeyTask.AsTask);

        actualException.Should()
            .BeEquivalentTo(expectedAllergyIntoleranceMatcherServiceValidationException);

        this.loggingBrokerMock.VerifyNoOtherCalls();
    }
}
