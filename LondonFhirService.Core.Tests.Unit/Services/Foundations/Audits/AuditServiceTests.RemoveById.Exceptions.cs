// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Brokers.Storages.Sql;
using LondonFhirService.Core.Models.Foundations.Audits;
using LondonFhirService.Core.Models.Foundations.Audits.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Audits
{
    public partial class AuditServiceTests
    {
        [Fact]
        public async Task ShouldThrowCriticalDependencyExceptionOnRemoveIfSqlErrorOccursAndLogItAsync()
        {
            // given
            Audit randomAudit = CreateRandomAudit();
            SqlException sqlException = GetSqlException();

            var failedAuditStorageException =
                new FailedStorageAuditServiceException(
                    message: "Failed audit storage error occurred, please contact support.",
                    innerException: sqlException);

            var expectedAuditDependencyException =
                new AuditServiceDependencyException(
                    message: "Audit dependency error occurred, please contact support.",
                    innerException: failedAuditStorageException);

            this.storageBrokerFactoryMock.Setup(broker =>
                broker.CreateDbContextAsync())
                    .ReturnsAsync(this.storageBrokerMock.Object as StorageBroker);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectAuditByIdAsync(randomAudit.Id))
                    .ThrowsAsync(sqlException);

            // when
            ValueTask<Audit> addAuditTask =
                this.auditService.RemoveAuditByIdAsync(randomAudit.Id);

            AuditServiceDependencyException actualAuditDependencyException =
                await Assert.ThrowsAsync<AuditServiceDependencyException>(
                    addAuditTask.AsTask);

            // then
            actualAuditDependencyException.Should()
                .BeEquivalentTo(expectedAuditDependencyException);

            this.storageBrokerFactoryMock.Verify(broker =>
                broker.CreateDbContextAsync(),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAuditByIdAsync(randomAudit.Id),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedAuditDependencyException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.DeleteAuditAsync(It.IsAny<Audit>()),
                    Times.Never);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never);

            this.storageBrokerFactoryMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyValidationOnRemoveIfDatabaseUpdateConcurrencyErrorOccursAndLogItAsync()
        {
            // given
            Guid someAuditId = Guid.NewGuid();

            var databaseUpdateConcurrencyException =
                new DbUpdateConcurrencyException();

            var lockedAuditException =
                new LockedAuditServiceException(
                    message: "Locked audit record exception, please try again later",
                    innerException: databaseUpdateConcurrencyException);

            var expectedAuditDependencyValidationException =
                new AuditServiceDependencyValidationException(
                    message: "Audit dependency validation occurred, please try again.",
                    innerException: lockedAuditException);

            this.storageBrokerFactoryMock.Setup(broker =>
                broker.CreateDbContextAsync())
                    .ReturnsAsync(this.storageBrokerMock.Object as StorageBroker);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectAuditByIdAsync(It.IsAny<Guid>()))
                    .ThrowsAsync(databaseUpdateConcurrencyException);

            // when
            ValueTask<Audit> removeAuditByIdTask =
                this.auditService.RemoveAuditByIdAsync(someAuditId);

            AuditServiceDependencyValidationException actualAuditDependencyValidationException =
                await Assert.ThrowsAsync<AuditServiceDependencyValidationException>(
                    removeAuditByIdTask.AsTask);

            // then
            actualAuditDependencyValidationException.Should()
                .BeEquivalentTo(expectedAuditDependencyValidationException);

            this.storageBrokerFactoryMock.Verify(broker =>
                broker.CreateDbContextAsync(),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAuditByIdAsync(It.IsAny<Guid>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedAuditDependencyValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.DeleteAuditAsync(It.IsAny<Audit>()),
                    Times.Never);

            this.storageBrokerFactoryMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyExceptionOnRemoveWhenSqlExceptionOccursAndLogItAsync()
        {
            // given
            Guid someAuditId = Guid.NewGuid();
            SqlException sqlException = GetSqlException();

            var failedAuditStorageException =
                new FailedStorageAuditServiceException(
                    message: "Failed audit storage error occurred, please contact support.",
                    innerException: sqlException);

            var expectedAuditDependencyException =
                new AuditServiceDependencyException(
                    message: "Audit dependency error occurred, please contact support.",
                    innerException: failedAuditStorageException);

            this.storageBrokerFactoryMock.Setup(broker =>
                broker.CreateDbContextAsync())
                    .ReturnsAsync(this.storageBrokerMock.Object as StorageBroker);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectAuditByIdAsync(It.IsAny<Guid>()))
                    .ThrowsAsync(sqlException);

            // when
            ValueTask<Audit> deleteAuditTask =
                this.auditService.RemoveAuditByIdAsync(someAuditId);

            AuditServiceDependencyException actualAuditDependencyException =
                await Assert.ThrowsAsync<AuditServiceDependencyException>(
                    deleteAuditTask.AsTask);

            // then
            actualAuditDependencyException.Should()
                .BeEquivalentTo(expectedAuditDependencyException);

            this.storageBrokerFactoryMock.Verify(broker =>
                broker.CreateDbContextAsync(),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAuditByIdAsync(It.IsAny<Guid>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedAuditDependencyException))),
                        Times.Once);

            this.storageBrokerFactoryMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnRemoveIfServiceErrorOccursAndLogItAsync()
        {
            // given
            Guid someAuditId = Guid.NewGuid();
            var serviceException = new Exception();

            var failedAuditServiceException =
                new FailedAuditServiceException(
                    message: "Failed audit service error occurred, please contact support.",
                    innerException: serviceException);

            var expectedAuditServiceException =
                new AuditServiceException(
                    message: "Audit service error occurred, please contact support.",
                    innerException: failedAuditServiceException);

            this.storageBrokerFactoryMock.Setup(broker =>
                broker.CreateDbContextAsync())
                    .ReturnsAsync(this.storageBrokerMock.Object as StorageBroker);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectAuditByIdAsync(It.IsAny<Guid>()))
                    .ThrowsAsync(serviceException);

            // when
            ValueTask<Audit> removeAuditByIdTask =
                this.auditService.RemoveAuditByIdAsync(someAuditId);

            AuditServiceException actualAuditServiceException =
                await Assert.ThrowsAsync<AuditServiceException>(
                    removeAuditByIdTask.AsTask);

            // then
            actualAuditServiceException.Should()
                .BeEquivalentTo(expectedAuditServiceException);

            this.storageBrokerFactoryMock.Verify(broker =>
                broker.CreateDbContextAsync(),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAuditByIdAsync(It.IsAny<Guid>()),
                        Times.Once());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedAuditServiceException))),
                        Times.Once);

            this.storageBrokerFactoryMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
        }
    }
}