// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.FhirRecords;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.FhirRecords
{
    public partial class FhirRecordServiceTests
    {
        [Fact]
        public async Task ShouldReturnTrueOnTryClaimFhirRecordIfOneRowWasChangedAsync()
        {
            // given
            Guid randomFhirRecordId = Guid.NewGuid();
            Guid inputFhirRecordId = randomFhirRecordId;
            StatusType inputExpectedStatus = StatusType.Pending;
            StatusType inputClaimedStatus = StatusType.Processing;
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            DateTimeOffset claimedDate = randomDateTimeOffset;
            DateTimeOffset randomNotUpdatedAfter = GetRandomDateTimeOffset();
            DateTimeOffset? inputNotUpdatedAfter = randomNotUpdatedAfter;
            int changedRowCount = 1;
            bool expectedResult = true;

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.storageBrokerMock.Setup(broker =>
                broker.ClaimFhirRecordAsync(
                    inputFhirRecordId,
                    inputExpectedStatus,
                    inputClaimedStatus,
                    claimedDate,
                    inputNotUpdatedAfter))
                        .ReturnsAsync(changedRowCount);

            // when
            bool actualResult = await this.fhirRecordService.TryClaimFhirRecordAsync(
                inputFhirRecordId,
                inputExpectedStatus,
                inputClaimedStatus,
                inputNotUpdatedAfter);

            // then
            actualResult.Should().Be(expectedResult);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.ClaimFhirRecordAsync(
                    inputFhirRecordId,
                    inputExpectedStatus,
                    inputClaimedStatus,
                    claimedDate,
                    inputNotUpdatedAfter),
                        Times.Once);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnFalseOnTryClaimFhirRecordIfNoRowWasChangedAsync()
        {
            // given
            Guid randomFhirRecordId = Guid.NewGuid();
            Guid inputFhirRecordId = randomFhirRecordId;
            StatusType inputExpectedStatus = StatusType.Processing;
            StatusType inputClaimedStatus = StatusType.Completed;
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            DateTimeOffset claimedDate = randomDateTimeOffset;
            DateTimeOffset randomNotUpdatedAfter = GetRandomDateTimeOffset();
            DateTimeOffset? inputNotUpdatedAfter = randomNotUpdatedAfter;
            int changedRowCount = 0;
            bool expectedResult = false;

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.storageBrokerMock.Setup(broker =>
                broker.ClaimFhirRecordAsync(
                    inputFhirRecordId,
                    inputExpectedStatus,
                    inputClaimedStatus,
                    claimedDate,
                    inputNotUpdatedAfter))
                        .ReturnsAsync(changedRowCount);

            // when
            bool actualResult = await this.fhirRecordService.TryClaimFhirRecordAsync(
                inputFhirRecordId,
                inputExpectedStatus,
                inputClaimedStatus,
                inputNotUpdatedAfter);

            // then
            actualResult.Should().Be(expectedResult);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.ClaimFhirRecordAsync(
                    inputFhirRecordId,
                    inputExpectedStatus,
                    inputClaimedStatus,
                    claimedDate,
                    inputNotUpdatedAfter),
                        Times.Once);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldClaimFhirRecordWithNullNotUpdatedAfterIfItWasNotSuppliedAsync()
        {
            // given
            Guid randomFhirRecordId = Guid.NewGuid();
            Guid inputFhirRecordId = randomFhirRecordId;
            StatusType inputExpectedStatus = StatusType.Pending;
            StatusType inputClaimedStatus = StatusType.Processing;
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            DateTimeOffset claimedDate = randomDateTimeOffset;
            DateTimeOffset? nullNotUpdatedAfter = null;
            int changedRowCount = 1;
            bool expectedResult = true;

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.storageBrokerMock.Setup(broker =>
                broker.ClaimFhirRecordAsync(
                    inputFhirRecordId,
                    inputExpectedStatus,
                    inputClaimedStatus,
                    claimedDate,
                    nullNotUpdatedAfter))
                        .ReturnsAsync(changedRowCount);

            // when
            bool actualResult = await this.fhirRecordService.TryClaimFhirRecordAsync(
                inputFhirRecordId,
                inputExpectedStatus,
                inputClaimedStatus);

            // then
            actualResult.Should().Be(expectedResult);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.ClaimFhirRecordAsync(
                    inputFhirRecordId,
                    inputExpectedStatus,
                    inputClaimedStatus,
                    claimedDate,
                    nullNotUpdatedAfter),
                        Times.Once);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldClaimFhirRecordWithDateTimeBrokerDateOnTryClaimAsync()
        {
            // given
            Guid randomFhirRecordId = Guid.NewGuid();
            Guid inputFhirRecordId = randomFhirRecordId;
            StatusType inputExpectedStatus = StatusType.Failed;
            StatusType inputClaimedStatus = StatusType.Pending;
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            DateTimeOffset expectedClaimedDate = randomDateTimeOffset;
            DateTimeOffset randomNotUpdatedAfter = GetRandomDateTimeOffset();
            DateTimeOffset? inputNotUpdatedAfter = randomNotUpdatedAfter;
            DateTimeOffset actualClaimedDate = default;
            DateTimeOffset? actualNotUpdatedAfter = default;
            int changedRowCount = 1;

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.storageBrokerMock.Setup(broker =>
                broker.ClaimFhirRecordAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<StatusType>(),
                    It.IsAny<StatusType>(),
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<DateTimeOffset?>()))
                        .Callback<Guid, StatusType, StatusType, DateTimeOffset, DateTimeOffset?>(
                            (fhirRecordId, expectedStatus, claimedStatus, claimedDate, notUpdatedAfter) =>
                            {
                                actualClaimedDate = claimedDate;
                                actualNotUpdatedAfter = notUpdatedAfter;
                            })
                        .ReturnsAsync(changedRowCount);

            // when
            await this.fhirRecordService.TryClaimFhirRecordAsync(
                inputFhirRecordId,
                inputExpectedStatus,
                inputClaimedStatus,
                inputNotUpdatedAfter);

            // then
            actualClaimedDate.Should().Be(expectedClaimedDate);
            actualNotUpdatedAfter.Should().Be(inputNotUpdatedAfter);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.ClaimFhirRecordAsync(
                    inputFhirRecordId,
                    inputExpectedStatus,
                    inputClaimedStatus,
                    expectedClaimedDate,
                    inputNotUpdatedAfter),
                        Times.Once);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
