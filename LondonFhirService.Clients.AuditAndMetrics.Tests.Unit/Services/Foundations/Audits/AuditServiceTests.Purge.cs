// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Clients.AuditAndMetrics.Models.Audits.Exceptions;
using LondonFhirService.Clients.AuditAndMetrics.Models.Configurations;
using Moq;

namespace LondonFhirService.Clients.AuditAndMetrics.Tests.Unit.Services.Foundations.Audits
{
    /// <summary>
    /// The audit retention sweep. It mirrors the metric one because the failure that matters is
    /// the same on both: a retention period of zero or less puts the cut off at or after the
    /// present and takes the whole table. On audits that is the worse of the two to get wrong.
    /// </summary>
    public partial class AuditServiceTests
    {
        [Fact]
        public async Task ShouldPurgeAuditsOlderThanTheRetentionPeriodAsync()
        {
            // given
            DateTimeOffset currentDateTimeOffset = GetRandomDateTimeOffset();
            int retentionPeriodInDays = GetRandomNumber();
            int batchSize = GetRandomNumber();
            int deletedCount = batchSize - 1;
            this.auditServiceConfigurations.AuditRetentionPeriodInDays = retentionPeriodInDays;
            this.auditServiceConfigurations.PurgeBatchSize = batchSize;
            DateTimeOffset expectedCutOffDate = currentDateTimeOffset.AddDays(-retentionPeriodInDays);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(currentDateTimeOffset);

            this.storageBrokerMock.Setup(broker =>
                broker.DeleteAuditsOlderThanAsync(
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(deletedCount);

            // when
            int actualPurgedCount =
                await this.auditService.PurgeAuditsOlderThanRetentionPeriodAsync(
                    TestContext.Current.CancellationToken);

            // then
            actualPurgedCount.Should().Be(deletedCount);

            // The cut off is derived from the retention period, and the delete runs in the
            // database rather than over a materialised candidate list.
            this.storageBrokerMock.Verify(broker =>
                broker.DeleteAuditsOlderThanAsync(
                    expectedCutOffDate,
                    batchSize,
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAllAuditsAsync(It.IsAny<CancellationToken>()),
                    Times.Never);
        }

        [Fact]
        public async Task ShouldKeepDeletingUntilABatchComesBackShortAsync()
        {
            // given
            // A full batch means there may be more behind it. Stopping after one would leave a
            // table that has never been purged taking as many sweeps as it has batches.
            DateTimeOffset currentDateTimeOffset = GetRandomDateTimeOffset();
            int batchSize = GetRandomNumber();
            int finalPartialBatch = batchSize - 1;
            this.auditServiceConfigurations.AuditRetentionPeriodInDays = GetRandomNumber();
            this.auditServiceConfigurations.PurgeBatchSize = batchSize;

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(currentDateTimeOffset);

            this.storageBrokerMock.SetupSequence(broker =>
                broker.DeleteAuditsOlderThanAsync(
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(batchSize)
                        .ReturnsAsync(batchSize)
                        .ReturnsAsync(finalPartialBatch);

            // when
            int actualPurgedCount =
                await this.auditService.PurgeAuditsOlderThanRetentionPeriodAsync(
                    TestContext.Current.CancellationToken);

            // then
            actualPurgedCount.Should().Be((batchSize * 2) + finalPartialBatch);

            this.storageBrokerMock.Verify(broker =>
                broker.DeleteAuditsOlderThanAsync(
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()),
                        Times.Exactly(3));
        }

        [Fact]
        public async Task ShouldNotPurgeAuditsIfPurgingIsNotAllowedAsync()
        {
            // given
            this.auditServiceConfigurations.IsAuditPurgingAllowed = false;

            // when
            int actualPurgedCount =
                await this.auditService.PurgeAuditsOlderThanRetentionPeriodAsync(
                    TestContext.Current.CancellationToken);

            // then
            actualPurgedCount.Should().Be(0);

            this.storageBrokerMock.Verify(broker =>
                broker.DeleteAuditsOlderThanAsync(
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()),
                        Times.Never);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never);
        }

        [Fact]
        public async Task ShouldNotValidateTheRetentionPeriodWhenPurgingIsNotAllowedAsync()
        {
            // given
            // The switch is checked before the retention period, so an environment that has
            // purging off is never tripped up by a retention period it is not using.
            this.auditServiceConfigurations.IsAuditPurgingAllowed = false;
            this.auditServiceConfigurations.AuditRetentionPeriodInDays = -1;

            // when
            int actualPurgedCount =
                await this.auditService.PurgeAuditsOlderThanRetentionPeriodAsync(
                    TestContext.Current.CancellationToken);

            // then
            actualPurgedCount.Should().Be(0);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.IsAny<Exception>()),
                    Times.Never);
        }

        [Fact]
        public async Task ShouldPurgeAuditsEvenWhenAuditRecordingIsDisabledAsync()
        {
            // given
            // The two switches are independent. A table that has stopped being written to still
            // has rows ageing past the retention period.
            DateTimeOffset currentDateTimeOffset = GetRandomDateTimeOffset();
            int deletedCount = this.auditServiceConfigurations.PurgeBatchSize - 1;
            this.auditServiceConfigurations.IsAuditEnabled = false;

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(currentDateTimeOffset);

            this.storageBrokerMock.Setup(broker =>
                broker.DeleteAuditsOlderThanAsync(
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(deletedCount);

            // when
            int actualPurgedCount =
                await this.auditService.PurgeAuditsOlderThanRetentionPeriodAsync(
                    TestContext.Current.CancellationToken);

            // then
            actualPurgedCount.Should().Be(deletedCount);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public async Task ShouldThrowValidationExceptionOnPurgeIfRetentionPeriodIsNotPositiveAndLogItAsync(
            int invalidRetentionPeriodInDays)
        {
            // given
            this.auditServiceConfigurations.AuditRetentionPeriodInDays = invalidRetentionPeriodInDays;

            var invalidAuditException =
                new InvalidAuditException(
                    message: "Invalid audit. Please correct the errors and try again.");

            invalidAuditException.AddData(
                key: nameof(AuditAndMetricsConfigurations.AuditRetentionPeriodInDays),
                values: "Value is expected to be greater than zero");

            var expectedAuditValidationException =
                new AuditValidationException(
                    message: "Audit validation errors occurred, please try again.",
                    innerException: invalidAuditException);

            // when
            ValueTask<int> purgeAuditsTask =
                this.auditService.PurgeAuditsOlderThanRetentionPeriodAsync(
                    TestContext.Current.CancellationToken);

            AuditValidationException actualAuditValidationException =
                await Assert.ThrowsAsync<AuditValidationException>(purgeAuditsTask.AsTask);

            // then
            actualAuditValidationException.Should().BeEquivalentTo(expectedAuditValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedAuditValidationException))),
                        Times.Once);

            // Nothing is read and nothing is deleted. A retention period of zero or less would
            // put the cut off at or after the present moment and take the whole table with it.
            this.storageBrokerMock.Verify(broker =>
                broker.SelectAllAuditsAsync(It.IsAny<CancellationToken>()),
                    Times.Never);

            this.storageBrokerMock.Verify(broker =>
                broker.DeleteAuditsOlderThanAsync(
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()),
                        Times.Never);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public async Task ShouldThrowValidationExceptionOnPurgeIfBatchSizeIsNotPositiveAndLogItAsync(
            int invalidPurgeBatchSize)
        {
            // given
            // A non-positive batch size would make the delete loop take nothing each pass and
            // never reach its short-batch exit.
            this.auditServiceConfigurations.PurgeBatchSize = invalidPurgeBatchSize;

            var invalidAuditException =
                new InvalidAuditException(
                    message: "Invalid audit. Please correct the errors and try again.");

            invalidAuditException.AddData(
                key: nameof(AuditAndMetricsConfigurations.PurgeBatchSize),
                values: "Value is expected to be greater than zero");

            var expectedAuditValidationException =
                new AuditValidationException(
                    message: "Audit validation errors occurred, please try again.",
                    innerException: invalidAuditException);

            // when
            ValueTask<int> purgeAuditsTask =
                this.auditService.PurgeAuditsOlderThanRetentionPeriodAsync(
                    TestContext.Current.CancellationToken);

            AuditValidationException actualAuditValidationException =
                await Assert.ThrowsAsync<AuditValidationException>(purgeAuditsTask.AsTask);

            // then
            actualAuditValidationException.Should().BeEquivalentTo(expectedAuditValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedAuditValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.DeleteAuditsOlderThanAsync(
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()),
                        Times.Never);
        }

        [Fact]
        public async Task ShouldStopPurgingWhenCancelledPartWayThroughAsync()
        {
            // given
            // The loop can run for many batches, so the token is checked each time round rather
            // than only on the way in.
            using var cancellationTokenSource = new CancellationTokenSource();
            DateTimeOffset currentDateTimeOffset = GetRandomDateTimeOffset();
            int batchSize = GetRandomNumber();
            this.auditServiceConfigurations.AuditRetentionPeriodInDays = GetRandomNumber();
            this.auditServiceConfigurations.PurgeBatchSize = batchSize;

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(currentDateTimeOffset);

            this.storageBrokerMock.Setup(broker =>
                broker.DeleteAuditsOlderThanAsync(
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(batchSize)
                        .Callback(() => cancellationTokenSource.Cancel());

            // when
            ValueTask<int> purgeAuditsTask =
                this.auditService.PurgeAuditsOlderThanRetentionPeriodAsync(
                    cancellationTokenSource.Token);

            // then
            await Assert.ThrowsAsync<AuditDependencyException>(purgeAuditsTask.AsTask);

            this.storageBrokerMock.Verify(broker =>
                broker.DeleteAuditsOlderThanAsync(
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()),
                        Times.Once);
        }
    }
}
