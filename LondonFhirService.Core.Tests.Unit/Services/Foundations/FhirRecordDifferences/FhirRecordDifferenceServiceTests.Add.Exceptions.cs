// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using EFxceptions.Models.Exceptions;
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
        public async Task ShouldThrowCriticalDependencyExceptionOnAddIfSqlErrorOccursAndLogItAsync()
        {
            // given
            FhirRecordDifference someFhirRecordDifference = CreateRandomFhirRecordDifference();
            SqlException sqlException = GetSqlException();

            var failedFhirRecordDifferenceStorageException =
                new FailedStorageFhirRecordDifferenceException(
                    message: "Failed fhirRecordDifference storage error occurred, contact support.",
                    innerException: sqlException);

            var expectedFhirRecordDifferenceDependencyException =
                new FhirRecordDifferenceDependencyException(
                    message: "FhirRecordDifference dependency error occurred, contact support.",
                    innerException: failedFhirRecordDifferenceStorageException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<FhirRecordDifference>()))
                    .ThrowsAsync(sqlException);

            // when
            ValueTask<FhirRecordDifference> addFhirRecordDifferenceTask =
                this.fhirRecordDifferenceService.AddFhirRecordDifferenceAsync(someFhirRecordDifference);

            FhirRecordDifferenceDependencyException actualFhirRecordDifferenceDependencyException =
                await Assert.ThrowsAsync<FhirRecordDifferenceDependencyException>(
                    addFhirRecordDifferenceTask.AsTask);

            // then
            actualFhirRecordDifferenceDependencyException.Should()
                .BeEquivalentTo(expectedFhirRecordDifferenceDependencyException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<FhirRecordDifference>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedFhirRecordDifferenceDependencyException))),
                        Times.Once);

            this.securityBrokerMock.Verify(broker =>
                broker.GetCurrentUserAsync(),
                    Times.Never);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never());

            this.storageBrokerMock.Verify(broker =>
                broker.InsertFhirRecordDifferenceAsync(It.IsAny<FhirRecordDifference>()),
                    Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyValidationExceptionOnAddIfFhirRecordDifferenceAlreadyExistsAndLogItAsync()
        {
            // given
            FhirRecordDifference randomFhirRecordDifference = CreateRandomFhirRecordDifference();
            FhirRecordDifference alreadyExistsFhirRecordDifference = randomFhirRecordDifference;
            string randomMessage = GetRandomString();

            var duplicateKeyException =
                new DuplicateKeyException(randomMessage);

            var alreadyExistsFhirRecordDifferenceException =
                new AlreadyExistsFhirRecordDifferenceException(
                    message: "FhirRecordDifference with the same Id already exists.",
                    innerException: duplicateKeyException);

            var expectedFhirRecordDifferenceDependencyValidationException =
                new FhirRecordDifferenceDependencyValidationException(
                    message: "FhirRecordDifference dependency validation occurred, please try again.",
                    innerException: alreadyExistsFhirRecordDifferenceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<FhirRecordDifference>()))
                    .ThrowsAsync(duplicateKeyException);

            // when
            ValueTask<FhirRecordDifference> addFhirRecordDifferenceTask =
                this.fhirRecordDifferenceService.AddFhirRecordDifferenceAsync(alreadyExistsFhirRecordDifference);

            // then
            FhirRecordDifferenceDependencyValidationException actualFhirRecordDifferenceDependencyValidationException =
                await Assert.ThrowsAsync<FhirRecordDifferenceDependencyValidationException>(
                    addFhirRecordDifferenceTask.AsTask);

            actualFhirRecordDifferenceDependencyValidationException.Should()
                .BeEquivalentTo(expectedFhirRecordDifferenceDependencyValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<FhirRecordDifference>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedFhirRecordDifferenceDependencyValidationException))),
                        Times.Once);

            this.securityBrokerMock.Verify(broker =>
                broker.GetCurrentUserAsync(),
                    Times.Never);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never());

            this.storageBrokerMock.Verify(broker =>
                broker.InsertFhirRecordDifferenceAsync(It.IsAny<FhirRecordDifference>()),
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
            FhirRecordDifference someFhirRecordDifference = CreateRandomFhirRecordDifference();
            string randomMessage = GetRandomString();
            string exceptionMessage = randomMessage;

            var foreignKeyConstraintConflictException =
                new ForeignKeyConstraintConflictException(exceptionMessage);

            var invalidFhirRecordDifferenceReferenceException =
                new InvalidReferenceFhirRecordDifferenceException(
                    message: "Invalid fhirRecordDifference reference error occurred.",
                    innerException: foreignKeyConstraintConflictException);

            var expectedFhirRecordDifferenceValidationException =
                new FhirRecordDifferenceDependencyValidationException(
                    message: "FhirRecordDifference dependency validation occurred, please try again.",
                    innerException: invalidFhirRecordDifferenceReferenceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<FhirRecordDifference>()))
                    .ThrowsAsync(foreignKeyConstraintConflictException);

            // when
            ValueTask<FhirRecordDifference> addFhirRecordDifferenceTask =
                this.fhirRecordDifferenceService.AddFhirRecordDifferenceAsync(someFhirRecordDifference);

            // then
            FhirRecordDifferenceDependencyValidationException actualFhirRecordDifferenceDependencyValidationException =
                await Assert.ThrowsAsync<FhirRecordDifferenceDependencyValidationException>(
                    addFhirRecordDifferenceTask.AsTask);

            actualFhirRecordDifferenceDependencyValidationException.Should()
                .BeEquivalentTo(expectedFhirRecordDifferenceValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<FhirRecordDifference>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedFhirRecordDifferenceValidationException))),
                        Times.Once);

            this.securityBrokerMock.Verify(broker =>
                broker.GetCurrentUserAsync(),
                    Times.Never);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never());

            this.storageBrokerMock.Verify(broker =>
                broker.InsertFhirRecordDifferenceAsync(It.IsAny<FhirRecordDifference>()),
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
            FhirRecordDifference someFhirRecordDifference = CreateRandomFhirRecordDifference();

            var databaseUpdateException =
                new DbUpdateException();

            var failedFhirRecordDifferenceStorageException =
                new FailedStorageFhirRecordDifferenceException(
                    message: "Failed fhirRecordDifference storage error occurred, contact support.",
                    innerException: databaseUpdateException);

            var expectedFhirRecordDifferenceDependencyException =
                new FhirRecordDifferenceDependencyException(
                    message: "FhirRecordDifference dependency error occurred, contact support.",
                    innerException: failedFhirRecordDifferenceStorageException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<FhirRecordDifference>()))
                    .ThrowsAsync(databaseUpdateException);

            // when
            ValueTask<FhirRecordDifference> addFhirRecordDifferenceTask =
                this.fhirRecordDifferenceService.AddFhirRecordDifferenceAsync(someFhirRecordDifference);

            FhirRecordDifferenceDependencyException actualFhirRecordDifferenceDependencyException =
                await Assert.ThrowsAsync<FhirRecordDifferenceDependencyException>(
                    addFhirRecordDifferenceTask.AsTask);

            // then
            actualFhirRecordDifferenceDependencyException.Should()
                .BeEquivalentTo(expectedFhirRecordDifferenceDependencyException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<FhirRecordDifference>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedFhirRecordDifferenceDependencyException))),
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
            FhirRecordDifference someFhirRecordDifference = CreateRandomFhirRecordDifference();
            var serviceException = new Exception();

            var failedFhirRecordDifferenceServiceException =
                new FailedFhirRecordDifferenceServiceException(
                    message: "Failed fhirRecordDifference service occurred, please contact support",
                    innerException: serviceException);

            var expectedFhirRecordDifferenceServiceException =
                new FhirRecordDifferenceServiceException(
                    message: "FhirRecordDifference service error occurred, contact support.",
                    innerException: failedFhirRecordDifferenceServiceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<FhirRecordDifference>()))
                    .ThrowsAsync(serviceException);

            // when
            ValueTask<FhirRecordDifference> addFhirRecordDifferenceTask =
                this.fhirRecordDifferenceService.AddFhirRecordDifferenceAsync(someFhirRecordDifference);

            FhirRecordDifferenceServiceException actualFhirRecordDifferenceServiceException =
                await Assert.ThrowsAsync<FhirRecordDifferenceServiceException>(
                    addFhirRecordDifferenceTask.AsTask);

            // then
            actualFhirRecordDifferenceServiceException.Should()
                .BeEquivalentTo(expectedFhirRecordDifferenceServiceException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<FhirRecordDifference>()),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertFhirRecordDifferenceAsync(It.IsAny<FhirRecordDifference>()),
                    Times.Never);

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