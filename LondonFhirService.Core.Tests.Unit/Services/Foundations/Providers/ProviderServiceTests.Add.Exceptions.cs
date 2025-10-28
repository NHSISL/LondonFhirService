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
        public async Task ShouldThrowCriticalDependencyExceptionOnAddIfSqlErrorOccursAndLogItAsync()
        {
            // given
            Provider someProvider = CreateRandomProvider();
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
                broker.ApplyAddAuditValuesAsync(It.IsAny<Provider>()))
                    .ThrowsAsync(sqlException);

            // when
            ValueTask<Provider> addProviderTask =
                this.providerService.AddProviderAsync(someProvider);

            ProviderServiceDependencyException actualProviderServiceDependencyException =
                await Assert.ThrowsAsync<ProviderServiceDependencyException>(
                    addProviderTask.AsTask);

            // then
            actualProviderServiceDependencyException.Should()
                .BeEquivalentTo(expectedProviderServiceDependencyException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<Provider>()),
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
                    Times.Never());

            this.storageBrokerMock.Verify(broker =>
                broker.InsertProviderAsync(It.IsAny<Provider>()),
                    Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyValidationExceptionOnAddIfProviderAlreadyExistsAndLogItAsync()
        {
            // given
            Provider randomProvider = CreateRandomProvider();
            Provider alreadyExistsProvider = randomProvider;
            string randomMessage = GetRandomString();

            var duplicateKeyException =
                new DuplicateKeyException(randomMessage);

            var alreadyExistsProviderServiceException =
                new AlreadyExistsProviderServiceException(
                    message: "Provider with the same Id already exists.",
                    innerException: duplicateKeyException);

            var expectedProviderServiceDependencyValidationException =
                new ProviderServiceDependencyValidationException(
                    message: "Provider dependency validation occurred, please try again.",
                    innerException: alreadyExistsProviderServiceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<Provider>()))
                    .ThrowsAsync(duplicateKeyException);

            // when
            ValueTask<Provider> addProviderTask =
                this.providerService.AddProviderAsync(alreadyExistsProvider);

            // then
            ProviderServiceDependencyValidationException actualProviderServiceDependencyValidationException =
                await Assert.ThrowsAsync<ProviderServiceDependencyValidationException>(
                    addProviderTask.AsTask);

            actualProviderServiceDependencyValidationException.Should()
                .BeEquivalentTo(expectedProviderServiceDependencyValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<Provider>()),
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
                    Times.Never());

            this.storageBrokerMock.Verify(broker =>
                broker.InsertProviderAsync(It.IsAny<Provider>()),
                    Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnAddIfReferenceErrorOccursAndLogItAsync()
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

            var expectedProviderServiceValidationException =
                new ProviderServiceDependencyValidationException(
                    message: "Provider dependency validation occurred, please try again.",
                    innerException: invalidProviderReferenceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<Provider>()))
                    .ThrowsAsync(foreignKeyConstraintConflictException);

            // when
            ValueTask<Provider> addProviderTask =
                this.providerService.AddProviderAsync(someProvider);

            // then
            ProviderServiceDependencyValidationException actualProviderServiceDependencyValidationException =
                await Assert.ThrowsAsync<ProviderServiceDependencyValidationException>(
                    addProviderTask.AsTask);

            actualProviderServiceDependencyValidationException.Should()
                .BeEquivalentTo(expectedProviderServiceValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<Provider>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedProviderServiceValidationException))),
                        Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Never);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never());

            this.storageBrokerMock.Verify(broker =>
                broker.InsertProviderAsync(It.IsAny<Provider>()),
                    Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyExceptionOnAddIfDatabaseUpdateErrorOccursAndLogItAsync()
        {
            // given
            Provider someProvider = CreateRandomProvider();

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
                broker.ApplyAddAuditValuesAsync(It.IsAny<Provider>()))
                    .ThrowsAsync(databaseUpdateException);

            // when
            ValueTask<Provider> addProviderTask =
                this.providerService.AddProviderAsync(someProvider);

            ProviderServiceDependencyException actualProviderServiceDependencyException =
                await Assert.ThrowsAsync<ProviderServiceDependencyException>(
                    addProviderTask.AsTask);

            // then
            actualProviderServiceDependencyException.Should()
                .BeEquivalentTo(expectedProviderServiceDependencyException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<Provider>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedProviderServiceDependencyException))),
                        Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnAddIfServiceErrorOccursAndLogItAsync()
        {
            // given
            Provider someProvider = CreateRandomProvider();
            var serviceException = new Exception();

            var failedProviderServiceException =
                new FailedProviderServiceException(
                    message: "Failed provider service occurred, please contact support",
                    innerException: serviceException);

            var expectedProviderServiceException =
                new ProviderServiceException(
                    message: "Provider service error occurred, contact support.",
                    innerException: failedProviderServiceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<Provider>()))
                    .ThrowsAsync(serviceException);

            // when
            ValueTask<Provider> addProviderTask = this.providerService.AddProviderAsync(someProvider);

            ProviderServiceException actualProviderServiceException =
                await Assert.ThrowsAsync<ProviderServiceException>(
                    addProviderTask.AsTask);

            // then
            actualProviderServiceException.Should()
                .BeEquivalentTo(expectedProviderServiceException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<Provider>()),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertProviderAsync(It.IsAny<Provider>()),
                    Times.Never);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedProviderServiceException))),
                        Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }
    }
}
