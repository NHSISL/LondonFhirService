// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.FhirRecords;
using LondonFhirService.Core.Models.Foundations.FhirRecords.Exceptions;
using Microsoft.Data.SqlClient;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.FhirRecords
{
    public partial class FhirRecordServiceTests
    {
        [Fact]
        public async Task ShouldThrowCriticalDependencyExceptionOnRetrieveAllWhenSqlExceptionOccursAndLogIt()
        {
            // given
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
                broker.SelectAllFhirRecordsAsync())
                    .ThrowsAsync(sqlException);

            // when
            ValueTask<IQueryable<FhirRecord>> retrieveAllFhirRecordsTask =
                this.fhirRecordService.RetrieveAllFhirRecordsAsync();

            FhirRecordDependencyException actualFhirRecordDependencyException =
                await Assert.ThrowsAsync<FhirRecordDependencyException>(
                    retrieveAllFhirRecordsTask.AsTask);

            // then
            actualFhirRecordDependencyException.Should()
                .BeEquivalentTo(expectedFhirRecordDependencyException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAllFhirRecordsAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedFhirRecordDependencyException))),
                        Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnRetrieveAllIfServiceErrorOccursAndLogItAsync()
        {
            // given
            string exceptionMessage = GetRandomString();
            var serviceException = new Exception(exceptionMessage);

            var failedFhirRecordServiceException =
                new FailedFhirRecordServiceException(
                    message: "Failed fhirRecord service occurred, please contact support",
                    innerException: serviceException);

            var expectedFhirRecordServiceException =
                new FhirRecordServiceException(
                    message: "FhirRecord service error occurred, contact support.",
                    innerException: failedFhirRecordServiceException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectAllFhirRecordsAsync())
                    .ThrowsAsync(serviceException);

            // when
            ValueTask<IQueryable<FhirRecord>> retrieveAllFhirRecordsTask =
                this.fhirRecordService.RetrieveAllFhirRecordsAsync();

            FhirRecordServiceException actualFhirRecordServiceException =
                await Assert.ThrowsAsync<FhirRecordServiceException>(retrieveAllFhirRecordsTask.AsTask);

            // then
            actualFhirRecordServiceException.Should()
                .BeEquivalentTo(expectedFhirRecordServiceException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAllFhirRecordsAsync(),
                    Times.Once);

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