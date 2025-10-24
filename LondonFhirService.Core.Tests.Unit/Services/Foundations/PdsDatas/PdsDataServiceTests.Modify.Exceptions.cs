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
        public async Task ShouldThrowCriticalDependencyExceptionOnModifyIfSqlErrorOccursAndLogItAsync()
        {
            // given
            PdsData randomPdsData = CreateRandomPdsData();
            SqlException sqlException = CreateSqlException();

            var failedStoragePdsDataException =
                new FailedStoragePdsDataException(
                    message: "Failed pdsData storage error occurred, contact support.",
                    innerException: sqlException);

            var expectedPdsDataDependencyException =
                new PdsDataDependencyException(
                    message: "PdsData dependency error occurred, contact support.",
                    innerException: failedStoragePdsDataException);

            this.storageBroker.Setup(broker =>
                broker.SelectPdsDataByIdAsync(randomPdsData.Id))
                    .ThrowsAsync(sqlException);

            // when
            ValueTask<PdsData> modifyPdsDataTask =
                this.pdsDataService.ModifyPdsDataAsync(randomPdsData);

            PdsDataDependencyException actualPdsDataDependencyException =
                await Assert.ThrowsAsync<PdsDataDependencyException>(
                    testCode: modifyPdsDataTask.AsTask);

            // then
            actualPdsDataDependencyException.Should()
                .BeEquivalentTo(expectedPdsDataDependencyException);

            this.storageBroker.Verify(broker =>
                broker.SelectPdsDataByIdAsync(randomPdsData.Id),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedPdsDataDependencyException))),
                        Times.Once);

            this.storageBroker.Verify(broker =>
                broker.UpdatePdsDataAsync(randomPdsData),
                    Times.Never);

            this.storageBroker.VerifyNoOtherCalls();
            this.dateTimeBroker.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnModifyIfReferenceErrorOccursAndLogItAsync()
        {
            // given
            PdsData somePdsData = CreateRandomPdsData();
            string randomMessage = GetRandomString();
            string exceptionMessage = randomMessage;

            var foreignKeyConstraintConflictException =
                new ForeignKeyConstraintConflictException(exceptionMessage);

            var invalidPdsDataReferenceException =
                new InvalidPdsDataReferenceException(
                    message: "Invalid pdsData reference error occurred.",
                    innerException: foreignKeyConstraintConflictException);

            PdsDataDependencyValidationException expectedPdsDataDependencyValidationException =
                new PdsDataDependencyValidationException(
                    message: "PdsData dependency validation occurred, please try again.",
                    innerException: invalidPdsDataReferenceException);

            this.storageBroker.Setup(broker =>
                broker.SelectPdsDataByIdAsync(somePdsData.Id))
                    .ThrowsAsync(foreignKeyConstraintConflictException);

            // when
            ValueTask<PdsData> modifyPdsDataTask =
                this.pdsDataService.ModifyPdsDataAsync(somePdsData);

            PdsDataDependencyValidationException actualPdsDataDependencyValidationException =
                await Assert.ThrowsAsync<PdsDataDependencyValidationException>(
                    testCode: modifyPdsDataTask.AsTask);

            // then
            actualPdsDataDependencyValidationException.Should()
                .BeEquivalentTo(expectedPdsDataDependencyValidationException);

            this.storageBroker.Verify(broker =>
                broker.SelectPdsDataByIdAsync(somePdsData.Id),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(expectedPdsDataDependencyValidationException))),
                    Times.Once);

            this.storageBroker.Verify(broker =>
                broker.UpdatePdsDataAsync(somePdsData),
                    Times.Never);

            this.storageBroker.VerifyNoOtherCalls();
            this.dateTimeBroker.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyExceptionOnModifyIfDatabaseUpdateExceptionOccursAndLogItAsync()
        {
            // given
            PdsData randomPdsData = CreateRandomPdsData();
            var databaseUpdateException = new DbUpdateException();

            var failedOperationPdsDataException =
                new FailedOperationPdsDataException(
                    message: "Failed pdsData operation error occurred, contact support.",
                    innerException: databaseUpdateException);

            var expectedPdsDataDependencyException =
                new PdsDataDependencyException(
                    message: "PdsData dependency error occurred, contact support.",
                    innerException: failedOperationPdsDataException);

            this.storageBroker.Setup(broker =>
                broker.SelectPdsDataByIdAsync(randomPdsData.Id))
                    .ThrowsAsync(databaseUpdateException);

            // when
            ValueTask<PdsData> modifyPdsDataTask =
                this.pdsDataService.ModifyPdsDataAsync(randomPdsData);

            PdsDataDependencyException actualPdsDataDependencyException =
                await Assert.ThrowsAsync<PdsDataDependencyException>(
                    testCode: modifyPdsDataTask.AsTask);

            // then
            actualPdsDataDependencyException.Should()
                .BeEquivalentTo(expectedPdsDataDependencyException);

            this.storageBroker.Verify(broker =>
                broker.SelectPdsDataByIdAsync(randomPdsData.Id),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedPdsDataDependencyException))),
                        Times.Once);

            this.storageBroker.Verify(broker =>
                broker.UpdatePdsDataAsync(randomPdsData),
                    Times.Never);

            this.storageBroker.VerifyNoOtherCalls();
            this.dateTimeBroker.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyValidationExceptionOnModifyIfDbUpdateConcurrencyErrorOccursAndLogAsync()
        {
            // given
            PdsData randomPdsData = CreateRandomPdsData();
            var databaseUpdateConcurrencyException = new DbUpdateConcurrencyException();

            var lockedPdsDataException =
                new LockedPdsDataException(
                    message: "Locked pdsData record exception, please try again later",
                    innerException: databaseUpdateConcurrencyException);

            var expectedPdsDataDependencyValidationException =
                new PdsDataDependencyValidationException(
                    message: "PdsData dependency validation occurred, please try again.",
                    innerException: lockedPdsDataException);

            this.storageBroker.Setup(broker =>
                broker.SelectPdsDataByIdAsync(randomPdsData.Id))
                    .ThrowsAsync(databaseUpdateConcurrencyException);

            // when
            ValueTask<PdsData> modifyPdsDataTask =
                this.pdsDataService.ModifyPdsDataAsync(randomPdsData);

            PdsDataDependencyValidationException actualPdsDataDependencyValidationException =
                await Assert.ThrowsAsync<PdsDataDependencyValidationException>(
                    testCode: modifyPdsDataTask.AsTask);

            // then
            actualPdsDataDependencyValidationException.Should()
                .BeEquivalentTo(expectedPdsDataDependencyValidationException);

            this.storageBroker.Verify(broker =>
                broker.SelectPdsDataByIdAsync(randomPdsData.Id),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedPdsDataDependencyValidationException))),
                        Times.Once);

            this.storageBroker.Verify(broker =>
                broker.UpdatePdsDataAsync(randomPdsData),
                    Times.Never);

            this.storageBroker.VerifyNoOtherCalls();
            this.dateTimeBroker.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnModifyIfServiceErrorOccursAndLogItAsync()
        {
            // given
            PdsData randomPdsData = CreateRandomPdsData();
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
                broker.SelectPdsDataByIdAsync(randomPdsData.Id))
                    .ThrowsAsync(serviceException);

            // when
            ValueTask<PdsData> modifyPdsDataTask =
                this.pdsDataService.ModifyPdsDataAsync(randomPdsData);

            PdsDataServiceException actualPdsDataServiceException =
                await Assert.ThrowsAsync<PdsDataServiceException>(
                    testCode: modifyPdsDataTask.AsTask);

            // then
            actualPdsDataServiceException.Should()
                .BeEquivalentTo(expectedPdsDataServiceException);

            this.storageBroker.Verify(broker =>
                broker.SelectPdsDataByIdAsync(randomPdsData.Id),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedPdsDataServiceException))),
                        Times.Once);

            this.storageBroker.Verify(broker =>
                broker.UpdatePdsDataAsync(randomPdsData),
                    Times.Never);

            this.storageBroker.VerifyNoOtherCalls();
            this.dateTimeBroker.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}