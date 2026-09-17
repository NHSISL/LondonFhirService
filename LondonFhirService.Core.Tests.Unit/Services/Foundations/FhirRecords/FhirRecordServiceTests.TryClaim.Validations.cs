// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
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
        public async Task ShouldThrowValidationExceptionOnTryClaimIfIdIsInvalidAndLogItAsync()
        {
            // given
            var invalidFhirRecordId = Guid.Empty;
            StatusType inputExpectedStatus = StatusType.Pending;
            StatusType inputClaimedStatus = StatusType.Processing;

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
            ValueTask<bool> tryClaimFhirRecordTask =
                this.fhirRecordService.TryClaimFhirRecordAsync(
                    invalidFhirRecordId,
                    inputExpectedStatus,
                    inputClaimedStatus,
                    GetRandomDateTimeOffset(),
                    isProcessed: false,
                    cancellationToken: TestContext.Current.CancellationToken);

            FhirRecordValidationException actualFhirRecordValidationException =
                await Assert.ThrowsAsync<FhirRecordValidationException>(
                    tryClaimFhirRecordTask.AsTask);

            // then
            actualFhirRecordValidationException.Should()
                .BeEquivalentTo(expectedFhirRecordValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedFhirRecordValidationException))),
                        Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never);

            // The actor lookup is the first thing past the guard, so proving it never happened is
            // what makes "validated before anything else runs" an assertion rather than a comment.
            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Never);

            // No wildcard stand-in for the claim itself: test-106 forbids It.IsAny in a
            // Validations file, and none of its arguments exist here anyway - validation
            // short-circuits before the clock and the actor are read, which is what would have
            // produced them. storageBrokerMock.VerifyNoOtherCalls() below proves the same thing
            // without inventing values.

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }
    }
}
