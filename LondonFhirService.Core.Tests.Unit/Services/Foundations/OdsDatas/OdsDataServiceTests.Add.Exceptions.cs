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
        public async Task ShouldThrowCriticalDependencyExceptionOnAddIfSqlErrorOccursAndLogItAsync()
        {
            // given
            OdsData someOdsData = CreateRandomOdsData();
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
                broker.InsertOdsDataAsync(It.IsAny<OdsData>()))
                    .ThrowsAsync(sqlException);

            // when
            ValueTask<OdsData> addOdsDataTask =
                this.odsDataService.AddOdsDataAsync(someOdsData);

            OdsDataServiceDependencyException actualOdsDataDependencyException =
                await Assert.ThrowsAsync<OdsDataServiceDependencyException>(
                    testCode: addOdsDataTask.AsTask);

            // then
            actualOdsDataDependencyException.Should()
                .BeEquivalentTo(expectedOdsDataDependencyException);

            this.storageBroker.Verify(broker =>
                broker.InsertOdsDataAsync(It.IsAny<OdsData>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedOdsDataDependencyException))),
                        Times.Once);

            this.storageBroker.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyValidationExceptionOnAddIfOdsDataAlreadyExsitsAndLogItAsync()
        {
            // given
            OdsData randomOdsData = CreateRandomOdsData();
            OdsData alreadyExistsOdsData = randomOdsData;
            string randomMessage = GetRandomString();

            var duplicateKeyException =
                new DuplicateKeyException(randomMessage);

            var alreadyExistsOdsDataException =
                new AlreadyExistsOdsDataServiceException(
                    message: "OdsData with the same Id already exists.",
                    innerException: duplicateKeyException,
                    data: duplicateKeyException.Data);

            var expectedOdsDataDependencyValidationException =
                new OdsDataServiceDependencyValidationException(
                    message: "OdsData dependency validation occurred, please try again.",
                    innerException: alreadyExistsOdsDataException);

            this.storageBroker.Setup(broker =>
                broker.InsertOdsDataAsync(It.IsAny<OdsData>()))
                    .ThrowsAsync(duplicateKeyException);

            // when
            ValueTask<OdsData> addOdsDataTask =
                this.odsDataService.AddOdsDataAsync(alreadyExistsOdsData);

            // then
            OdsDataServiceDependencyValidationException actualOdsDataDependencyValidationException =
                await Assert.ThrowsAsync<OdsDataServiceDependencyValidationException>(
                    testCode: addOdsDataTask.AsTask);

            actualOdsDataDependencyValidationException.Should()
                .BeEquivalentTo(expectedOdsDataDependencyValidationException);

            this.storageBroker.Verify(broker =>
                broker.InsertOdsDataAsync(It.IsAny<OdsData>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedOdsDataDependencyValidationException))),
                        Times.Once);

            this.storageBroker.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnAddIfReferenceErrorOccursAndLogItAsync()
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

            var expectedOdsDataValidationException =
                new OdsDataServiceDependencyValidationException(
                    message: "OdsData dependency validation occurred, please try again.",
                    innerException: invalidOdsDataReferenceException);

            this.storageBroker.Setup(broker =>
                broker.InsertOdsDataAsync(someOdsData))
                    .ThrowsAsync(foreignKeyConstraintConflictException);

            // when
            ValueTask<OdsData> addOdsDataTask =
                this.odsDataService.AddOdsDataAsync(someOdsData);

            // then
            OdsDataServiceDependencyValidationException actualOdsDataDependencyValidationException =
                await Assert.ThrowsAsync<OdsDataServiceDependencyValidationException>(
                    testCode: addOdsDataTask.AsTask);

            actualOdsDataDependencyValidationException.Should()
                .BeEquivalentTo(expectedOdsDataValidationException);

            this.storageBroker.Verify(broker =>
                broker.InsertOdsDataAsync(someOdsData),
                    Times.Once());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedOdsDataValidationException))),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBroker.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyExceptionOnAddIfDatabaseUpdateErrorOccursAndLogItAsync()
        {
            // given
            OdsData someOdsData = CreateRandomOdsData();

            var databaseUpdateException =
                new DbUpdateException();

            var failedOperationOdsDataException =
                new FailedOperationOdsDataServiceException(
                    message: "Failed odsData operation error occurred, contact support.",
                    innerException: databaseUpdateException);

            var expectedOdsDataDependencyException =
                new OdsDataServiceDependencyException(
                    message: "OdsData dependency error occurred, contact support.",
                    innerException: failedOperationOdsDataException);

            this.storageBroker.Setup(broker =>
                broker.InsertOdsDataAsync(It.IsAny<OdsData>()))
                    .ThrowsAsync(databaseUpdateException);

            // when
            ValueTask<OdsData> addOdsDataTask =
                this.odsDataService.AddOdsDataAsync(someOdsData);

            OdsDataServiceDependencyException actualOdsDataDependencyException =
                await Assert.ThrowsAsync<OdsDataServiceDependencyException>(
                    testCode: addOdsDataTask.AsTask);

            // then
            actualOdsDataDependencyException.Should()
                .BeEquivalentTo(expectedOdsDataDependencyException);

            this.storageBroker.Verify(broker =>
                broker.InsertOdsDataAsync(It.IsAny<OdsData>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedOdsDataDependencyException))),
                        Times.Once);

            this.storageBroker.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnAddIfServiceErrorOccursAndLogItAsync()
        {
            // given
            OdsData someOdsData = CreateRandomOdsData();
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
                broker.InsertOdsDataAsync(It.IsAny<OdsData>()))
                    .ThrowsAsync(serviceException);

            // when
            ValueTask<OdsData> addOdsDataTask =
                this.odsDataService.AddOdsDataAsync(someOdsData);

            OdsDataServiceException actualOdsDataServiceException =
                await Assert.ThrowsAsync<OdsDataServiceException>(
                    testCode: addOdsDataTask.AsTask);

            // then
            actualOdsDataServiceException.Should()
                .BeEquivalentTo(expectedOdsDataServiceException);

            this.storageBroker.Verify(broker =>
                broker.InsertOdsDataAsync(It.IsAny<OdsData>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedOdsDataServiceException))),
                        Times.Once);

            this.storageBroker.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}