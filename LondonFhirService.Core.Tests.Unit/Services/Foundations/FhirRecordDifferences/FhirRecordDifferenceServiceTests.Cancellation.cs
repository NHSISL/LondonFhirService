// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.FhirRecordDifferences;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.FhirRecordDifferences
{
    /// <summary>
    /// Cancellation is never wrapped. A caller that cancels gets back the cancellation exception
    /// itself, nothing is logged as an error, and a caller that has already cancelled costs no
    /// round trip to storage.
    /// </summary>
    public partial class FhirRecordDifferenceServiceTests
    {
        [Theory]
        [MemberData(nameof(CancellationExceptions))]
        public async Task ShouldPassCancellationThroughOnRetrieveFhirRecordDifferenceByIdAsync(
            Exception cancellationException)
        {
            // given
            Guid randomFhirRecordDifferenceId = Guid.NewGuid();
            Guid inputFhirRecordDifferenceId = randomFhirRecordDifferenceId;

            this.storageBrokerMock.Setup(broker =>
                broker.SelectFhirRecordDifferenceByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(cancellationException);

            // when
            ValueTask<FhirRecordDifference> retrieveFhirRecordDifferenceTask =
                this.fhirRecordDifferenceService.RetrieveFhirRecordDifferenceByIdAsync(
                    inputFhirRecordDifferenceId, TestContext.Current.CancellationToken);

            OperationCanceledException actualOperationCanceledException =
                await Assert.ThrowsAnyAsync<OperationCanceledException>(
                    retrieveFhirRecordDifferenceTask.AsTask);

            // then
            actualOperationCanceledException.Should().BeSameAs(cancellationException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectFhirRecordDifferenceByIdAsync(
                    inputFhirRecordDifferenceId, It.IsAny<CancellationToken>()),
                        Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(CancellationExceptions))]
        public async Task ShouldPassCancellationThroughOnRetrieveAllFhirRecordDifferencesAsync(
            Exception cancellationException)
        {
            // given
            this.storageBrokerMock.Setup(broker =>
                broker.SelectAllFhirRecordDifferencesAsync(It.IsAny<CancellationToken>()))
                    .ThrowsAsync(cancellationException);

            // when
            ValueTask<IQueryable<FhirRecordDifference>> retrieveAllFhirRecordDifferencesTask =
                this.fhirRecordDifferenceService.RetrieveAllFhirRecordDifferencesAsync(
                    TestContext.Current.CancellationToken);

            OperationCanceledException actualOperationCanceledException =
                await Assert.ThrowsAnyAsync<OperationCanceledException>(
                    retrieveAllFhirRecordDifferencesTask.AsTask);

            // then
            actualOperationCanceledException.Should().BeSameAs(cancellationException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAllFhirRecordDifferencesAsync(It.IsAny<CancellationToken>()),
                    Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldPassCancellationThroughOnAddFhirRecordDifferenceIfAlreadyCancelledAsync()
        {
            // given
            CancellationToken cancelledToken = new CancellationToken(canceled: true);
            FhirRecordDifference randomFhirRecordDifference = CreateRandomFhirRecordDifference();

            // when
            ValueTask<FhirRecordDifference> addFhirRecordDifferenceTask =
                this.fhirRecordDifferenceService.AddFhirRecordDifferenceAsync(
                    randomFhirRecordDifference, cancelledToken);

            OperationCanceledException actualOperationCanceledException =
                await Assert.ThrowsAnyAsync<OperationCanceledException>(addFhirRecordDifferenceTask.AsTask);

            // then
            actualOperationCanceledException.CancellationToken.Should().Be(cancelledToken);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldPassCancellationThroughOnRetrieveAllFhirRecordDifferencesIfAlreadyCancelledAsync()
        {
            // given
            CancellationToken cancelledToken = new CancellationToken(canceled: true);

            // when
            ValueTask<IQueryable<FhirRecordDifference>> retrieveAllFhirRecordDifferencesTask =
                this.fhirRecordDifferenceService.RetrieveAllFhirRecordDifferencesAsync(cancelledToken);

            OperationCanceledException actualOperationCanceledException =
                await Assert.ThrowsAnyAsync<OperationCanceledException>(
                    retrieveAllFhirRecordDifferencesTask.AsTask);

            // then
            actualOperationCanceledException.CancellationToken.Should().Be(cancelledToken);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldPassCancellationThroughOnRetrieveFhirRecordDifferenceByIdIfAlreadyCancelledAsync()
        {
            // given
            CancellationToken cancelledToken = new CancellationToken(canceled: true);
            Guid randomFhirRecordDifferenceId = Guid.NewGuid();

            // when
            ValueTask<FhirRecordDifference> retrieveFhirRecordDifferenceTask =
                this.fhirRecordDifferenceService.RetrieveFhirRecordDifferenceByIdAsync(
                    randomFhirRecordDifferenceId, cancelledToken);

            OperationCanceledException actualOperationCanceledException =
                await Assert.ThrowsAnyAsync<OperationCanceledException>(
                    retrieveFhirRecordDifferenceTask.AsTask);

            // then
            actualOperationCanceledException.CancellationToken.Should().Be(cancelledToken);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldPassCancellationThroughOnModifyFhirRecordDifferenceIfAlreadyCancelledAsync()
        {
            // given
            CancellationToken cancelledToken = new CancellationToken(canceled: true);
            FhirRecordDifference randomFhirRecordDifference = CreateRandomFhirRecordDifference();

            // when
            ValueTask<FhirRecordDifference> modifyFhirRecordDifferenceTask =
                this.fhirRecordDifferenceService.ModifyFhirRecordDifferenceAsync(
                    randomFhirRecordDifference, cancelledToken);

            OperationCanceledException actualOperationCanceledException =
                await Assert.ThrowsAnyAsync<OperationCanceledException>(
                    modifyFhirRecordDifferenceTask.AsTask);

            // then
            actualOperationCanceledException.CancellationToken.Should().Be(cancelledToken);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldPassCancellationThroughOnRemoveFhirRecordDifferenceByIdIfAlreadyCancelledAsync()
        {
            // given
            CancellationToken cancelledToken = new CancellationToken(canceled: true);
            Guid randomFhirRecordDifferenceId = Guid.NewGuid();

            // when
            ValueTask<FhirRecordDifference> removeFhirRecordDifferenceTask =
                this.fhirRecordDifferenceService.RemoveFhirRecordDifferenceByIdAsync(
                    randomFhirRecordDifferenceId, cancelledToken);

            OperationCanceledException actualOperationCanceledException =
                await Assert.ThrowsAnyAsync<OperationCanceledException>(
                    removeFhirRecordDifferenceTask.AsTask);

            // then
            actualOperationCanceledException.CancellationToken.Should().Be(cancelledToken);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
