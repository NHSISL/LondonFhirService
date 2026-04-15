// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers.Exceptions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers.Medications.Exceptions;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Medications
{
    public partial class MedicationMatcherServiceTests
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

            var expectedMedicationMatcherServiceValidationException =
                new MedicationMatcherServiceValidationException(
                    message: "Medication matcher validation errors occurred, " +
                        "please try again.",
                    innerException: invalidArgumentResourceMatcherException);

            // when
            ValueTask<string> getMatchKeyTask =
                this.medicationMatcherService.GetMatchKeyAsync(
                    invalidResource,
                    invalidResourceIndex);

            // then
            MedicationMatcherServiceValidationException actualException =
                await Assert.ThrowsAsync<MedicationMatcherServiceValidationException>(
                    getMatchKeyTask.AsTask);

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