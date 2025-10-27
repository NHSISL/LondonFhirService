// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
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
        public async Task ShouldThrowCriticalDependencyExceptionOnRemoveIfSqlErrorOccursAndLogItAsync()
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
            ValueTask<OdsData> addOdsDataTask =
                this.odsDataService.RemoveOdsDataByIdAsync(randomOdsData.Id);

            OdsDataServiceDependencyException actualOdsDataDependencyException =
                await Assert.ThrowsAsync<OdsDataServiceDependencyException>(
                    testCode: addOdsDataTask.AsTask);

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
                broker.DeleteOdsDataAsync(It.IsAny<OdsData>()),
                    Times.Never);

            this.storageBroker.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyValidationOnRemoveIfDatabaseUpdateConcurrencyErrorOccursAndLogItAsync()
        {
            // given
            Guid someOdsDataId = Guid.NewGuid();

            var databaseUpdateConcurrencyException =
                new DbUpdateConcurrencyException();

            var lockedOdsDataException =
                new LockedOdsDataServiceException(
                    message: "Locked odsData record exception, please try again later",
                    innerException: databaseUpdateConcurrencyException);

            var expectedOdsDataDependencyValidationException =
                new OdsDataServiceDependencyValidationException(
                    message: "OdsData dependency validation occurred, please try again.",
                    innerException: lockedOdsDataException);

            this.storageBroker.Setup(broker =>
                broker.SelectOdsDataByIdAsync(It.IsAny<Guid>()))
                    .ThrowsAsync(databaseUpdateConcurrencyException);

            // when
            ValueTask<OdsData> removeOdsDataByIdTask =
                this.odsDataService.RemoveOdsDataByIdAsync(someOdsDataId);

            OdsDataServiceDependencyValidationException actualOdsDataDependencyValidationException =
                await Assert.ThrowsAsync<OdsDataServiceDependencyValidationException>(
                    testCode: removeOdsDataByIdTask.AsTask);

            // then
            actualOdsDataDependencyValidationException.Should()
                .BeEquivalentTo(expectedOdsDataDependencyValidationException);

            this.storageBroker.Verify(broker =>
                broker.SelectOdsDataByIdAsync(It.IsAny<Guid>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedOdsDataDependencyValidationException))),
                        Times.Once);

            this.storageBroker.Verify(broker =>
                broker.DeleteOdsDataAsync(It.IsAny<OdsData>()),
                    Times.Never);

            this.storageBroker.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyExceptionOnRemoveWhenSqlExceptionOccursAndLogItAsync()
        {
            // given
            Guid someOdsDataId = Guid.NewGuid();
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
                broker.SelectOdsDataByIdAsync(It.IsAny<Guid>()))
                    .ThrowsAsync(sqlException);

            // when
            ValueTask<OdsData> deleteOdsDataTask =
                this.odsDataService.RemoveOdsDataByIdAsync(someOdsDataId);

            OdsDataServiceDependencyException actualOdsDataDependencyException =
                await Assert.ThrowsAsync<OdsDataServiceDependencyException>(
                    testCode: deleteOdsDataTask.AsTask);

            // then
            actualOdsDataDependencyException.Should()
                .BeEquivalentTo(expectedOdsDataDependencyException);

            this.storageBroker.Verify(broker =>
                broker.SelectOdsDataByIdAsync(It.IsAny<Guid>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedOdsDataDependencyException))),
                        Times.Once);

            this.storageBroker.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnRemoveIfServiceErrorOccursAndLogItAsync()
        {
            // given
            Guid someOdsDataId = Guid.NewGuid();
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
                broker.SelectOdsDataByIdAsync(It.IsAny<Guid>()))
                    .ThrowsAsync(serviceException);

            // when
            ValueTask<OdsData> removeOdsDataByIdTask =
                this.odsDataService.RemoveOdsDataByIdAsync(someOdsDataId);

            OdsDataServiceException actualOdsDataServiceException =
                await Assert.ThrowsAsync<OdsDataServiceException>(
                    testCode: removeOdsDataByIdTask.AsTask);

            // then
            actualOdsDataServiceException.Should()
                .BeEquivalentTo(expectedOdsDataServiceException);

            this.storageBroker.Verify(broker =>
                broker.SelectOdsDataByIdAsync(It.IsAny<Guid>()),
                        Times.Once());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedOdsDataServiceException))),
                        Times.Once);

            this.storageBroker.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}