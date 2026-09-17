// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
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
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    inputNotUpdatedAfter,
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(changedRowCount);

            // when
            bool actualResult = await this.fhirRecordService.TryClaimFhirRecordAsync(
                inputFhirRecordId,
                inputExpectedStatus,
                inputClaimedStatus,
                claimedDate,
                false,
                inputNotUpdatedAfter,
                cancellationToken: TestContext.Current.CancellationToken);

            // then
            actualResult.Should().Be(expectedResult);

            this.storageBrokerMock.Verify(broker =>
                broker.ClaimFhirRecordAsync(
                    inputFhirRecordId,
                    inputExpectedStatus,
                    inputClaimedStatus,
                    claimedDate,
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    inputNotUpdatedAfter,
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            // Fetched and put into the statement by hand, because ExecuteUpdateAsync goes
            // round the change tracker and nothing else would stamp the actor on an
            // IAuditable row whose UpdatedDate this call moves.
            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
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
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    inputNotUpdatedAfter,
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(changedRowCount);

            // when
            bool actualResult = await this.fhirRecordService.TryClaimFhirRecordAsync(
                inputFhirRecordId,
                inputExpectedStatus,
                inputClaimedStatus,
                claimedDate,
                false,
                inputNotUpdatedAfter,
                cancellationToken: TestContext.Current.CancellationToken);

            // then
            actualResult.Should().Be(expectedResult);

            this.storageBrokerMock.Verify(broker =>
                broker.ClaimFhirRecordAsync(
                    inputFhirRecordId,
                    inputExpectedStatus,
                    inputClaimedStatus,
                    claimedDate,
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    inputNotUpdatedAfter,
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            // Fetched and put into the statement by hand, because ExecuteUpdateAsync goes
            // round the change tracker and nothing else would stamp the actor on an
            // IAuditable row whose UpdatedDate this call moves.
            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
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
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    nullNotUpdatedAfter,
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(changedRowCount);

            // when
            bool actualResult = await this.fhirRecordService.TryClaimFhirRecordAsync(
                inputFhirRecordId,
                inputExpectedStatus,
                inputClaimedStatus,
                claimedDate,
                isProcessed: false,
                cancellationToken: TestContext.Current.CancellationToken);

            // then
            actualResult.Should().Be(expectedResult);

            this.storageBrokerMock.Verify(broker =>
                broker.ClaimFhirRecordAsync(
                    inputFhirRecordId,
                    inputExpectedStatus,
                    inputClaimedStatus,
                    claimedDate,
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    nullNotUpdatedAfter,
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            // Fetched and put into the statement by hand, because ExecuteUpdateAsync goes
            // round the change tracker and nothing else would stamp the actor on an
            // IAuditable row whose UpdatedDate this call moves.
            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
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
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<DateTimeOffset?>(),
                    It.IsAny<CancellationToken>()))
                        .Callback<Guid, StatusType, StatusType, DateTimeOffset, string, bool,
                            DateTimeOffset?, CancellationToken>(
                            (fhirRecordId, expectedStatus, claimedStatus, claimedDate, claimedBy,
                                isProcessed, notUpdatedAfter, _) =>
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
                expectedClaimedDate,
                false,
                inputNotUpdatedAfter,
                cancellationToken: TestContext.Current.CancellationToken);

            // then
            // Forwarded unchanged. The caller owns this value because it is also its lease token:
            // re-asserting the claim with it is what proves the row was not taken back.
            actualClaimedDate.Should().Be(expectedClaimedDate);
            actualNotUpdatedAfter.Should().Be(inputNotUpdatedAfter);

            this.storageBrokerMock.Verify(broker =>
                broker.ClaimFhirRecordAsync(
                    inputFhirRecordId,
                    inputExpectedStatus,
                    inputClaimedStatus,
                    expectedClaimedDate,
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    inputNotUpdatedAfter,
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            // Fetched and put into the statement by hand, because ExecuteUpdateAsync goes
            // round the change tracker and nothing else would stamp the actor on an
            // IAuditable row whose UpdatedDate this call moves.
            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
