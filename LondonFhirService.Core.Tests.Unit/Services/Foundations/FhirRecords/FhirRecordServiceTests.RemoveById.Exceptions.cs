// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
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
        public async Task ShouldThrowCriticalDependencyExceptionOnRemoveIfSqlErrorOccursAndLogItAsync()
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

            this.storageBrokerMock.Setup(broker =>
                broker.SelectFhirRecordByIdAsync(randomFhirRecord.Id, It.IsAny<CancellationToken>()))
                    .Throws(sqlException);

            // when
            ValueTask<FhirRecord> removeFhirRecordTask =
                this.fhirRecordService.RemoveFhirRecordByIdAsync(
                    randomFhirRecord.Id, TestContext.Current.CancellationToken);

            FhirRecordDependencyException actualFhirRecordDependencyException =
                await Assert.ThrowsAsync<FhirRecordDependencyException>(
                    removeFhirRecordTask.AsTask);

            // then
            actualFhirRecordDependencyException.Should()
                .BeEquivalentTo(expectedFhirRecordDependencyException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectFhirRecordByIdAsync(randomFhirRecord.Id, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedFhirRecordDependencyException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.DeleteFhirRecordAsync(It.IsAny<FhirRecord>(), It.IsAny<CancellationToken>()),
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
            Guid someFhirRecordId = Guid.NewGuid();

            var databaseUpdateConcurrencyException =
                new DbUpdateConcurrencyException();

            var lockedFhirRecordException =
                new LockedFhirRecordException(
                    message: "Locked fhirRecord record exception, please try again later",
                    innerException: databaseUpdateConcurrencyException);

            var expectedFhirRecordDependencyValidationException =
                new FhirRecordDependencyValidationException(
                    message: "FhirRecord dependency validation occurred, please try again.",
                    innerException: lockedFhirRecordException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectFhirRecordByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(databaseUpdateConcurrencyException);

            // when
            ValueTask<FhirRecord> removeFhirRecordByIdTask =
                this.fhirRecordService.RemoveFhirRecordByIdAsync(
                    someFhirRecordId, TestContext.Current.CancellationToken);

            FhirRecordDependencyValidationException actualFhirRecordDependencyValidationException =
                await Assert.ThrowsAsync<FhirRecordDependencyValidationException>(
                    removeFhirRecordByIdTask.AsTask);

            // then
            actualFhirRecordDependencyValidationException.Should()
                .BeEquivalentTo(expectedFhirRecordDependencyValidationException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectFhirRecordByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedFhirRecordDependencyValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.DeleteFhirRecordAsync(It.IsAny<FhirRecord>(), It.IsAny<CancellationToken>()),
                    Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnRemoveIfServiceErrorOccursAndLogItAsync()
        {
            // given
            Guid someFhirRecordId = Guid.NewGuid();
            var serviceException = new Exception();

            var failedFhirRecordServiceException =
                new FailedFhirRecordServiceException(
                    message: "Failed fhirRecord service occurred, please contact support",
                    innerException: serviceException);

            var expectedFhirRecordServiceException =
                new FhirRecordServiceException(
                    message: "FhirRecord service error occurred, contact support.",
                    innerException: failedFhirRecordServiceException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectFhirRecordByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(serviceException);

            // when
            ValueTask<FhirRecord> removeFhirRecordByIdTask =
                this.fhirRecordService.RemoveFhirRecordByIdAsync(
                    someFhirRecordId, TestContext.Current.CancellationToken);

            FhirRecordServiceException actualFhirRecordServiceException =
                await Assert.ThrowsAsync<FhirRecordServiceException>(
                    removeFhirRecordByIdTask.AsTask);

            // then
            actualFhirRecordServiceException.Should()
                .BeEquivalentTo(expectedFhirRecordServiceException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectFhirRecordByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                        Times.Once());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedFhirRecordServiceException))),
                        Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }
    }
}