// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers.Exceptions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers.Patients.Exceptions;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Patients
{
    public partial class PatientMatcherServiceTests
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

            var expectedPatientMatcherServiceValidationException =
                new PatientMatcherServiceValidationException(
                    message: "Patient matcher validation errors occurred, " +
                        "please try again.",
                    innerException: invalidArgumentResourceMatcherException);

            // when
            ValueTask<string> getMatchKeyTask =
                this.patientMatcherService.GetMatchKeyAsync(
                    invalidResource,
                    invalidResourceIndex);

            // then
            PatientMatcherServiceValidationException actualException =
                await Assert.ThrowsAsync<PatientMatcherServiceValidationException>(
                    getMatchKeyTask.AsTask);

            actualException.Should()
                .BeEquivalentTo(expectedPatientMatcherServiceValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedPatientMatcherServiceValidationException))),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}