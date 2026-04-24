// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
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
        public async Task ShouldThrowCriticalDependencyExceptionOnRetrieveByIdIfSqlErrorOccursAndLogItAsync()
        {
            // given
            Guid someId = Guid.NewGuid();
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
                broker.SelectFhirRecordByIdAsync(It.IsAny<Guid>()))
                    .ThrowsAsync(sqlException);

            // when
            ValueTask<FhirRecord> retrieveFhirRecordByIdTask =
                this.fhirRecordService.RetrieveFhirRecordByIdAsync(someId);

            FhirRecordDependencyException actualFhirRecordDependencyException =
                await Assert.ThrowsAsync<FhirRecordDependencyException>(
                    retrieveFhirRecordByIdTask.AsTask);

            // then
            actualFhirRecordDependencyException.Should()
                .BeEquivalentTo(expectedFhirRecordDependencyException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectFhirRecordByIdAsync(It.IsAny<Guid>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedFhirRecordDependencyException))),
                        Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnRetrieveByIdIfServiceErrorOccursAndLogItAsync()
        {
            // given
            Guid someId = Guid.NewGuid();
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
                broker.SelectFhirRecordByIdAsync(It.IsAny<Guid>()))
                    .ThrowsAsync(serviceException);

            // when
            ValueTask<FhirRecord> retrieveFhirRecordByIdTask =
                this.fhirRecordService.RetrieveFhirRecordByIdAsync(someId);

            FhirRecordServiceException actualFhirRecordServiceException =
                await Assert.ThrowsAsync<FhirRecordServiceException>(
                    retrieveFhirRecordByIdTask.AsTask);

            // then
            actualFhirRecordServiceException.Should()
                .BeEquivalentTo(expectedFhirRecordServiceException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectFhirRecordByIdAsync(It.IsAny<Guid>()),
                    Times.Once);

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