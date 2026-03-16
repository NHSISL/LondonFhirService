// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using EFxceptions.Models.Exceptions;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.FhirRecords;
using LondonFhirService.Core.Models.Foundations.FhirRecords.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.FhirRecords
{
    public partial class FhirRecordServiceTests
    {
        [Fact]
        public async Task ShouldThrowCriticalDependencyExceptionOnModifyIfSqlErrorOccursAndLogItAsync()
        {
            // given
            FhirRecord randomFhirRecord = CreateRandomFhirRecord();
            SqlException sqlException = GetSqlException();

            var failedFhirRecordStorageException =
                new FailedStorageFhirRecordException(
                    message: "Failed fhirRecord storage error occurred, contact support.",
                    innerException: sqlException);

            var expectedFhirRecordDependencyException =
                new FhirRecordDependencyException(
                    message: "FhirRecord dependency error occurred, contact support.",
                    innerException: failedFhirRecordStorageException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<FhirRecord>()))
                    .ThrowsAsync(sqlException);

            // when
            ValueTask<FhirRecord> modifyFhirRecordTask =
                this.fhirRecordService.ModifyFhirRecordAsync(randomFhirRecord);

            FhirRecordDependencyException actualFhirRecordDependencyException =
                await Assert.ThrowsAsync<FhirRecordDependencyException>(
                    modifyFhirRecordTask.AsTask);

            // then
            actualFhirRecordDependencyException.Should()
                .BeEquivalentTo(expectedFhirRecordDependencyException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<FhirRecord>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedFhirRecordDependencyException))),
                        Times.Once);

            this.securityBrokerMock.Verify(broker =>
                broker.GetCurrentUserAsync(),
                    Times.Never);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectFhirRecordByIdAsync(It.IsAny<Guid>()),
                    Times.Never);

            this.securityAuditBrokerMock.Verify(broker => broker
                .EnsureAddAuditValuesRemainsUnchangedOnModifyAsync(It.IsAny<FhirRecord>(), It.IsAny<FhirRecord>()),
                    Times.Never);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateFhirRecordAsync(It.IsAny<FhirRecord>()),
                    Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnModifyIfReferenceErrorOccursAndLogItAsync()
        {
            // given
            FhirRecord someFhirRecord = CreateRandomFhirRecord();
            string randomMessage = GetRandomString();
            string exceptionMessage = randomMessage;

            var foreignKeyConstraintConflictException =
                new ForeignKeyConstraintConflictException(exceptionMessage);

            var invalidFhirRecordReferenceException =
                new InvalidReferenceFhirRecordException(
                    message: "Invalid fhirRecord reference error occurred.",
                    innerException: foreignKeyConstraintConflictException);

            FhirRecordDependencyValidationException expectedFhirRecordDependencyValidationException =
                new FhirRecordDependencyValidationException(
                    message: "FhirRecord dependency validation occurred, please try again.",
                    innerException: invalidFhirRecordReferenceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<FhirRecord>()))
                    .ThrowsAsync(foreignKeyConstraintConflictException);

            // when
            ValueTask<FhirRecord> modifyFhirRecordTask =
                this.fhirRecordService.ModifyFhirRecordAsync(someFhirRecord);

            FhirRecordDependencyValidationException actualFhirRecordDependencyValidationException =
                await Assert.ThrowsAsync<FhirRecordDependencyValidationException>(
                    modifyFhirRecordTask.AsTask);

            // then
            actualFhirRecordDependencyValidationException.Should()
                .BeEquivalentTo(expectedFhirRecordDependencyValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<FhirRecord>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(expectedFhirRecordDependencyValidationException))),
                    Times.Once);

            this.securityBrokerMock.Verify(broker =>
                broker.GetCurrentUserAsync(),
                    Times.Never);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectFhirRecordByIdAsync(It.IsAny<Guid>()),
                    Times.Never);

            this.securityAuditBrokerMock.Verify(broker => broker
                .EnsureAddAuditValuesRemainsUnchangedOnModifyAsync(It.IsAny<FhirRecord>(), It.IsAny<FhirRecord>()),
                    Times.Never);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateFhirRecordAsync(It.IsAny<FhirRecord>()),
                    Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyExceptionOnModifyIfDatabaseUpdateExceptionOccursAndLogItAsync()
        {
            // given
            FhirRecord randomFhirRecord = CreateRandomFhirRecord();
            var databaseUpdateException = new DbUpdateException();

            var failedFhirRecordStorageException =
                new FailedStorageFhirRecordException(
                    message: "Failed fhirRecord storage error occurred, contact support.",
                    innerException: databaseUpdateException);

            var expectedFhirRecordDependencyException =
                new FhirRecordDependencyException(
                    message: "FhirRecord dependency error occurred, contact support.",
                    innerException: failedFhirRecordStorageException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<FhirRecord>()))
                    .ThrowsAsync(databaseUpdateException);

            // when
            ValueTask<FhirRecord> modifyFhirRecordTask =
                this.fhirRecordService.ModifyFhirRecordAsync(randomFhirRecord);

            FhirRecordDependencyException actualFhirRecordDependencyException =
                await Assert.ThrowsAsync<FhirRecordDependencyException>(
                    modifyFhirRecordTask.AsTask);

            // then
            actualFhirRecordDependencyException.Should()
                .BeEquivalentTo(expectedFhirRecordDependencyException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<FhirRecord>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedFhirRecordDependencyException))),
                        Times.Once);

            this.securityBrokerMock.Verify(broker =>
                broker.GetCurrentUserAsync(),
                    Times.Never);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectFhirRecordByIdAsync(It.IsAny<Guid>()),
                    Times.Never);

            this.securityAuditBrokerMock.Verify(broker => broker
                .EnsureAddAuditValuesRemainsUnchangedOnModifyAsync(It.IsAny<FhirRecord>(), It.IsAny<FhirRecord>()),
                    Times.Never);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateFhirRecordAsync(It.IsAny<FhirRecord>()),
                    Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyValidationExceptionOnModifyIfDbUpdateConcurrencyErrorOccursAndLogAsync()
        {
            // given
            FhirRecord randomFhirRecord = CreateRandomFhirRecord();
            var databaseUpdateConcurrencyException = new DbUpdateConcurrencyException();

            var lockedFhirRecordException =
                new LockedFhirRecordException(
                    message: "Locked fhirRecord record exception, please try again later",
                    innerException: databaseUpdateConcurrencyException);

            var expectedFhirRecordDependencyValidationException =
                new FhirRecordDependencyValidationException(
                    message: "FhirRecord dependency validation occurred, please try again.",
                    innerException: lockedFhirRecordException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<FhirRecord>()))
                    .ThrowsAsync(databaseUpdateConcurrencyException);

            // when
            ValueTask<FhirRecord> modifyFhirRecordTask =
                this.fhirRecordService.ModifyFhirRecordAsync(randomFhirRecord);

            FhirRecordDependencyValidationException actualFhirRecordDependencyValidationException =
                await Assert.ThrowsAsync<FhirRecordDependencyValidationException>(
                    modifyFhirRecordTask.AsTask);

            // then
            actualFhirRecordDependencyValidationException.Should()
                .BeEquivalentTo(expectedFhirRecordDependencyValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<FhirRecord>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedFhirRecordDependencyValidationException))),
                        Times.Once);

            this.securityBrokerMock.Verify(broker =>
                broker.GetCurrentUserAsync(),
                    Times.Never);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectFhirRecordByIdAsync(It.IsAny<Guid>()),
                    Times.Never);

            this.securityAuditBrokerMock.Verify(broker => broker
                .EnsureAddAuditValuesRemainsUnchangedOnModifyAsync(It.IsAny<FhirRecord>(), It.IsAny<FhirRecord>()),
                    Times.Never);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateFhirRecordAsync(It.IsAny<FhirRecord>()),
                    Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnModifyIfServiceErrorOccursAndLogItAsync()
        {
            // given
            FhirRecord randomFhirRecord = CreateRandomFhirRecord();
            var serviceException = new Exception();

            var failedFhirRecordServiceException =
                new FailedFhirRecordServiceException(
                    message: "Failed fhirRecord service occurred, please contact support",
                    innerException: serviceException);

            var expectedFhirRecordServiceException =
                new FhirRecordServiceException(
                    message: "FhirRecord service error occurred, contact support.",
                    innerException: failedFhirRecordServiceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<FhirRecord>()))
                    .Throws(serviceException);

            // when
            ValueTask<FhirRecord> modifyFhirRecordTask =
                this.fhirRecordService.ModifyFhirRecordAsync(randomFhirRecord);

            FhirRecordServiceException actualFhirRecordServiceException =
                await Assert.ThrowsAsync<FhirRecordServiceException>(
                    modifyFhirRecordTask.AsTask);

            // then
            actualFhirRecordServiceException.Should()
                .BeEquivalentTo(expectedFhirRecordServiceException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<FhirRecord>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedFhirRecordServiceException))),
                        Times.Once);

            this.securityBrokerMock.Verify(broker =>
                broker.GetCurrentUserAsync(),
                    Times.Never);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectFhirRecordByIdAsync(It.IsAny<Guid>()),
                    Times.Never);

            this.securityAuditBrokerMock.Verify(broker => broker
                .EnsureAddAuditValuesRemainsUnchangedOnModifyAsync(It.IsAny<FhirRecord>(), It.IsAny<FhirRecord>()),
                    Times.Never);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateFhirRecordAsync(It.IsAny<FhirRecord>()),
                    Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
        }
    }
}