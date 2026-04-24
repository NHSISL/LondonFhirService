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
        public async Task ShouldThrowCriticalDependencyExceptionOnModifyIfSqlErrorOccursAndLogItAsync()
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

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<FhirRecordDifference>()))
                    .ThrowsAsync(sqlException);

            // when
            ValueTask<FhirRecordDifference> modifyFhirRecordDifferenceTask =
                this.fhirRecordDifferenceService.ModifyFhirRecordDifferenceAsync(randomFhirRecordDifference);

            FhirRecordDifferenceDependencyException actualFhirRecordDifferenceDependencyException =
                await Assert.ThrowsAsync<FhirRecordDifferenceDependencyException>(
                    modifyFhirRecordDifferenceTask.AsTask);

            // then
            actualFhirRecordDifferenceDependencyException.Should()
                .BeEquivalentTo(expectedFhirRecordDifferenceDependencyException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<FhirRecordDifference>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedFhirRecordDifferenceDependencyException))),
                        Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Never);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectFhirRecordDifferenceByIdAsync(It.IsAny<Guid>()),
                    Times.Never);

            this.securityAuditBrokerMock.Verify(broker => broker
                .EnsureAddAuditValuesRemainsUnchangedOnModifyAsync(It.IsAny<FhirRecordDifference>(), It.IsAny<FhirRecordDifference>()),
                    Times.Never);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateFhirRecordDifferenceAsync(It.IsAny<FhirRecordDifference>()),
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
            FhirRecordDifference someFhirRecordDifference = CreateRandomFhirRecordDifference();
            string randomMessage = GetRandomString();
            string exceptionMessage = randomMessage;

            var foreignKeyConstraintConflictException =
                new ForeignKeyConstraintConflictException(exceptionMessage);

            var invalidFhirRecordDifferenceReferenceException =
                new InvalidReferenceFhirRecordDifferenceException(
                    message: "Invalid fhirRecordDifference reference error occurred.",
                    innerException: foreignKeyConstraintConflictException);

            FhirRecordDifferenceDependencyValidationException expectedFhirRecordDifferenceDependencyValidationException =
                new FhirRecordDifferenceDependencyValidationException(
                    message: "FhirRecordDifference dependency validation occurred, please try again.",
                    innerException: invalidFhirRecordDifferenceReferenceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<FhirRecordDifference>()))
                    .ThrowsAsync(foreignKeyConstraintConflictException);

            // when
            ValueTask<FhirRecordDifference> modifyFhirRecordDifferenceTask =
                this.fhirRecordDifferenceService.ModifyFhirRecordDifferenceAsync(someFhirRecordDifference);

            FhirRecordDifferenceDependencyValidationException actualFhirRecordDifferenceDependencyValidationException =
                await Assert.ThrowsAsync<FhirRecordDifferenceDependencyValidationException>(
                    modifyFhirRecordDifferenceTask.AsTask);

            // then
            actualFhirRecordDifferenceDependencyValidationException.Should()
                .BeEquivalentTo(expectedFhirRecordDifferenceDependencyValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<FhirRecordDifference>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(expectedFhirRecordDifferenceDependencyValidationException))),
                    Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Never);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectFhirRecordDifferenceByIdAsync(It.IsAny<Guid>()),
                    Times.Never);

            this.securityAuditBrokerMock.Verify(broker => broker
                .EnsureAddAuditValuesRemainsUnchangedOnModifyAsync(It.IsAny<FhirRecordDifference>(), It.IsAny<FhirRecordDifference>()),
                    Times.Never);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateFhirRecordDifferenceAsync(It.IsAny<FhirRecordDifference>()),
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
            FhirRecordDifference randomFhirRecordDifference = CreateRandomFhirRecordDifference();
            var databaseUpdateException = new DbUpdateException();

            var failedFhirRecordDifferenceStorageException =
                new FailedStorageFhirRecordDifferenceException(
                    message: "Failed fhirRecordDifference storage error occurred, contact support.",
                    innerException: databaseUpdateException);

            var expectedFhirRecordDifferenceDependencyException =
                new FhirRecordDifferenceDependencyException(
                    message: "FhirRecordDifference dependency error occurred, contact support.",
                    innerException: failedFhirRecordDifferenceStorageException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<FhirRecordDifference>()))
                    .ThrowsAsync(databaseUpdateException);

            // when
            ValueTask<FhirRecordDifference> modifyFhirRecordDifferenceTask =
                this.fhirRecordDifferenceService.ModifyFhirRecordDifferenceAsync(randomFhirRecordDifference);

            FhirRecordDifferenceDependencyException actualFhirRecordDifferenceDependencyException =
                await Assert.ThrowsAsync<FhirRecordDifferenceDependencyException>(
                    modifyFhirRecordDifferenceTask.AsTask);

            // then
            actualFhirRecordDifferenceDependencyException.Should()
                .BeEquivalentTo(expectedFhirRecordDifferenceDependencyException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<FhirRecordDifference>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedFhirRecordDifferenceDependencyException))),
                        Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Never);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectFhirRecordDifferenceByIdAsync(It.IsAny<Guid>()),
                    Times.Never);

            this.securityAuditBrokerMock.Verify(broker => broker
                .EnsureAddAuditValuesRemainsUnchangedOnModifyAsync(It.IsAny<FhirRecordDifference>(), It.IsAny<FhirRecordDifference>()),
                    Times.Never);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateFhirRecordDifferenceAsync(It.IsAny<FhirRecordDifference>()),
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
            FhirRecordDifference randomFhirRecordDifference = CreateRandomFhirRecordDifference();
            var databaseUpdateConcurrencyException = new DbUpdateConcurrencyException();

            var lockedFhirRecordDifferenceException =
                new LockedFhirRecordDifferenceException(
                    message: "Locked fhirRecordDifference record exception, please try again later",
                    innerException: databaseUpdateConcurrencyException);

            var expectedFhirRecordDifferenceDependencyValidationException =
                new FhirRecordDifferenceDependencyValidationException(
                    message: "FhirRecordDifference dependency validation occurred, please try again.",
                    innerException: lockedFhirRecordDifferenceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<FhirRecordDifference>()))
                    .ThrowsAsync(databaseUpdateConcurrencyException);

            // when
            ValueTask<FhirRecordDifference> modifyFhirRecordDifferenceTask =
                this.fhirRecordDifferenceService.ModifyFhirRecordDifferenceAsync(randomFhirRecordDifference);

            FhirRecordDifferenceDependencyValidationException actualFhirRecordDifferenceDependencyValidationException =
                await Assert.ThrowsAsync<FhirRecordDifferenceDependencyValidationException>(
                    modifyFhirRecordDifferenceTask.AsTask);

            // then
            actualFhirRecordDifferenceDependencyValidationException.Should()
                .BeEquivalentTo(expectedFhirRecordDifferenceDependencyValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<FhirRecordDifference>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedFhirRecordDifferenceDependencyValidationException))),
                        Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Never);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectFhirRecordDifferenceByIdAsync(It.IsAny<Guid>()),
                    Times.Never);

            this.securityAuditBrokerMock.Verify(broker => broker
                .EnsureAddAuditValuesRemainsUnchangedOnModifyAsync(It.IsAny<FhirRecordDifference>(), It.IsAny<FhirRecordDifference>()),
                    Times.Never);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateFhirRecordDifferenceAsync(It.IsAny<FhirRecordDifference>()),
                    Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnModifyIfServiceErrorOccursAndLogItAsync()
        {
            // given
            FhirRecordDifference randomFhirRecordDifference = CreateRandomFhirRecordDifference();
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
                broker.ApplyModifyAuditValuesAsync(It.IsAny<FhirRecordDifference>()))
                    .Throws(serviceException);

            // when
            ValueTask<FhirRecordDifference> modifyFhirRecordDifferenceTask =
                this.fhirRecordDifferenceService.ModifyFhirRecordDifferenceAsync(randomFhirRecordDifference);

            FhirRecordDifferenceServiceException actualFhirRecordDifferenceServiceException =
                await Assert.ThrowsAsync<FhirRecordDifferenceServiceException>(
                    modifyFhirRecordDifferenceTask.AsTask);

            // then
            actualFhirRecordDifferenceServiceException.Should()
                .BeEquivalentTo(expectedFhirRecordDifferenceServiceException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<FhirRecordDifference>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedFhirRecordDifferenceServiceException))),
                        Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Never);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectFhirRecordDifferenceByIdAsync(It.IsAny<Guid>()),
                    Times.Never);

            this.securityAuditBrokerMock.Verify(broker => broker
                .EnsureAddAuditValuesRemainsUnchangedOnModifyAsync(It.IsAny<FhirRecordDifference>(), It.IsAny<FhirRecordDifference>()),
                    Times.Never);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateFhirRecordDifferenceAsync(It.IsAny<FhirRecordDifference>()),
                    Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }
    }
}