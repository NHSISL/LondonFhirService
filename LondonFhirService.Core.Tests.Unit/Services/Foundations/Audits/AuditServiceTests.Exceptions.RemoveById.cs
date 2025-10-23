// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
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
                new FailedAuditStorageException(
                    message: "Failed audit storage error occurred, please contact support.",
                    innerException: sqlException);

            var expectedAuditDependencyException =
                new AuditDependencyException(
                    message: "Audit dependency error occurred, please contact support.",
                    innerException: failedAuditStorageException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectAuditByIdAsync(randomAudit.Id))
                    .ThrowsAsync(sqlException);

            // when
            ValueTask<Audit> addAuditTask =
                this.auditService.RemoveAuditByIdAsync(randomAudit.Id);

            AuditDependencyException actualAuditDependencyException =
                await Assert.ThrowsAsync<AuditDependencyException>(
                    addAuditTask.AsTask);

            // then
            actualAuditDependencyException.Should()
                .BeEquivalentTo(expectedAuditDependencyException);

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

            this.storageBrokerMock.VerifyNoOtherCalls();
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
                new LockedAuditException(
                    message: "Locked audit record exception, please try again later",
                    innerException: databaseUpdateConcurrencyException);

            var expectedAuditDependencyValidationException =
                new AuditDependencyValidationException(
                    message: "Audit dependency validation occurred, please try again.",
                    innerException: lockedAuditException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectAuditByIdAsync(It.IsAny<Guid>()))
                    .ThrowsAsync(databaseUpdateConcurrencyException);

            // when
            ValueTask<Audit> removeAuditByIdTask =
                this.auditService.RemoveAuditByIdAsync(someAuditId);

            AuditDependencyValidationException actualAuditDependencyValidationException =
                await Assert.ThrowsAsync<AuditDependencyValidationException>(
                    removeAuditByIdTask.AsTask);

            // then
            actualAuditDependencyValidationException.Should()
                .BeEquivalentTo(expectedAuditDependencyValidationException);

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

            this.storageBrokerMock.VerifyNoOtherCalls();
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
                new FailedAuditStorageException(
                    message: "Failed audit storage error occurred, please contact support.",
                    innerException: sqlException);

            var expectedAuditDependencyException =
                new AuditDependencyException(
                    message: "Audit dependency error occurred, please contact support.",
                    innerException: failedAuditStorageException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectAuditByIdAsync(It.IsAny<Guid>()))
                    .ThrowsAsync(sqlException);

            // when
            ValueTask<Audit> deleteAuditTask =
                this.auditService.RemoveAuditByIdAsync(someAuditId);

            AuditDependencyException actualAuditDependencyException =
                await Assert.ThrowsAsync<AuditDependencyException>(
                    deleteAuditTask.AsTask);

            // then
            actualAuditDependencyException.Should()
                .BeEquivalentTo(expectedAuditDependencyException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAuditByIdAsync(It.IsAny<Guid>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedAuditDependencyException))),
                        Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
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

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAuditByIdAsync(It.IsAny<Guid>()),
                        Times.Once());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedAuditServiceException))),
                        Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
        }
    }
}