// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using EFxceptions.Models.Exceptions;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.PdsDatas;
using LondonFhirService.Core.Models.Foundations.PdsDatas.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.PdsDatas
{
    public partial class PdsDataServiceTests
    {
        [Fact]
        public async Task ShouldThrowCriticalDependencyExceptionOnAddIfSqlErrorOccursAndLogItAsync()
        {
            // given
            PdsData somePdsData = CreateRandomPdsData();
            SqlException sqlException = CreateSqlException();

            var failedStoragePdsDataException =
                new FailedStoragePdsDataServiceException(
                    message: "Failed pdsData storage error occurred, contact support.",
                    innerException: sqlException);

            var expectedPdsDataDependencyException =
                new PdsDataServiceDependencyException(
                    message: "PdsData dependency error occurred, contact support.",
                    innerException: failedStoragePdsDataException);

            this.storageBroker.Setup(broker =>
                broker.InsertPdsDataAsync(It.IsAny<PdsData>()))
                    .ThrowsAsync(sqlException);

            // when
            ValueTask<PdsData> addPdsDataTask =
                this.pdsDataService.AddPdsDataAsync(somePdsData);

            PdsDataServiceDependencyException actualPdsDataDependencyException =
                await Assert.ThrowsAsync<PdsDataServiceDependencyException>(
                    testCode: addPdsDataTask.AsTask);

            // then
            actualPdsDataDependencyException.Should()
                .BeEquivalentTo(expectedPdsDataDependencyException);

            this.storageBroker.Verify(broker =>
                broker.InsertPdsDataAsync(It.IsAny<PdsData>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedPdsDataDependencyException))),
                        Times.Once);

            this.storageBroker.VerifyNoOtherCalls();
            this.dateTimeBroker.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyValidationExceptionOnAddIfPdsDataAlreadyExsitsAndLogItAsync()
        {
            // given
            PdsData randomPdsData = CreateRandomPdsData();
            PdsData alreadyExistsPdsData = randomPdsData;
            string randomMessage = GetRandomString();

            var duplicateKeyException =
                new DuplicateKeyException(randomMessage);

            var alreadyExistsPdsDataException =
                new AlreadyExistsPdsDataServiceException(
                    message: "PdsData with the same Id already exists.",
                    innerException: duplicateKeyException,
                    data: duplicateKeyException.Data);

            var expectedPdsDataDependencyValidationException =
                new PdsDataServiceDependencyValidationException(
                    message: "PdsData dependency validation occurred, please try again.",
                    innerException: alreadyExistsPdsDataException);

            this.storageBroker.Setup(broker =>
                broker.InsertPdsDataAsync(It.IsAny<PdsData>()))
                    .ThrowsAsync(duplicateKeyException);

            // when
            ValueTask<PdsData> addPdsDataTask =
                this.pdsDataService.AddPdsDataAsync(alreadyExistsPdsData);

            // then
            PdsDataServiceDependencyValidationException actualPdsDataDependencyValidationException =
                await Assert.ThrowsAsync<PdsDataServiceDependencyValidationException>(
                    testCode: addPdsDataTask.AsTask);

            actualPdsDataDependencyValidationException.Should()
                .BeEquivalentTo(expectedPdsDataDependencyValidationException);

            this.storageBroker.Verify(broker =>
                broker.InsertPdsDataAsync(It.IsAny<PdsData>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedPdsDataDependencyValidationException))),
                        Times.Once);

            this.storageBroker.VerifyNoOtherCalls();
            this.dateTimeBroker.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnAddIfReferenceErrorOccursAndLogItAsync()
        {
            // given
            PdsData somePdsData = CreateRandomPdsData();
            string randomMessage = GetRandomString();
            string exceptionMessage = randomMessage;

            var foreignKeyConstraintConflictException =
                new ForeignKeyConstraintConflictException(exceptionMessage);

            var invalidPdsDataReferenceException =
                new InvalidReferencePdsDataServiceException(
                    message: "Invalid pdsData reference error occurred.",
                    innerException: foreignKeyConstraintConflictException);

            var expectedPdsDataValidationException =
                new PdsDataServiceDependencyValidationException(
                    message: "PdsData dependency validation occurred, please try again.",
                    innerException: invalidPdsDataReferenceException);

            this.storageBroker.Setup(broker =>
                broker.InsertPdsDataAsync(somePdsData))
                    .ThrowsAsync(foreignKeyConstraintConflictException);

            // when
            ValueTask<PdsData> addPdsDataTask =
                this.pdsDataService.AddPdsDataAsync(somePdsData);

            // then
            PdsDataServiceDependencyValidationException actualPdsDataDependencyValidationException =
                await Assert.ThrowsAsync<PdsDataServiceDependencyValidationException>(
                    testCode: addPdsDataTask.AsTask);

            actualPdsDataDependencyValidationException.Should()
                .BeEquivalentTo(expectedPdsDataValidationException);

            this.storageBroker.Verify(broker =>
                broker.InsertPdsDataAsync(somePdsData),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedPdsDataValidationException))),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBroker.VerifyNoOtherCalls();
            this.storageBroker.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyExceptionOnAddIfDatabaseUpdateErrorOccursAndLogItAsync()
        {
            // given
            PdsData somePdsData = CreateRandomPdsData();

            var databaseUpdateException =
                new DbUpdateException();

            var failedOperationPdsDataException =
                new FailedOperationPdsDataServiceException(
                    message: "Failed pdsData operation error occurred, contact support.",
                    innerException: databaseUpdateException);

            var expectedPdsDataDependencyException =
                new PdsDataServiceDependencyException(
                    message: "PdsData dependency error occurred, contact support.",
                    innerException: failedOperationPdsDataException);

            this.storageBroker.Setup(broker =>
                broker.InsertPdsDataAsync(It.IsAny<PdsData>()))
                    .ThrowsAsync(databaseUpdateException);

            // when
            ValueTask<PdsData> addPdsDataTask =
                this.pdsDataService.AddPdsDataAsync(somePdsData);

            PdsDataServiceDependencyException actualPdsDataDependencyException =
                await Assert.ThrowsAsync<PdsDataServiceDependencyException>(
                    testCode: addPdsDataTask.AsTask);

            // then
            actualPdsDataDependencyException.Should()
                .BeEquivalentTo(expectedPdsDataDependencyException);

            this.storageBroker.Verify(broker =>
                broker.InsertPdsDataAsync(It.IsAny<PdsData>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedPdsDataDependencyException))),
                        Times.Once);

            this.storageBroker.VerifyNoOtherCalls();
            this.dateTimeBroker.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnAddIfServiceErrorOccursAndLogItAsync()
        {
            // given
            PdsData somePdsData = CreateRandomPdsData();
            var serviceException = new Exception();

            var failedPdsDataServiceException =
                new FailedPdsDataServiceException(
                    message: "Failed pdsData service occurred, please contact support",
                    innerException: serviceException);

            var expectedPdsDataServiceException =
                new PdsDataServiceException(
                    message: "PdsData service error occurred, contact support.",
                    innerException: failedPdsDataServiceException);

            this.storageBroker.Setup(broker =>
                broker.InsertPdsDataAsync(It.IsAny<PdsData>()))
                    .ThrowsAsync(serviceException);

            // when
            ValueTask<PdsData> addPdsDataTask =
                this.pdsDataService.AddPdsDataAsync(somePdsData);

            PdsDataServiceException actualPdsDataServiceException =
                await Assert.ThrowsAsync<PdsDataServiceException>(
                    testCode: addPdsDataTask.AsTask);

            // then
            actualPdsDataServiceException.Should()
                .BeEquivalentTo(expectedPdsDataServiceException);

            this.storageBroker.Verify(broker =>
                broker.InsertPdsDataAsync(It.IsAny<PdsData>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedPdsDataServiceException))),
                        Times.Once);

            this.storageBroker.VerifyNoOtherCalls();
            this.dateTimeBroker.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}