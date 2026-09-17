// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.FhirRecords;
using LondonFhirService.Core.Models.Foundations.FhirRecords.Exceptions;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.FhirRecords
{
    public partial class FhirRecordServiceTests
    {
        [Fact]
        public async Task ShouldThrowValidationExceptionOnTryTransitionIfIdIsInvalidAndLogItAsync()
        {
            // given
            var invalidFhirRecordId = Guid.Empty;

            var invalidFhirRecordException =
                new InvalidFhirRecordException(
                    message: "Invalid fhirRecord. Please correct the errors and try again.");

            invalidFhirRecordException.AddData(
                key: nameof(FhirRecord.Id),
                values: "Id is required");

            var expectedFhirRecordValidationException =
                new FhirRecordValidationException(
                    message: "FhirRecord validation errors occurred, please try again.",
                    innerException: invalidFhirRecordException);

            // when
            ValueTask<bool> tryTransitionFhirRecordTask =
                this.fhirRecordService.TryTransitionFhirRecordStatusAsync(
                    invalidFhirRecordId,
                    excludedStatus: StatusType.Completed,
                    newStatus: StatusType.Completed,
                    isProcessed: true,
                    cancellationToken: TestContext.Current.CancellationToken);

            FhirRecordValidationException actualFhirRecordValidationException =
                await Assert.ThrowsAsync<FhirRecordValidationException>(
                    tryTransitionFhirRecordTask.AsTask);

            // then
            // Validated before the clock is read and before anything reaches storage, so an invalid
            // id never turns into a statement that would match every row but the excluded status.
            actualFhirRecordValidationException.Should()
                .BeEquivalentTo(expectedFhirRecordValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedFhirRecordValidationException))),
                        Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
        }
    }
}
