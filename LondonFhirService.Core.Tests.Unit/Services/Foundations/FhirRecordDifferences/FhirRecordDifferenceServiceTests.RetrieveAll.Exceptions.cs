// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
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
        public async Task ShouldThrowCriticalDependencyExceptionOnRetrieveAllWhenSqlExceptionOccursAndLogIt()
        {
            // given
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
                broker.SelectAllFhirRecordDifferencesAsync())
                    .ThrowsAsync(sqlException);

            // when
            ValueTask<IQueryable<FhirRecordDifference>> retrieveAllFhirRecordDifferencesTask =
                this.fhirRecordDifferenceService.RetrieveAllFhirRecordDifferencesAsync();

            FhirRecordDifferenceDependencyException actualFhirRecordDifferenceDependencyException =
                await Assert.ThrowsAsync<FhirRecordDifferenceDependencyException>(
                    retrieveAllFhirRecordDifferencesTask.AsTask);

            // then
            actualFhirRecordDifferenceDependencyException.Should()
                .BeEquivalentTo(expectedFhirRecordDifferenceDependencyException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAllFhirRecordDifferencesAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedFhirRecordDifferenceDependencyException))),
                        Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnRetrieveAllIfServiceErrorOccursAndLogItAsync()
        {
            // given
            string exceptionMessage = GetRandomString();
            var serviceException = new Exception(exceptionMessage);

            var failedFhirRecordDifferenceServiceException =
                new FailedFhirRecordDifferenceServiceException(
                    message: "Failed fhirRecordDifference service occurred, please contact support",
                    innerException: serviceException);

            var expectedFhirRecordDifferenceServiceException =
                new FhirRecordDifferenceServiceException(
                    message: "FhirRecordDifference service error occurred, contact support.",
                    innerException: failedFhirRecordDifferenceServiceException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectAllFhirRecordDifferencesAsync())
                    .ThrowsAsync(serviceException);

            // when
            ValueTask<IQueryable<FhirRecordDifference>> retrieveAllFhirRecordDifferencesTask =
                this.fhirRecordDifferenceService.RetrieveAllFhirRecordDifferencesAsync();

            FhirRecordDifferenceServiceException actualFhirRecordDifferenceServiceException =
                await Assert.ThrowsAsync<FhirRecordDifferenceServiceException>(retrieveAllFhirRecordDifferencesTask.AsTask);

            // then
            actualFhirRecordDifferenceServiceException.Should()
                .BeEquivalentTo(expectedFhirRecordDifferenceServiceException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAllFhirRecordDifferencesAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedFhirRecordDifferenceServiceException))),
                        Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }
    }
}