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
                    inputClaimedStatus);

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

            this.storageBrokerMock.Verify(broker =>
                broker.ClaimFhirRecordAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<StatusType>(),
                    It.IsAny<StatusType>(),
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<DateTimeOffset?>()),
                        Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }
    }
}
