// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers.Exceptions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers.MedicationStatements.Exceptions;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.MedicationStatements
{
    public partial class MedicationStatementMatcherServiceTests
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

            var expectedMedicationStatementMatcherServiceValidationException =
                new MedicationStatementMatcherServiceValidationException(
                    message: "Medication statement matcher validation errors occurred, " +
                        "please try again.",
                    innerException: invalidArgumentResourceMatcherException);

            // when
            ValueTask<string> getMatchKeyTask =
                this.medicationStatementMatcherService.GetMatchKeyAsync(
                    invalidResource,
                    invalidResourceIndex);

            // then
            MedicationStatementMatcherServiceValidationException actualException =
                await Assert.ThrowsAsync<MedicationStatementMatcherServiceValidationException>(
                    getMatchKeyTask.AsTask);

            actualException.Should()
                .BeEquivalentTo(expectedMedicationStatementMatcherServiceValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedMedicationStatementMatcherServiceValidationException))),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}