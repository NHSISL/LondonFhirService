// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.AllergyIntolerances.AllergyIntolerances.Exceptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.AllergyIntolerances.AllergyIntolerances;

public partial class AllergyIntoleranceMatcherServiceTests
{
    [Fact]
    public async Task ShouldThrowServiceExceptionOnMatchIfServiceErrorOccurs()
    {
        // given
        string snomedCode = "12345";
        string onsetDateTime = "2024-01-01";

        JsonElement duplicateResource1 = CreateAllergyIntoleranceResource(
            snomedCode,
            onsetDateTime,
            "allergy-1");

        JsonElement duplicateResource2 = CreateAllergyIntoleranceResource(
            snomedCode,
            onsetDateTime,
            "allergy-2");

        List<JsonElement> source1Resources = [duplicateResource1, duplicateResource2];
        List<JsonElement> source2Resources = [];
        Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
        Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();

        // when
        Action matchAction = () =>
            this.allergyIntoleranceMatcherService.Match(
                source1Resources,
                source2Resources,
                source1ResourceIndex,
                source2ResourceIndex);

        // then
        AllergyIntoleranceServiceException actualAllergyIntoleranceServiceException =
            Assert.Throws<AllergyIntoleranceServiceException>(matchAction);

        actualAllergyIntoleranceServiceException.Should().BeOfType<AllergyIntoleranceServiceException>();

        actualAllergyIntoleranceServiceException.Message.Should()
            .Be("Allergy intolerance service error occurred, contact support.");

        actualAllergyIntoleranceServiceException.InnerException.Should()
            .BeOfType<FailedAllergyIntolerancesServiceException>();

        actualAllergyIntoleranceServiceException.InnerException.Message.Should()
            .Be("Failed allergy intolerance service error occurred, contact support.");

        actualAllergyIntoleranceServiceException.InnerException.InnerException.Should()
            .BeOfType<ArgumentException>();
    }

}
