// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers.Exceptions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers.Medications.Exceptions;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Medications
{
    public partial class MedicationMatcherServiceTests
    {
        [Fact]
        public async Task ShouldThrowValidationExceptionOnMatchIfSource1ResourcesIsInvalidAsync()
        {
            // given
            List<JsonElement> invalidSource1Resources = null;
            List<JsonElement> invalidSource2Resources = null;
            Dictionary<string, JsonElement> invalidSource1ResourceIndex = null;
            Dictionary<string, JsonElement> invalidSource2ResourceIndex = null;

            var invalidArgumentResourceMatcherException =
                new InvalidArgumentResourceMatcherException(
                    message: "Resource matcher arguments are invalid. Please correct the errors and try again.");

            invalidArgumentResourceMatcherException.AddData(
                key: "source1Resources",
                values: "List is required.");

            invalidArgumentResourceMatcherException.AddData(
                key: "source2Resources",
                values: "List is required.");

            invalidArgumentResourceMatcherException.AddData(
                key: "source1ResourceIndex",
                values: "Dictionary is required.");

            invalidArgumentResourceMatcherException.AddData(
                key: "source2ResourceIndex",
                values: "Dictionary is required.");

            var expectedMedicationMatcherServiceValidationException =
                new MedicationMatcherServiceValidationException(
                    message: "Medication matcher validation errors occurred, " +
                        "please try again.",
                    innerException: invalidArgumentResourceMatcherException);

            // when
            ValueTask<ResourceMatch> matchTask =
                this.medicationMatcherService.MatchAsync(
                    invalidSource1Resources,
                    invalidSource2Resources,
                    invalidSource1ResourceIndex,
                    invalidSource2ResourceIndex);

            // then
            MedicationMatcherServiceValidationException actualException =
                await Assert.ThrowsAsync<MedicationMatcherServiceValidationException>(
                    matchTask.AsTask);

            actualException.Should()
                .BeEquivalentTo(expectedMedicationMatcherServiceValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedMedicationMatcherServiceValidationException))),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}