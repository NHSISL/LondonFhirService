// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using EFxceptions.Models.Exceptions;
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
        public async Task ShouldThrowCriticalDependencyExceptionOnModifyIfSqlErrorOccursAndLogItAsync()
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

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<Provider>()))
                    .ThrowsAsync(sqlException);

            // when
            ValueTask<Provider> modifyProviderTask =
                this.providerService.ModifyProviderAsync(randomProvider);

            ProviderServiceDependencyException actualProviderServiceDependencyException =
                await Assert.ThrowsAsync<ProviderServiceDependencyException>(
                    modifyProviderTask.AsTask);

            // then
            actualProviderServiceDependencyException.Should()
                .BeEquivalentTo(expectedProviderServiceDependencyException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<Provider>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedProviderServiceDependencyException))),
                        Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Never);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectProviderByIdAsync(It.IsAny<Guid>()),
                    Times.Never);

            this.securityAuditBrokerMock.Verify(broker => broker
                .EnsureAddAuditValuesRemainsUnchangedOnModifyAsync(It.IsAny<Provider>(), It.IsAny<Provider>()),
                    Times.Never);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateProviderAsync(It.IsAny<Provider>()),
                    Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnModifyIfReferenceErrorOccursAndLogItAsync()
        {
            // given
            Provider someProvider = CreateRandomProvider();
            string randomMessage = GetRandomString();
            string exceptionMessage = randomMessage;

            var foreignKeyConstraintConflictException =
                new ForeignKeyConstraintConflictException(exceptionMessage);

            var invalidProviderReferenceException =
                new InvalidReferenceProviderServiceException(
                    message: "Invalid provider reference error occurred.",
                    innerException: foreignKeyConstraintConflictException);

            ProviderServiceDependencyValidationException expectedProviderServiceDependencyValidationException =
                new ProviderServiceDependencyValidationException(
                    message: "Provider dependency validation occurred, please try again.",
                    innerException: invalidProviderReferenceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<Provider>()))
                    .ThrowsAsync(foreignKeyConstraintConflictException);

            // when
            ValueTask<Provider> modifyProviderTask =
                this.providerService.ModifyProviderAsync(someProvider);

            ProviderServiceDependencyValidationException actualProviderServiceDependencyValidationException =
                await Assert.ThrowsAsync<ProviderServiceDependencyValidationException>(
                    modifyProviderTask.AsTask);

            // then
            actualProviderServiceDependencyValidationException.Should()
                .BeEquivalentTo(expectedProviderServiceDependencyValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<Provider>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(expectedProviderServiceDependencyValidationException))),
                    Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Never);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectProviderByIdAsync(It.IsAny<Guid>()),
                    Times.Never);

            this.securityAuditBrokerMock.Verify(broker => broker
                .EnsureAddAuditValuesRemainsUnchangedOnModifyAsync(It.IsAny<Provider>(), It.IsAny<Provider>()),
                    Times.Never);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateProviderAsync(It.IsAny<Provider>()),
                    Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyExceptionOnModifyIfDatabaseUpdateExceptionOccursAndLogItAsync()
        {
            // given
            Provider randomProvider = CreateRandomProvider();
            var databaseUpdateException = new DbUpdateException();

            var failedStorageProviderServiceException =
                new FailedStorageProviderServiceException(
                    message: "Failed provider storage error occurred, contact support.",
                    innerException: databaseUpdateException);

            var expectedProviderServiceDependencyException =
                new ProviderServiceDependencyException(
                    message: "Provider dependency error occurred, contact support.",
                    innerException: failedStorageProviderServiceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<Provider>()))
                    .ThrowsAsync(databaseUpdateException);

            // when
            ValueTask<Provider> modifyProviderTask =
                this.providerService.ModifyProviderAsync(randomProvider);

            ProviderServiceDependencyException actualProviderServiceDependencyException =
                await Assert.ThrowsAsync<ProviderServiceDependencyException>(
                    modifyProviderTask.AsTask);

            // then
            actualProviderServiceDependencyException.Should()
                .BeEquivalentTo(expectedProviderServiceDependencyException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<Provider>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedProviderServiceDependencyException))),
                        Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Never);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectProviderByIdAsync(It.IsAny<Guid>()),
                    Times.Never);

            this.securityAuditBrokerMock.Verify(broker => broker
                .EnsureAddAuditValuesRemainsUnchangedOnModifyAsync(It.IsAny<Provider>(), It.IsAny<Provider>()),
                    Times.Never);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateProviderAsync(It.IsAny<Provider>()),
                    Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyValidationExceptionOnModifyIfDbUpdateConcurrencyErrorOccursAndLogAsync()
        {
            // given
            Provider randomProvider = CreateRandomProvider();
            var databaseUpdateConcurrencyException = new DbUpdateConcurrencyException();

            var lockedProviderServiceException =
                new LockedProviderServiceException(
                    message: "Locked provider record exception, please try again later",
                    innerException: databaseUpdateConcurrencyException);

            var expectedProviderServiceDependencyValidationException =
                new ProviderServiceDependencyValidationException(
                    message: "Provider dependency validation occurred, please try again.",
                    innerException: lockedProviderServiceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<Provider>()))
                    .ThrowsAsync(databaseUpdateConcurrencyException);

            // when
            ValueTask<Provider> modifyProviderTask =
                this.providerService.ModifyProviderAsync(randomProvider);

            ProviderServiceDependencyValidationException actualProviderServiceDependencyValidationException =
                await Assert.ThrowsAsync<ProviderServiceDependencyValidationException>(
                    modifyProviderTask.AsTask);

            // then
            actualProviderServiceDependencyValidationException.Should()
                .BeEquivalentTo(expectedProviderServiceDependencyValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<Provider>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedProviderServiceDependencyValidationException))),
                        Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Never);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectProviderByIdAsync(It.IsAny<Guid>()),
                    Times.Never);

            this.securityAuditBrokerMock.Verify(broker => broker
                .EnsureAddAuditValuesRemainsUnchangedOnModifyAsync(It.IsAny<Provider>(), It.IsAny<Provider>()),
                    Times.Never);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateProviderAsync(It.IsAny<Provider>()),
                    Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }
    }
}
