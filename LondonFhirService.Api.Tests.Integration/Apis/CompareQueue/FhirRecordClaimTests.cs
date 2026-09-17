// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Api.Tests.Integration.Brokers;
using LondonFhirService.Core.Brokers.Storages.Sql;
using LondonFhirService.Core.Models.Foundations.FhirRecords;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LondonFhirService.Api.Tests.Integration.Apis.CompareQueue
{
    /// <summary>
    /// The compare-queue claim against a real database.
    ///
    /// This is the one part of the queue that unit tests cannot prove. They mock IStorageBroker, so
    /// they verify the orchestration's decisions but never that ClaimFhirRecordAsync's expression
    /// translates to SQL at all - and it is an ExecuteUpdateAsync whose predicate closes over a
    /// nullable DateTimeOffset, which is exactly the shape that fails at runtime rather than at
    /// compile time. These tests exercise the real translation and the real guard semantics.
    /// </summary>
    [Collection(nameof(ApiTestCollection))]
    public class FhirRecordClaimTests
    {
        private readonly ApiBroker apiBroker;

        public FhirRecordClaimTests(ApiBroker apiBroker) =>
            this.apiBroker = apiBroker;

        [Fact]
        public async Task ShouldClaimAPendingRecordExactlyOnceAsync()
        {
            // given
            FhirRecord pendingFhirRecord = await InsertFhirRecordAsync(
                status: StatusType.Pending,
                updatedDate: DateTimeOffset.UtcNow.AddHours(-1));

            // when
            int firstClaimCount = await ClaimAsync(
                pendingFhirRecord.Id, StatusType.Pending, StatusType.Processing, notUpdatedAfter: null);

            int secondClaimCount = await ClaimAsync(
                pendingFhirRecord.Id, StatusType.Pending, StatusType.Processing, notUpdatedAfter: null);

            // then
            // The database arbitrates: the first caller changes the row, the second matches
            // nothing because the status has moved on. This is what stops two workers comparing
            // the same pair and writing two difference rows for it.
            firstClaimCount.Should().Be(1);
            secondClaimCount.Should().Be(0);

            FhirRecord claimedFhirRecord = await SelectFhirRecordAsync(pendingFhirRecord.Id);
            claimedFhirRecord.Status.Should().Be(StatusType.Processing);

            await DeleteFhirRecordAsync(pendingFhirRecord.Id);
        }

        [Fact]
        public async Task ShouldReclaimAStrandedRecordOnlyOnceWhenTheLeaseHasExpiredAsync()
        {
            // given
            // A row stranded mid-claim: still Processing, last touched long before the lease
            // expiry. Both sides of this move are Processing, so the status check alone is
            // vacuous - the lease bound is the only thing arbitrating.
            DateTimeOffset leaseExpiry = DateTimeOffset.UtcNow.AddMinutes(-30);

            FhirRecord strandedFhirRecord = await InsertFhirRecordAsync(
                status: StatusType.Processing,
                updatedDate: leaseExpiry.AddMinutes(-5));

            // when
            int firstReclaimCount = await ClaimAsync(
                strandedFhirRecord.Id, StatusType.Processing, StatusType.Processing, leaseExpiry);

            int secondReclaimCount = await ClaimAsync(
                strandedFhirRecord.Id, StatusType.Processing, StatusType.Processing, leaseExpiry);

            // then
            // The first reclaim bumps UpdatedDate past the lease bound, so the second matches
            // nothing. Without the lease in the statement both would report success and two
            // workers would own the same row.
            firstReclaimCount.Should().Be(1);
            secondReclaimCount.Should().Be(0);

            await DeleteFhirRecordAsync(strandedFhirRecord.Id);
        }

        [Fact]
        public async Task ShouldNotClaimARecordWhoseLeaseHasNotExpiredAsync()
        {
            // given
            // Still within its lease - another worker is presumably still processing it.
            DateTimeOffset leaseExpiry = DateTimeOffset.UtcNow.AddMinutes(-30);

            FhirRecord liveFhirRecord = await InsertFhirRecordAsync(
                status: StatusType.Processing,
                updatedDate: DateTimeOffset.UtcNow);

            // when
            int claimCount = await ClaimAsync(
                liveFhirRecord.Id, StatusType.Processing, StatusType.Processing, leaseExpiry);

            // then
            claimCount.Should().Be(0);

            await DeleteFhirRecordAsync(liveFhirRecord.Id);
        }

        [Fact]
        public async Task ShouldTransitionARecordThatIsNotAlreadyInTheTargetStatusAsync()
        {
            // given
            FhirRecord processingFhirRecord = await InsertFhirRecordAsync(
                status: StatusType.Processing,
                updatedDate: DateTimeOffset.UtcNow.AddMinutes(-5));

            // when
            int transitionCount = await TransitionAsync(
                processingFhirRecord.Id,
                excludedStatus: StatusType.Completed,
                newStatus: StatusType.Completed,
                isProcessed: true);

            // then
            // The predicate and the SET list both have to survive translation. IsProcessed in
            // particular: the read-then-write path this replaced set it for terminal statuses, and
            // a mocked service cannot show that the statement actually writes it.
            transitionCount.Should().Be(1);

            FhirRecord completedFhirRecord = await SelectFhirRecordAsync(processingFhirRecord.Id);
            completedFhirRecord.Status.Should().Be(StatusType.Completed);
            completedFhirRecord.IsProcessed.Should().BeTrue();

            await DeleteFhirRecordAsync(processingFhirRecord.Id);
        }

        [Fact]
        public async Task ShouldNotTransitionARecordAlreadyInTheTargetStatusAsync()
        {
            // given
            FhirRecord completedFhirRecord = await InsertFhirRecordAsync(
                status: StatusType.Completed,
                updatedDate: DateTimeOffset.UtcNow.AddMinutes(-5));

            // when
            int transitionCount = await TransitionAsync(
                completedFhirRecord.Id,
                excludedStatus: StatusType.Completed,
                newStatus: StatusType.Completed,
                isProcessed: true);

            // then
            // Zero rows is what makes the primary safe to complete from several workers at once:
            // the shared primary is reached once per secondary, and the second and later callers
            // must be no-ops rather than racing a read against a write.
            transitionCount.Should().Be(0);

            FhirRecord unchangedFhirRecord = await SelectFhirRecordAsync(completedFhirRecord.Id);
            unchangedFhirRecord.Status.Should().Be(StatusType.Completed);

            // Untouched, not rewritten with the value this call would have set.
            unchangedFhirRecord.IsProcessed.Should().BeFalse();

            await DeleteFhirRecordAsync(completedFhirRecord.Id);
        }

        [Fact]
        public async Task ShouldRetainAClaimOnlyWhileTheLeaseTokenStillMatchesAsync()
        {
            // given
            DateTimeOffset claimedAt = DateTimeOffset.UtcNow.AddMinutes(-1);

            FhirRecord claimedFhirRecord = await InsertFhirRecordAsync(
                status: StatusType.Processing,
                updatedDate: claimedAt);

            // when
            // The first re-assertion is the worker proving it still owns the row; it also moves
            // UpdatedDate on, which is exactly why the caller has to advance its token.
            int firstRetainCount = await ClaimAsync(
                claimedFhirRecord.Id,
                StatusType.Processing,
                StatusType.Processing,
                notUpdatedAfter: claimedAt);

            int staleRetainCount = await ClaimAsync(
                claimedFhirRecord.Id,
                StatusType.Processing,
                StatusType.Processing,
                notUpdatedAfter: claimedAt);

            // then
            firstRetainCount.Should().Be(1);

            // Replaying the ORIGINAL token now matches nothing - which is the same shape as
            // another worker having reclaimed the row, and is what the fence relies on.
            staleRetainCount.Should().Be(0);

            await DeleteFhirRecordAsync(claimedFhirRecord.Id);
        }

        private async ValueTask<int> TransitionAsync(
            Guid fhirRecordId,
            StatusType excludedStatus,
            StatusType newStatus,
            bool isProcessed)
        {
            using var scope = this.apiBroker.WebApplicationFactory.Services.CreateScope();
            var storageBroker = scope.ServiceProvider.GetRequiredService<StorageBroker>();

            return await storageBroker.UpdateFhirRecordStatusAsync(
                fhirRecordId,
                excludedStatus,
                newStatus,
                isProcessed,
                updatedDate: DateTimeOffset.UtcNow);
        }

        private async ValueTask<int> ClaimAsync(
            Guid fhirRecordId,
            StatusType expectedStatus,
            StatusType claimedStatus,
            DateTimeOffset? notUpdatedAfter)
        {
            using var scope = this.apiBroker.WebApplicationFactory.Services.CreateScope();
            var storageBroker = scope.ServiceProvider.GetRequiredService<StorageBroker>();

            return await storageBroker.ClaimFhirRecordAsync(
                fhirRecordId,
                expectedStatus,
                claimedStatus,
                claimedDate: DateTimeOffset.UtcNow,
                notUpdatedAfter: notUpdatedAfter);
        }

        private async ValueTask<FhirRecord> InsertFhirRecordAsync(
            StatusType status,
            DateTimeOffset updatedDate)
        {
            using var scope = this.apiBroker.WebApplicationFactory.Services.CreateScope();
            var storageBroker = scope.ServiceProvider.GetRequiredService<StorageBroker>();

            var fhirRecord = new FhirRecord
            {
                Id = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid().ToString(),
                JsonPayload = "{}",
                SourceName = "ClaimTests",
                IsPrimarySource = false,
                IsProcessed = false,
                Status = status,
                CreatedBy = "ClaimTests",
                CreatedDate = updatedDate,
                UpdatedBy = "ClaimTests",
                UpdatedDate = updatedDate
            };

            return await storageBroker.InsertFhirRecordAsync(fhirRecord);
        }

        private async ValueTask<FhirRecord> SelectFhirRecordAsync(Guid fhirRecordId)
        {
            using var scope = this.apiBroker.WebApplicationFactory.Services.CreateScope();
            var storageBroker = scope.ServiceProvider.GetRequiredService<StorageBroker>();

            return await storageBroker.SelectFhirRecordByIdAsync(fhirRecordId);
        }

        private async ValueTask DeleteFhirRecordAsync(Guid fhirRecordId)
        {
            using var scope = this.apiBroker.WebApplicationFactory.Services.CreateScope();
            var storageBroker = scope.ServiceProvider.GetRequiredService<StorageBroker>();
            FhirRecord fhirRecord = await storageBroker.SelectFhirRecordByIdAsync(fhirRecordId);

            if (fhirRecord is not null)
            {
                await storageBroker.DeleteFhirRecordAsync(fhirRecord);
            }
        }
    }
}
