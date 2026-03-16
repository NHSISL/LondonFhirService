// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.FhirRecordDifferences;
using LondonFhirService.Core.Models.Foundations.FhirRecordDifferences.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.FhirRecordDifferences
{
    public partial class FhirRecordDifferenceServiceTests
    {
        [Fact]
        public async Task ShouldThrowCriticalDependencyExceptionOnRemoveIfSqlErrorOccursAndLogItAsync()
        {
            // given
            FhirRecordDifference randomFhirRecordDifference = CreateRandomFhirRecordDifference();
            SqlException sqlException = GetSqlException();

            var failedFhirRecordDifferenceStorageException =
                new FailedStorageFhirRecordDifferenceException(
                    message: "Failed fhirRecordDifference storage error occurred, contact support.",
                    innerException: sqlException);

            var expectedFhirRecordDifferenceDependencyException =
                new FhirRecordDifferenceDependencyException(
                    message: "FhirRecordDifference dependency error occurred, contact support.",
                    innerException: failedFhirRecordDifferenceStorageException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectFhirRecordDifferenceByIdAsync(randomFhirRecordDifference.Id))
                    .Throws(sqlException);

            // when
            ValueTask<FhirRecordDifference> addFhirRecordDifferenceTask =
                this.fhirRecordDifferenceService.RemoveFhirRecordDifferenceByIdAsync(randomFhirRecordDifference.Id);

            FhirRecordDifferenceDependencyException actualFhirRecordDifferenceDependencyException =
                await Assert.ThrowsAsync<FhirRecordDifferenceDependencyException>(
                    addFhirRecordDifferenceTask.AsTask);

            // then
            actualFhirRecordDifferenceDependencyException.Should()
                .BeEquivalentTo(expectedFhirRecordDifferenceDependencyException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectFhirRecordDifferenceByIdAsync(randomFhirRecordDifference.Id),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedFhirRecordDifferenceDependencyException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.DeleteFhirRecordDifferenceAsync(It.IsAny<FhirRecordDifference>()),
                    Times.Never);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyValidationOnRemoveIfDatabaseUpdateConcurrencyErrorOccursAndLogItAsync()
        {
            // given
            Guid someFhirRecordDifferenceId = Guid.NewGuid();

            var databaseUpdateConcurrencyException =
                new DbUpdateConcurrencyException();

            var lockedFhirRecordDifferenceException =
                new LockedFhirRecordDifferenceException(
                    message: "Locked fhirRecordDifference record exception, please try again later",
                    innerException: databaseUpdateConcurrencyException);

            var expectedFhirRecordDifferenceDependencyValidationException =
                new FhirRecordDifferenceDependencyValidationException(
                    message: "FhirRecordDifference dependency validation occurred, please try again.",
                    innerException: lockedFhirRecordDifferenceException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectFhirRecordDifferenceByIdAsync(It.IsAny<Guid>()))
                    .ThrowsAsync(databaseUpdateConcurrencyException);

            // when
            ValueTask<FhirRecordDifference> removeFhirRecordDifferenceByIdTask =
                this.fhirRecordDifferenceService.RemoveFhirRecordDifferenceByIdAsync(someFhirRecordDifferenceId);

            FhirRecordDifferenceDependencyValidationException actualFhirRecordDifferenceDependencyValidationException =
                await Assert.ThrowsAsync<FhirRecordDifferenceDependencyValidationException>(
                    removeFhirRecordDifferenceByIdTask.AsTask);

            // then
            actualFhirRecordDifferenceDependencyValidationException.Should()
                .BeEquivalentTo(expectedFhirRecordDifferenceDependencyValidationException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectFhirRecordDifferenceByIdAsync(It.IsAny<Guid>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedFhirRecordDifferenceDependencyValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.DeleteFhirRecordDifferenceAsync(It.IsAny<FhirRecordDifference>()),
                    Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyExceptionOnRemoveWhenSqlExceptionOccursAndLogItAsync()
        {
            // given
            Guid someFhirRecordDifferenceId = Guid.NewGuid();
            SqlException sqlException = GetSqlException();

            var failedFhirRecordDifferenceStorageException =
                new FailedStorageFhirRecordDifferenceException(
                    message: "Failed fhirRecordDifference storage error occurred, contact support.",
                    innerException: sqlException);

            var expectedFhirRecordDifferenceDependencyException =
                new FhirRecordDifferenceDependencyException(
                    message: "FhirRecordDifference dependency error occurred, contact support.",
                    innerException: failedFhirRecordDifferenceStorageException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectFhirRecordDifferenceByIdAsync(It.IsAny<Guid>()))
                    .ThrowsAsync(sqlException);

            // when
            ValueTask<FhirRecordDifference> deleteFhirRecordDifferenceTask =
                this.fhirRecordDifferenceService.RemoveFhirRecordDifferenceByIdAsync(someFhirRecordDifferenceId);

            FhirRecordDifferenceDependencyException actualFhirRecordDifferenceDependencyException =
                await Assert.ThrowsAsync<FhirRecordDifferenceDependencyException>(
                    deleteFhirRecordDifferenceTask.AsTask);

            // then
            actualFhirRecordDifferenceDependencyException.Should()
                .BeEquivalentTo(expectedFhirRecordDifferenceDependencyException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectFhirRecordDifferenceByIdAsync(It.IsAny<Guid>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedFhirRecordDifferenceDependencyException))),
                        Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnRemoveIfServiceErrorOccursAndLogItAsync()
        {
            // given
            Guid someFhirRecordDifferenceId = Guid.NewGuid();
            var serviceException = new Exception();

            var failedFhirRecordDifferenceServiceException =
                new FailedFhirRecordDifferenceServiceException(
                    message: "Failed fhirRecordDifference service occurred, please contact support",
                    innerException: serviceException);

            var expectedFhirRecordDifferenceServiceException =
                new FhirRecordDifferenceServiceException(
                    message: "FhirRecordDifference service error occurred, contact support.",
                    innerException: failedFhirRecordDifferenceServiceException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectFhirRecordDifferenceByIdAsync(It.IsAny<Guid>()))
                    .ThrowsAsync(serviceException);

            // when
            ValueTask<FhirRecordDifference> removeFhirRecordDifferenceByIdTask =
                this.fhirRecordDifferenceService.RemoveFhirRecordDifferenceByIdAsync(someFhirRecordDifferenceId);

            FhirRecordDifferenceServiceException actualFhirRecordDifferenceServiceException =
                await Assert.ThrowsAsync<FhirRecordDifferenceServiceException>(
                    removeFhirRecordDifferenceByIdTask.AsTask);

            // then
            actualFhirRecordDifferenceServiceException.Should()
                .BeEquivalentTo(expectedFhirRecordDifferenceServiceException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectFhirRecordDifferenceByIdAsync(It.IsAny<Guid>()),
                        Times.Once());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedFhirRecordDifferenceServiceException))),
                        Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
        }
    }
}