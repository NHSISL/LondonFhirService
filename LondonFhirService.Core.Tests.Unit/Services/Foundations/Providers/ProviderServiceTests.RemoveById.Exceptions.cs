// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.Providers;
using LondonFhirService.Core.Models.Foundations.Providers.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Providers
{
    public partial class ProviderServiceTests
    {
        [Fact]
        public async Task ShouldThrowCriticalDependencyExceptionOnRemoveIfSqlErrorOccursAndLogItAsync()
        {
            // given
            Provider randomProvider = CreateRandomProvider();
            SqlException sqlException = GetSqlException();

            var failedStorageProviderServiceException =
                new FailedStorageProviderServiceException(
                    message: "Failed provider storage error occurred, contact support.",
                    innerException: sqlException);

            var expectedProviderServiceDependencyException =
                new ProviderServiceDependencyException(
                    message: "Provider dependency error occurred, contact support.",
                    innerException: failedStorageProviderServiceException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectProviderByIdAsync(randomProvider.Id, It.IsAny<CancellationToken>()))
                    .Throws(sqlException);

            // when
            ValueTask<Provider> addProviderTask =
                this.providerService.RemoveProviderByIdAsync(randomProvider.Id, TestContext.Current.CancellationToken);

            ProviderServiceDependencyException actualProviderServiceDependencyException =
                await Assert.ThrowsAsync<ProviderServiceDependencyException>(
                    addProviderTask.AsTask);

            // then
            actualProviderServiceDependencyException.Should()
                .BeEquivalentTo(expectedProviderServiceDependencyException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectProviderByIdAsync(randomProvider.Id, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedProviderServiceDependencyException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.DeleteProviderAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()),
                    Times.Never);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }


        [Fact]
        public async Task ShouldThrowDependencyValidationOnRemoveIfDatabaseUpdateConcurrencyErrorOccursAndLogItAsync()
        {
            // given
            Guid someProviderId = Guid.NewGuid();

            var databaseUpdateConcurrencyException =
                new DbUpdateConcurrencyException();

            var lockedProviderServiceException =
                new LockedProviderServiceException(
                    message: "Locked provider record exception, please try again later",
                    innerException: databaseUpdateConcurrencyException);

            var expectedProviderServiceDependencyValidationException =
                new ProviderServiceDependencyValidationException(
                    message: "Provider dependency validation occurred, please try again.",
                    innerException: lockedProviderServiceException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectProviderByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(databaseUpdateConcurrencyException);

            // when
            ValueTask<Provider> removeProviderByIdTask =
                this.providerService.RemoveProviderByIdAsync(someProviderId, TestContext.Current.CancellationToken);

            ProviderServiceDependencyValidationException actualProviderServiceDependencyValidationException =
                await Assert.ThrowsAsync<ProviderServiceDependencyValidationException>(
                    removeProviderByIdTask.AsTask);

            // then
            actualProviderServiceDependencyValidationException.Should()
                .BeEquivalentTo(expectedProviderServiceDependencyValidationException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectProviderByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedProviderServiceDependencyValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.DeleteProviderAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()),
                    Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyExceptionOnRemoveWhenSqlExceptionOccursAndLogItAsync()
        {
            // given
            Guid someProviderId = Guid.NewGuid();
            SqlException sqlException = GetSqlException();

            var failedStorageProviderServiceException =
                new FailedStorageProviderServiceException(
                    message: "Failed provider storage error occurred, contact support.",
                    innerException: sqlException);

            var expectedProviderServiceDependencyException =
                new ProviderServiceDependencyException(
                    message: "Provider dependency error occurred, contact support.",
                    innerException: failedStorageProviderServiceException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectProviderByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(sqlException);

            // when
            ValueTask<Provider> deleteProviderTask =
                this.providerService.RemoveProviderByIdAsync(someProviderId, TestContext.Current.CancellationToken);

            ProviderServiceDependencyException actualProviderServiceDependencyException =
                await Assert.ThrowsAsync<ProviderServiceDependencyException>(
                    deleteProviderTask.AsTask);

            // then
            actualProviderServiceDependencyException.Should()
                .BeEquivalentTo(expectedProviderServiceDependencyException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectProviderByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedProviderServiceDependencyException))),
                        Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnRemoveIfServiceErrorOccursAndLogItAsync()
        {
            // given
            Guid someProviderId = Guid.NewGuid();
            string randomString = GetRandomString();
            var serviceException = new Exception(randomString);

            var failedProviderServiceException =
                new FailedProviderServiceException(
                    message: "Failed provider service occurred, please contact support",
                    innerException: serviceException,
                    data: serviceException.Data);

            var expectedProviderServiceException =
                new ProviderServiceException(
                    message: "Provider service error occurred, contact support.",
                    innerException: failedProviderServiceException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectProviderByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(serviceException);

            // when
            ValueTask<Provider> removeProviderByIdTask =
                this.providerService.RemoveProviderByIdAsync(someProviderId, TestContext.Current.CancellationToken);

            ProviderServiceException actualProviderServiceException =
                await Assert.ThrowsAsync<ProviderServiceException>(
                    removeProviderByIdTask.AsTask);

            // then
            actualProviderServiceException.Should()
                .BeEquivalentTo(expectedProviderServiceException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectProviderByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                    Times.Once());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedProviderServiceException))),
                        Times.Once());

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }
    }
}
