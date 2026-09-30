// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.FhirRecords;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.FhirRecords
{
    /// <summary>
    /// Cancellation is never wrapped. A caller that cancels gets back the cancellation exception
    /// itself, nothing is logged as an error, and a caller that has already cancelled costs no
    /// round trip to storage.
    /// </summary>
    public partial class FhirRecordServiceTests
    {
        [Theory]
        [MemberData(nameof(CancellationExceptions))]
        public async Task ShouldPassCancellationThroughOnRetrieveFhirRecordByIdAsync(
            Exception cancellationException)
        {
            // given
            Guid randomFhirRecordId = Guid.NewGuid();
            Guid inputFhirRecordId = randomFhirRecordId;

            this.storageBrokerMock.Setup(broker =>
                broker.SelectFhirRecordByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(cancellationException);

            // when
            ValueTask<FhirRecord> retrieveFhirRecordTask =
                this.fhirRecordService.RetrieveFhirRecordByIdAsync(
                    inputFhirRecordId, TestContext.Current.CancellationToken);

            OperationCanceledException actualOperationCanceledException =
                await Assert.ThrowsAnyAsync<OperationCanceledException>(retrieveFhirRecordTask.AsTask);

            // then
            actualOperationCanceledException.Should().BeSameAs(cancellationException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectFhirRecordByIdAsync(inputFhirRecordId, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(CancellationExceptions))]
        public async Task ShouldPassCancellationThroughOnRetrieveAllFhirRecordsAsync(
            Exception cancellationException)
        {
            // given
            this.storageBrokerMock.Setup(broker =>
                broker.SelectAllFhirRecordsAsync(It.IsAny<CancellationToken>()))
                    .ThrowsAsync(cancellationException);

            // when
            ValueTask<IQueryable<FhirRecord>> retrieveAllFhirRecordsTask =
                this.fhirRecordService.RetrieveAllFhirRecordsAsync(TestContext.Current.CancellationToken);

            OperationCanceledException actualOperationCanceledException =
                await Assert.ThrowsAnyAsync<OperationCanceledException>(retrieveAllFhirRecordsTask.AsTask);

            // then
            actualOperationCanceledException.Should().BeSameAs(cancellationException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAllFhirRecordsAsync(It.IsAny<CancellationToken>()),
                    Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(CancellationExceptions))]
        public async Task ShouldPassCancellationThroughOnTryClaimFhirRecordAsync(
            Exception cancellationException)
        {
            // given
            Guid randomFhirRecordId = Guid.NewGuid();
            Guid inputFhirRecordId = randomFhirRecordId;
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();

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
                        .ThrowsAsync(cancellationException);

            // when
            ValueTask<bool> tryClaimFhirRecordTask =
                this.fhirRecordService.TryClaimFhirRecordAsync(
                    inputFhirRecordId,
                    StatusType.Pending,
                    StatusType.Processing,
                    randomDateTimeOffset,
                    isProcessed: false,
                    cancellationToken: TestContext.Current.CancellationToken);

            OperationCanceledException actualOperationCanceledException =
                await Assert.ThrowsAnyAsync<OperationCanceledException>(tryClaimFhirRecordTask.AsTask);

            // then
            actualOperationCanceledException.Should().BeSameAs(cancellationException);

            this.storageBrokerMock.Verify(broker =>
                broker.ClaimFhirRecordAsync(
                    inputFhirRecordId,
                    It.IsAny<StatusType>(),
                    It.IsAny<StatusType>(),
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<DateTimeOffset?>(),
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldPassCancellationThroughOnAddFhirRecordIfAlreadyCancelledAsync()
        {
            // given
            CancellationToken cancelledToken = new CancellationToken(canceled: true);
            FhirRecord randomFhirRecord = CreateRandomFhirRecord();

            // when
            ValueTask<FhirRecord> addFhirRecordTask =
                this.fhirRecordService.AddFhirRecordAsync(randomFhirRecord, cancelledToken);

            OperationCanceledException actualOperationCanceledException =
                await Assert.ThrowsAnyAsync<OperationCanceledException>(addFhirRecordTask.AsTask);

            // then
            actualOperationCanceledException.CancellationToken.Should().Be(cancelledToken);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldPassCancellationThroughOnRetrieveAllFhirRecordsIfAlreadyCancelledAsync()
        {
            // given
            CancellationToken cancelledToken = new CancellationToken(canceled: true);

            // when
            ValueTask<IQueryable<FhirRecord>> retrieveAllFhirRecordsTask =
                this.fhirRecordService.RetrieveAllFhirRecordsAsync(cancelledToken);

            OperationCanceledException actualOperationCanceledException =
                await Assert.ThrowsAnyAsync<OperationCanceledException>(retrieveAllFhirRecordsTask.AsTask);

            // then
            actualOperationCanceledException.CancellationToken.Should().Be(cancelledToken);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldPassCancellationThroughOnRetrieveFhirRecordByIdIfAlreadyCancelledAsync()
        {
            // given
            CancellationToken cancelledToken = new CancellationToken(canceled: true);
            Guid randomFhirRecordId = Guid.NewGuid();

            // when
            ValueTask<FhirRecord> retrieveFhirRecordTask =
                this.fhirRecordService.RetrieveFhirRecordByIdAsync(randomFhirRecordId, cancelledToken);

            OperationCanceledException actualOperationCanceledException =
                await Assert.ThrowsAnyAsync<OperationCanceledException>(retrieveFhirRecordTask.AsTask);

            // then
            actualOperationCanceledException.CancellationToken.Should().Be(cancelledToken);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldPassCancellationThroughOnTryTransitionFhirRecordStatusIfAlreadyCancelledAsync()
        {
            // given
            CancellationToken cancelledToken = new CancellationToken(canceled: true);
            Guid randomFhirRecordId = Guid.NewGuid();

            // when
            ValueTask<bool> tryTransitionTask =
                this.fhirRecordService.TryTransitionFhirRecordStatusAsync(
                    randomFhirRecordId,
                    StatusType.Completed,
                    StatusType.Completed,
                    isProcessed: true,
                    cancelledToken);

            OperationCanceledException actualOperationCanceledException =
                await Assert.ThrowsAnyAsync<OperationCanceledException>(tryTransitionTask.AsTask);

            // then
            actualOperationCanceledException.CancellationToken.Should().Be(cancelledToken);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldPassCancellationThroughOnTryClaimFhirRecordIfAlreadyCancelledAsync()
        {
            // given
            CancellationToken cancelledToken = new CancellationToken(canceled: true);
            Guid randomFhirRecordId = Guid.NewGuid();
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();

            // when
            ValueTask<bool> tryClaimFhirRecordTask =
                this.fhirRecordService.TryClaimFhirRecordAsync(
                    randomFhirRecordId,
                    StatusType.Pending,
                    StatusType.Processing,
                    randomDateTimeOffset,
                    isProcessed: false,
                    cancellationToken: cancelledToken);

            OperationCanceledException actualOperationCanceledException =
                await Assert.ThrowsAnyAsync<OperationCanceledException>(tryClaimFhirRecordTask.AsTask);

            // then
            actualOperationCanceledException.CancellationToken.Should().Be(cancelledToken);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldPassCancellationThroughOnModifyFhirRecordIfAlreadyCancelledAsync()
        {
            // given
            CancellationToken cancelledToken = new CancellationToken(canceled: true);
            FhirRecord randomFhirRecord = CreateRandomFhirRecord();

            // when
            ValueTask<FhirRecord> modifyFhirRecordTask =
                this.fhirRecordService.ModifyFhirRecordAsync(randomFhirRecord, cancelledToken);

            OperationCanceledException actualOperationCanceledException =
                await Assert.ThrowsAnyAsync<OperationCanceledException>(modifyFhirRecordTask.AsTask);

            // then
            actualOperationCanceledException.CancellationToken.Should().Be(cancelledToken);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldPassCancellationThroughOnRemoveFhirRecordByIdIfAlreadyCancelledAsync()
        {
            // given
            CancellationToken cancelledToken = new CancellationToken(canceled: true);
            Guid randomFhirRecordId = Guid.NewGuid();

            // when
            ValueTask<FhirRecord> removeFhirRecordTask =
                this.fhirRecordService.RemoveFhirRecordByIdAsync(randomFhirRecordId, cancelledToken);

            OperationCanceledException actualOperationCanceledException =
                await Assert.ThrowsAnyAsync<OperationCanceledException>(removeFhirRecordTask.AsTask);

            // then
            actualOperationCanceledException.CancellationToken.Should().Be(cancelledToken);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
