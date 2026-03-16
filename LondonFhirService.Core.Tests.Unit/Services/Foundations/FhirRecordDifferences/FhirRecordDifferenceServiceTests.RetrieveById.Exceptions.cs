// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.FhirRecordDifferences;
using LondonFhirService.Core.Models.Foundations.FhirRecordDifferences.Exceptions;
using Microsoft.Data.SqlClient;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.FhirRecordDifferences
{
    public partial class FhirRecordDifferenceServiceTests
    {
        [Fact]
        public async Task ShouldThrowCriticalDependencyExceptionOnRetrieveByIdIfSqlErrorOccursAndLogItAsync()
        {
            // given
            Guid someId = Guid.NewGuid();
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
            ValueTask<FhirRecordDifference> retrieveFhirRecordDifferenceByIdTask =
                this.fhirRecordDifferenceService.RetrieveFhirRecordDifferenceByIdAsync(someId);

            FhirRecordDifferenceDependencyException actualFhirRecordDifferenceDependencyException =
                await Assert.ThrowsAsync<FhirRecordDifferenceDependencyException>(
                    retrieveFhirRecordDifferenceByIdTask.AsTask);

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
        public async Task ShouldThrowServiceExceptionOnRetrieveByIdIfServiceErrorOccursAndLogItAsync()
        {
            // given
            Guid someId = Guid.NewGuid();
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
            ValueTask<FhirRecordDifference> retrieveFhirRecordDifferenceByIdTask =
                this.fhirRecordDifferenceService.RetrieveFhirRecordDifferenceByIdAsync(someId);

            FhirRecordDifferenceServiceException actualFhirRecordDifferenceServiceException =
                await Assert.ThrowsAsync<FhirRecordDifferenceServiceException>(
                    retrieveFhirRecordDifferenceByIdTask.AsTask);

            // then
            actualFhirRecordDifferenceServiceException.Should()
                .BeEquivalentTo(expectedFhirRecordDifferenceServiceException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectFhirRecordDifferenceByIdAsync(It.IsAny<Guid>()),
                    Times.Once);

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