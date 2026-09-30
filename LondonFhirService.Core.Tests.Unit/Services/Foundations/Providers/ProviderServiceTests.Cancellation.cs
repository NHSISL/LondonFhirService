// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.Providers;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Providers
{
    /// <summary>
    /// Cancellation is never wrapped. A caller that cancels gets back the cancellation exception
    /// itself, nothing is logged as an error, and a caller that has already cancelled costs no
    /// round trip to storage.
    /// </summary>
    public partial class ProviderServiceTests
    {
        [Theory]
        [MemberData(nameof(CancellationExceptions))]
        public async Task ShouldPassCancellationThroughOnRetrieveProviderByIdAsync(
            Exception cancellationException)
        {
            // given
            Guid randomProviderId = Guid.NewGuid();
            Guid inputProviderId = randomProviderId;

            this.storageBrokerMock.Setup(broker =>
                broker.SelectProviderByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(cancellationException);

            // when
            ValueTask<Provider> retrieveProviderTask =
                this.providerService.RetrieveProviderByIdAsync(
                    inputProviderId, TestContext.Current.CancellationToken);

            OperationCanceledException actualOperationCanceledException =
                await Assert.ThrowsAnyAsync<OperationCanceledException>(retrieveProviderTask.AsTask);

            // then
            actualOperationCanceledException.Should().BeSameAs(cancellationException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectProviderByIdAsync(inputProviderId, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(CancellationExceptions))]
        public async Task ShouldPassCancellationThroughOnRetrieveAllProvidersAsync(
            Exception cancellationException)
        {
            // given
            this.storageBrokerMock.Setup(broker =>
                broker.SelectAllProvidersAsync(It.IsAny<CancellationToken>()))
                    .ThrowsAsync(cancellationException);

            // when
            ValueTask<IQueryable<Provider>> retrieveAllProvidersTask =
                this.providerService.RetrieveAllProvidersAsync(TestContext.Current.CancellationToken);

            OperationCanceledException actualOperationCanceledException =
                await Assert.ThrowsAnyAsync<OperationCanceledException>(retrieveAllProvidersTask.AsTask);

            // then
            actualOperationCanceledException.Should().BeSameAs(cancellationException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAllProvidersAsync(It.IsAny<CancellationToken>()),
                    Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(CancellationExceptions))]
        public async Task ShouldPassCancellationThroughOnRetrieveAllProvidersAsListAsync(
            Exception cancellationException)
        {
            // given
            this.storageBrokerMock.Setup(broker =>
                broker.SelectAllProvidersAsListAsync(It.IsAny<CancellationToken>()))
                    .ThrowsAsync(cancellationException);

            // when
            ValueTask<List<Provider>> retrieveAllProvidersTask =
                this.providerService.RetrieveAllProvidersAsListAsync(TestContext.Current.CancellationToken);

            OperationCanceledException actualOperationCanceledException =
                await Assert.ThrowsAnyAsync<OperationCanceledException>(retrieveAllProvidersTask.AsTask);

            // then
            actualOperationCanceledException.Should().BeSameAs(cancellationException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAllProvidersAsListAsync(It.IsAny<CancellationToken>()),
                    Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldPassCancellationThroughOnAddProviderIfAlreadyCancelledAsync()
        {
            // given
            CancellationToken cancelledToken = new CancellationToken(canceled: true);
            Provider randomProvider = CreateRandomProvider();

            // when
            ValueTask<Provider> addProviderTask =
                this.providerService.AddProviderAsync(randomProvider, cancelledToken);

            OperationCanceledException actualOperationCanceledException =
                await Assert.ThrowsAnyAsync<OperationCanceledException>(addProviderTask.AsTask);

            // then
            actualOperationCanceledException.CancellationToken.Should().Be(cancelledToken);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldPassCancellationThroughOnRetrieveAllProvidersIfAlreadyCancelledAsync()
        {
            // given
            CancellationToken cancelledToken = new CancellationToken(canceled: true);

            // when
            ValueTask<IQueryable<Provider>> retrieveAllProvidersTask =
                this.providerService.RetrieveAllProvidersAsync(cancelledToken);

            OperationCanceledException actualOperationCanceledException =
                await Assert.ThrowsAnyAsync<OperationCanceledException>(retrieveAllProvidersTask.AsTask);

            // then
            actualOperationCanceledException.CancellationToken.Should().Be(cancelledToken);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldPassCancellationThroughOnRetrieveAllProvidersAsListIfAlreadyCancelledAsync()
        {
            // given
            CancellationToken cancelledToken = new CancellationToken(canceled: true);

            // when
            ValueTask<List<Provider>> retrieveAllProvidersTask =
                this.providerService.RetrieveAllProvidersAsListAsync(cancelledToken);

            OperationCanceledException actualOperationCanceledException =
                await Assert.ThrowsAnyAsync<OperationCanceledException>(retrieveAllProvidersTask.AsTask);

            // then
            actualOperationCanceledException.CancellationToken.Should().Be(cancelledToken);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldPassCancellationThroughOnRetrieveProviderByIdIfAlreadyCancelledAsync()
        {
            // given
            CancellationToken cancelledToken = new CancellationToken(canceled: true);
            Guid randomProviderId = Guid.NewGuid();

            // when
            ValueTask<Provider> retrieveProviderTask =
                this.providerService.RetrieveProviderByIdAsync(randomProviderId, cancelledToken);

            OperationCanceledException actualOperationCanceledException =
                await Assert.ThrowsAnyAsync<OperationCanceledException>(retrieveProviderTask.AsTask);

            // then
            actualOperationCanceledException.CancellationToken.Should().Be(cancelledToken);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldPassCancellationThroughOnModifyProviderIfAlreadyCancelledAsync()
        {
            // given
            CancellationToken cancelledToken = new CancellationToken(canceled: true);
            Provider randomProvider = CreateRandomProvider();

            // when
            ValueTask<Provider> modifyProviderTask =
                this.providerService.ModifyProviderAsync(randomProvider, cancelledToken);

            OperationCanceledException actualOperationCanceledException =
                await Assert.ThrowsAnyAsync<OperationCanceledException>(modifyProviderTask.AsTask);

            // then
            actualOperationCanceledException.CancellationToken.Should().Be(cancelledToken);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldPassCancellationThroughOnRemoveProviderByIdIfAlreadyCancelledAsync()
        {
            // given
            CancellationToken cancelledToken = new CancellationToken(canceled: true);
            Guid randomProviderId = Guid.NewGuid();

            // when
            ValueTask<Provider> removeProviderTask =
                this.providerService.RemoveProviderByIdAsync(randomProviderId, cancelledToken);

            OperationCanceledException actualOperationCanceledException =
                await Assert.ThrowsAnyAsync<OperationCanceledException>(removeProviderTask.AsTask);

            // then
            actualOperationCanceledException.CancellationToken.Should().Be(cancelledToken);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
