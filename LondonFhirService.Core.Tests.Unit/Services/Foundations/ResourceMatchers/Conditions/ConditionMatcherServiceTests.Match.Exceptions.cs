// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.Conditions.Conditions.Exceptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Conditions.Conditions;

public partial class ConditionMatcherServiceTests
{
    [Fact]
    public async Task ShouldThrowServiceExceptionOnMatchIfServiceErrorOccurs()
    {
        // given
        string snomedCode = "12345";
        string onsetDateTime = "2024-01-01";

        JsonElement duplicateResource1 = CreateConditionResource(
            snomedCode,
            onsetDateTime,
            "allergy-1");

        JsonElement duplicateResource2 = CreateConditionResource(
            snomedCode,
            onsetDateTime,
            "allergy-2");

        List<JsonElement> source1Resources = [duplicateResource1, duplicateResource2];
        List<JsonElement> source2Resources = [];
        Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
        Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();

        // when
        Action matchAction = () =>
            this.conditionMatcherService.Match(
                source1Resources,
                source2Resources,
                source1ResourceIndex,
                source2ResourceIndex);

        // then
        ConditionServiceException actualConditionServiceException =
            Assert.Throws<ConditionServiceException>(matchAction);

        actualConditionServiceException.Should().BeOfType<ConditionServiceException>();

        actualConditionServiceException.Message.Should()
            .Be("Allergy intolerance service error occurred, contact support.");

        actualConditionServiceException.InnerException.Should()
            .BeOfType<FailedConditionsServiceException>();

        actualConditionServiceException.InnerException.Message.Should()
            .Be("Failed allergy intolerance service error occurred, contact support.");

        actualConditionServiceException.InnerException.InnerException.Should()
            .BeOfType<ArgumentException>();
    }

}
