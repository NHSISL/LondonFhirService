// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using EFxceptions.Models.Exceptions;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.OdsDatas;
using LondonFhirService.Core.Models.Foundations.OdsDatas.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.OdsDatas
{
    public partial class OdsDataServiceTests
    {
        [Fact]
        public async Task ShouldThrowCriticalDependencyExceptionOnModifyIfSqlErrorOccursAndLogItAsync()
        {
            // given
            OdsData randomOdsData = CreateRandomOdsData();
            SqlException sqlException = CreateSqlException();

            var failedStorageOdsDataException =
                new FailedStorageOdsDataServiceException(
                    message: "Failed odsData storage error occurred, contact support.",
                    innerException: sqlException);

            var expectedOdsDataDependencyException =
                new OdsDataServiceDependencyException(
                    message: "OdsData dependency error occurred, contact support.",
                    innerException: failedStorageOdsDataException);

            this.storageBroker.Setup(broker =>
                broker.SelectOdsDataByIdAsync(randomOdsData.Id))
                    .ThrowsAsync(sqlException);

            // when
            ValueTask<OdsData> modifyOdsDataTask =
                this.odsDataService.ModifyOdsDataAsync(randomOdsData);

            OdsDataServiceDependencyException actualOdsDataDependencyException =
                await Assert.ThrowsAsync<OdsDataServiceDependencyException>(
                    testCode: modifyOdsDataTask.AsTask);

            // then
            actualOdsDataDependencyException.Should()
                .BeEquivalentTo(expectedOdsDataDependencyException);

            this.storageBroker.Verify(broker =>
                broker.SelectOdsDataByIdAsync(randomOdsData.Id),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedOdsDataDependencyException))),
                        Times.Once);

            this.storageBroker.Verify(broker =>
                broker.UpdateOdsDataAsync(randomOdsData),
                    Times.Never);

            this.storageBroker.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnModifyIfReferenceErrorOccursAndLogItAsync()
        {
            // given
            OdsData someOdsData = CreateRandomOdsData();
            string randomMessage = GetRandomString();
            string exceptionMessage = randomMessage;

            var foreignKeyConstraintConflictException =
                new ForeignKeyConstraintConflictException(exceptionMessage);

            var invalidOdsDataReferenceException =
                new InvalidReferenceOdsDataServiceException(
                    message: "Invalid odsData reference error occurred.",
                    innerException: foreignKeyConstraintConflictException);

            OdsDataServiceDependencyValidationException expectedOdsDataDependencyValidationException =
                new OdsDataServiceDependencyValidationException(
                    message: "OdsData dependency validation occurred, please try again.",
                    innerException: invalidOdsDataReferenceException);

            this.storageBroker.Setup(broker =>
                broker.SelectOdsDataByIdAsync(someOdsData.Id))
                    .ThrowsAsync(foreignKeyConstraintConflictException);

            // when
            ValueTask<OdsData> modifyOdsDataTask =
                this.odsDataService.ModifyOdsDataAsync(someOdsData);

            OdsDataServiceDependencyValidationException actualOdsDataDependencyValidationException =
                await Assert.ThrowsAsync<OdsDataServiceDependencyValidationException>(
                    testCode: modifyOdsDataTask.AsTask);

            // then
            actualOdsDataDependencyValidationException.Should()
                .BeEquivalentTo(expectedOdsDataDependencyValidationException);

            this.storageBroker.Verify(broker =>
                broker.SelectOdsDataByIdAsync(someOdsData.Id),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(expectedOdsDataDependencyValidationException))),
                    Times.Once);

            this.storageBroker.Verify(broker =>
                broker.UpdateOdsDataAsync(someOdsData),
                    Times.Never);

            this.storageBroker.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyExceptionOnModifyIfDatabaseUpdateExceptionOccursAndLogItAsync()
        {
            // given
            OdsData randomOdsData = CreateRandomOdsData();
            var databaseUpdateException = new DbUpdateException();

            var failedOperationOdsDataException =
                new FailedOperationOdsDataServiceException(
                    message: "Failed odsData operation error occurred, contact support.",
                    innerException: databaseUpdateException);

            var expectedOdsDataDependencyException =
                new OdsDataServiceDependencyException(
                    message: "OdsData dependency error occurred, contact support.",
                    innerException: failedOperationOdsDataException);

            this.storageBroker.Setup(broker =>
                broker.SelectOdsDataByIdAsync(randomOdsData.Id))
                    .ThrowsAsync(databaseUpdateException);

            // when
            ValueTask<OdsData> modifyOdsDataTask =
                this.odsDataService.ModifyOdsDataAsync(randomOdsData);

            OdsDataServiceDependencyException actualOdsDataDependencyException =
                await Assert.ThrowsAsync<OdsDataServiceDependencyException>(
                    testCode: modifyOdsDataTask.AsTask);

            // then
            actualOdsDataDependencyException.Should()
                .BeEquivalentTo(expectedOdsDataDependencyException);

            this.storageBroker.Verify(broker =>
                broker.SelectOdsDataByIdAsync(randomOdsData.Id),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedOdsDataDependencyException))),
                        Times.Once);

            this.storageBroker.Verify(broker =>
                broker.UpdateOdsDataAsync(randomOdsData),
                    Times.Never);

            this.storageBroker.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyValidationExceptionOnModifyIfDbUpdateConcurrencyErrorOccursAndLogAsync()
        {
            // given
            OdsData randomOdsData = CreateRandomOdsData();
            var databaseUpdateConcurrencyException = new DbUpdateConcurrencyException();

            var lockedOdsDataException =
                new LockedOdsDataServiceException(
                    message: "Locked odsData record exception, please try again later",
                    innerException: databaseUpdateConcurrencyException);

            var expectedOdsDataDependencyValidationException =
                new OdsDataServiceDependencyValidationException(
                    message: "OdsData dependency validation occurred, please try again.",
                    innerException: lockedOdsDataException);

            this.storageBroker.Setup(broker =>
                broker.SelectOdsDataByIdAsync(randomOdsData.Id))
                    .ThrowsAsync(databaseUpdateConcurrencyException);

            // when
            ValueTask<OdsData> modifyOdsDataTask =
                this.odsDataService.ModifyOdsDataAsync(randomOdsData);

            OdsDataServiceDependencyValidationException actualOdsDataDependencyValidationException =
                await Assert.ThrowsAsync<OdsDataServiceDependencyValidationException>(
                    testCode: modifyOdsDataTask.AsTask);

            // then
            actualOdsDataDependencyValidationException.Should()
                .BeEquivalentTo(expectedOdsDataDependencyValidationException);

            this.storageBroker.Verify(broker =>
                broker.SelectOdsDataByIdAsync(randomOdsData.Id),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedOdsDataDependencyValidationException))),
                        Times.Once);

            this.storageBroker.Verify(broker =>
                broker.UpdateOdsDataAsync(randomOdsData),
                    Times.Never);

            this.storageBroker.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnModifyIfServiceErrorOccursAndLogItAsync()
        {
            // given
            OdsData randomOdsData = CreateRandomOdsData();
            var serviceException = new Exception();

            var failedOdsDataServiceException =
                new FailedOdsDataServiceException(
                    message: "Failed odsData service occurred, please contact support",
                    innerException: serviceException);

            var expectedOdsDataServiceException =
                new OdsDataServiceException(
                    message: "OdsData service error occurred, contact support.",
                    innerException: failedOdsDataServiceException);

            this.storageBroker.Setup(broker =>
                broker.SelectOdsDataByIdAsync(randomOdsData.Id))
                    .ThrowsAsync(serviceException);

            // when
            ValueTask<OdsData> modifyOdsDataTask =
                this.odsDataService.ModifyOdsDataAsync(randomOdsData);

            OdsDataServiceException actualOdsDataServiceException =
                await Assert.ThrowsAsync<OdsDataServiceException>(
                    testCode: modifyOdsDataTask.AsTask);

            // then
            actualOdsDataServiceException.Should()
                .BeEquivalentTo(expectedOdsDataServiceException);

            this.storageBroker.Verify(broker =>
                broker.SelectOdsDataByIdAsync(randomOdsData.Id),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedOdsDataServiceException))),
                        Times.Once);

            this.storageBroker.Verify(broker =>
                broker.UpdateOdsDataAsync(randomOdsData),
                    Times.Never);

            this.storageBroker.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}