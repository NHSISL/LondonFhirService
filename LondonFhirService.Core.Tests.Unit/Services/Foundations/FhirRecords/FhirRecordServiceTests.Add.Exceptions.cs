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
        public async Task ShouldThrowCriticalDependencyExceptionOnAddIfSqlErrorOccursAndLogItAsync()
        {
            // given
            FhirRecord someFhirRecord = CreateRandomFhirRecord();
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
                broker.ApplyAddAuditValuesAsync(It.IsAny<FhirRecord>()))
                    .ThrowsAsync(sqlException);

            // when
            ValueTask<FhirRecord> addFhirRecordTask =
                this.fhirRecordService.AddFhirRecordAsync(someFhirRecord);

            FhirRecordDependencyException actualFhirRecordDependencyException =
                await Assert.ThrowsAsync<FhirRecordDependencyException>(
                    addFhirRecordTask.AsTask);

            // then
            actualFhirRecordDependencyException.Should()
                .BeEquivalentTo(expectedFhirRecordDependencyException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<FhirRecord>()),
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
                    Times.Never());

            this.storageBrokerMock.Verify(broker =>
                broker.InsertFhirRecordAsync(It.IsAny<FhirRecord>()),
                    Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyValidationExceptionOnAddIfFhirRecordAlreadyExistsAndLogItAsync()
        {
            // given
            FhirRecord randomFhirRecord = CreateRandomFhirRecord();
            FhirRecord alreadyExistsFhirRecord = randomFhirRecord;
            string randomMessage = GetRandomString();

            var duplicateKeyException =
                new DuplicateKeyException(randomMessage);

            var alreadyExistsFhirRecordException =
                new AlreadyExistsFhirRecordException(
                    message: "FhirRecord with the same Id already exists.",
                    innerException: duplicateKeyException);

            var expectedFhirRecordDependencyValidationException =
                new FhirRecordDependencyValidationException(
                    message: "FhirRecord dependency validation occurred, please try again.",
                    innerException: alreadyExistsFhirRecordException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<FhirRecord>()))
                    .ThrowsAsync(duplicateKeyException);

            // when
            ValueTask<FhirRecord> addFhirRecordTask =
                this.fhirRecordService.AddFhirRecordAsync(alreadyExistsFhirRecord);

            // then
            FhirRecordDependencyValidationException actualFhirRecordDependencyValidationException =
                await Assert.ThrowsAsync<FhirRecordDependencyValidationException>(
                    addFhirRecordTask.AsTask);

            actualFhirRecordDependencyValidationException.Should()
                .BeEquivalentTo(expectedFhirRecordDependencyValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<FhirRecord>()),
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
                    Times.Never());

            this.storageBrokerMock.Verify(broker =>
                broker.InsertFhirRecordAsync(It.IsAny<FhirRecord>()),
                    Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnAddIfReferenceErrorOccursAndLogItAsync()
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

            var expectedFhirRecordValidationException =
                new FhirRecordDependencyValidationException(
                    message: "FhirRecord dependency validation occurred, please try again.",
                    innerException: invalidFhirRecordReferenceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<FhirRecord>()))
                    .ThrowsAsync(foreignKeyConstraintConflictException);

            // when
            ValueTask<FhirRecord> addFhirRecordTask =
                this.fhirRecordService.AddFhirRecordAsync(someFhirRecord);

            // then
            FhirRecordDependencyValidationException actualFhirRecordDependencyValidationException =
                await Assert.ThrowsAsync<FhirRecordDependencyValidationException>(
                    addFhirRecordTask.AsTask);

            actualFhirRecordDependencyValidationException.Should()
                .BeEquivalentTo(expectedFhirRecordValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<FhirRecord>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedFhirRecordValidationException))),
                        Times.Once);

            this.securityBrokerMock.Verify(broker =>
                broker.GetCurrentUserAsync(),
                    Times.Never);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never());

            this.storageBrokerMock.Verify(broker =>
                broker.InsertFhirRecordAsync(It.IsAny<FhirRecord>()),
                    Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyExceptionOnAddIfDatabaseUpdateErrorOccursAndLogItAsync()
        {
            // given
            FhirRecord someFhirRecord = CreateRandomFhirRecord();

            var databaseUpdateException =
                new DbUpdateException();

            var failedFhirRecordStorageException =
                new FailedStorageFhirRecordException(
                    message: "Failed fhirRecord storage error occurred, contact support.",
                    innerException: databaseUpdateException);

            var expectedFhirRecordDependencyException =
                new FhirRecordDependencyException(
                    message: "FhirRecord dependency error occurred, contact support.",
                    innerException: failedFhirRecordStorageException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<FhirRecord>()))
                    .ThrowsAsync(databaseUpdateException);

            // when
            ValueTask<FhirRecord> addFhirRecordTask =
                this.fhirRecordService.AddFhirRecordAsync(someFhirRecord);

            FhirRecordDependencyException actualFhirRecordDependencyException =
                await Assert.ThrowsAsync<FhirRecordDependencyException>(
                    addFhirRecordTask.AsTask);

            // then
            actualFhirRecordDependencyException.Should()
                .BeEquivalentTo(expectedFhirRecordDependencyException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<FhirRecord>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedFhirRecordDependencyException))),
                        Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnAddIfServiceErrorOccursAndLogItAsync()
        {
            // given
            FhirRecord someFhirRecord = CreateRandomFhirRecord();
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
                broker.ApplyAddAuditValuesAsync(It.IsAny<FhirRecord>()))
                    .ThrowsAsync(serviceException);

            // when
            ValueTask<FhirRecord> addFhirRecordTask =
                this.fhirRecordService.AddFhirRecordAsync(someFhirRecord);

            FhirRecordServiceException actualFhirRecordServiceException =
                await Assert.ThrowsAsync<FhirRecordServiceException>(
                    addFhirRecordTask.AsTask);

            // then
            actualFhirRecordServiceException.Should()
                .BeEquivalentTo(expectedFhirRecordServiceException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<FhirRecord>()),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertFhirRecordAsync(It.IsAny<FhirRecord>()),
                    Times.Never);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedFhirRecordServiceException))),
                        Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
        }
    }
}