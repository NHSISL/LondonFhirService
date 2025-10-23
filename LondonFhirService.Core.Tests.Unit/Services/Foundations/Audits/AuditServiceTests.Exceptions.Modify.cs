// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using EFxceptions.Models.Exceptions;
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
        public async Task ShouldThrowCriticalDependencyExceptionOnModifyIfSqlErrorOccursAndLogItAsync()
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

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<Audit>()))
                    .ThrowsAsync(sqlException);

            // when
            ValueTask<Audit> modifyAuditTask =
                this.auditService.ModifyAuditAsync(randomAudit);

            AuditDependencyException actualAuditDependencyException =
                await Assert.ThrowsAsync<AuditDependencyException>(
                    modifyAuditTask.AsTask);

            // then
            actualAuditDependencyException.Should()
                .BeEquivalentTo(expectedAuditDependencyException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<Audit>()),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAuditByIdAsync(randomAudit.Id),
                    Times.Never);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedAuditDependencyException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateAuditAsync(randomAudit),
                    Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnModifyIfReferenceErrorOccursAndLogItAsync()
        {
            // given
            Audit someAudit = CreateRandomAudit();
            string randomMessage = GetRandomString();
            string exceptionMessage = randomMessage;

            var foreignKeyConstraintConflictException =
                new ForeignKeyConstraintConflictException(exceptionMessage);

            var invalidAuditReferenceException =
                new InvalidAuditReferenceException(
                    message: "Invalid audit reference error occurred.",
                    innerException: foreignKeyConstraintConflictException);

            AuditDependencyValidationException expectedAuditDependencyValidationException =
                new AuditDependencyValidationException(
                    message: "Audit dependency validation occurred, please try again.",
                    innerException: invalidAuditReferenceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<Audit>()))
                    .ThrowsAsync(foreignKeyConstraintConflictException);

            // when
            ValueTask<Audit> modifyAuditTask =
                this.auditService.ModifyAuditAsync(someAudit);

            AuditDependencyValidationException actualAuditDependencyValidationException =
                await Assert.ThrowsAsync<AuditDependencyValidationException>(
                    modifyAuditTask.AsTask);

            // then
            actualAuditDependencyValidationException.Should()
                .BeEquivalentTo(expectedAuditDependencyValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<Audit>()),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAuditByIdAsync(someAudit.Id),
                    Times.Never);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(expectedAuditDependencyValidationException))),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateAuditAsync(someAudit),
                    Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyExceptionOnModifyIfDatabaseUpdateExceptionOccursAndLogItAsync()
        {
            // given
            Audit randomAudit = CreateRandomAudit();
            var databaseUpdateException = new DbUpdateException();

            var failedAuditStorageException =
                new FailedAuditStorageException(
                    message: "Failed audit storage error occurred, please contact support.",
                    innerException: databaseUpdateException);

            var expectedAuditDependencyException =
                new AuditDependencyException(
                    message: "Audit dependency error occurred, please contact support.",
                    innerException: failedAuditStorageException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<Audit>()))
                    .ThrowsAsync(databaseUpdateException);

            // when
            ValueTask<Audit> modifyAuditTask =
                this.auditService.ModifyAuditAsync(randomAudit);

            AuditDependencyException actualAuditDependencyException =
                await Assert.ThrowsAsync<AuditDependencyException>(
                    modifyAuditTask.AsTask);

            // then
            actualAuditDependencyException.Should()
                .BeEquivalentTo(expectedAuditDependencyException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<Audit>()),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAuditByIdAsync(randomAudit.Id),
                    Times.Never);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedAuditDependencyException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateAuditAsync(randomAudit),
                    Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyValidationExceptionOnModifyIfDbUpdateConcurrencyErrorOccursAndLogAsync()
        {
            // given
            Audit randomAudit = CreateRandomAudit();
            var databaseUpdateConcurrencyException = new DbUpdateConcurrencyException();

            var lockedAuditException =
                new LockedAuditException(
                    message: "Locked audit record exception, please try again later",
                    innerException: databaseUpdateConcurrencyException);

            var expectedAuditDependencyValidationException =
                new AuditDependencyValidationException(
                    message: "Audit dependency validation occurred, please try again.",
                    innerException: lockedAuditException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<Audit>()))
                    .ThrowsAsync(databaseUpdateConcurrencyException);

            // when
            ValueTask<Audit> modifyAuditTask =
                this.auditService.ModifyAuditAsync(randomAudit);

            AuditDependencyValidationException actualAuditDependencyValidationException =
                await Assert.ThrowsAsync<AuditDependencyValidationException>(
                    modifyAuditTask.AsTask);

            // then
            actualAuditDependencyValidationException.Should()
                .BeEquivalentTo(expectedAuditDependencyValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<Audit>()),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAuditByIdAsync(randomAudit.Id),
                    Times.Never);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedAuditDependencyValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateAuditAsync(randomAudit),
                    Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnModifyIfServiceErrorOccursAndLogItAsync()
        {
            // given
            Audit randomAudit = CreateRandomAudit();
            var serviceException = new Exception();

            var failedAuditServiceException =
                new FailedAuditServiceException(
                    message: "Failed audit service error occurred, please contact support.",
                    innerException: serviceException);

            var expectedAuditServiceException =
                new AuditServiceException(
                    message: "Audit service error occurred, please contact support.",
                    innerException: failedAuditServiceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<Audit>()))
                    .ThrowsAsync(serviceException);

            // when
            ValueTask<Audit> modifyAuditTask =
                this.auditService.ModifyAuditAsync(randomAudit);

            AuditServiceException actualAuditServiceException =
                await Assert.ThrowsAsync<AuditServiceException>(
                    modifyAuditTask.AsTask);

            // then
            actualAuditServiceException.Should()
                .BeEquivalentTo(expectedAuditServiceException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<Audit>()),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAuditByIdAsync(randomAudit.Id),
                    Times.Never);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedAuditServiceException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateAuditAsync(randomAudit),
                    Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
        }
    }
}