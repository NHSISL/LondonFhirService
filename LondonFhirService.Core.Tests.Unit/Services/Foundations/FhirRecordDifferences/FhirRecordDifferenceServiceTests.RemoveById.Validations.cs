// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.FhirRecordDifferences;
using LondonFhirService.Core.Models.Foundations.FhirRecordDifferences.Exceptions;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.FhirRecordDifferences
{
    public partial class FhirRecordDifferenceServiceTests
    {
        [Fact]
        public async Task ShouldThrowValidationExceptionOnRemoveIfIdIsInvalidAndLogItAsync()
        {
            // given
            Guid invalidFhirRecordDifferenceId = Guid.Empty;

            var invalidFhirRecordDifferenceException =
                new InvalidFhirRecordDifferenceException(
                    message: "Invalid fhirRecordDifference. Please correct the errors and try again.");

            invalidFhirRecordDifferenceException.AddData(
                key: nameof(FhirRecordDifference.Id),
                values: "Id is required");

            var expectedFhirRecordDifferenceValidationException =
                new FhirRecordDifferenceValidationException(
                    message: "FhirRecordDifference validation errors occurred, please try again.",
                    innerException: invalidFhirRecordDifferenceException);

            // when
            ValueTask<FhirRecordDifference> removeFhirRecordDifferenceByIdTask =
                this.fhirRecordDifferenceService.RemoveFhirRecordDifferenceByIdAsync(invalidFhirRecordDifferenceId);

            FhirRecordDifferenceValidationException actualFhirRecordDifferenceValidationException =
                await Assert.ThrowsAsync<FhirRecordDifferenceValidationException>(
                    removeFhirRecordDifferenceByIdTask.AsTask);

            // then
            actualFhirRecordDifferenceValidationException.Should()
                .BeEquivalentTo(expectedFhirRecordDifferenceValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedFhirRecordDifferenceValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.DeleteFhirRecordDifferenceAsync(It.IsAny<FhirRecordDifference>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }
    }
}