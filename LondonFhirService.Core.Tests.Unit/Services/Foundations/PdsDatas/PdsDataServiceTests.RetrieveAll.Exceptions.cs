// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.PdsDatas;
using LondonFhirService.Core.Models.Foundations.PdsDatas.Exceptions;
using Microsoft.Data.SqlClient;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.PdsDatas
{
    public partial class PdsDataServiceTests
    {
        [Fact]
        public async Task ShouldThrowCriticalDependencyExceptionOnRetrieveAllWhenSqlExceptionOccursAndLogItAsync()
        {
            // given
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
                broker.SelectAllPdsDatasAsync())
                    .ThrowsAsync(sqlException);

            // when
            ValueTask<IQueryable<PdsData>> retrieveAllPdsDatasTask =
                this.pdsDataService.RetrieveAllPdsDatasAsync();

            PdsDataServiceDependencyException actualPdsDataDependencyException =
                await Assert.ThrowsAsync<PdsDataServiceDependencyException>(
                    testCode: retrieveAllPdsDatasTask.AsTask);

            // then
            actualPdsDataDependencyException.Should()
                .BeEquivalentTo(expectedPdsDataDependencyException);

            this.storageBroker.Verify(broker =>
                broker.SelectAllPdsDatasAsync(),
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
        public async Task ShouldThrowServiceExceptionOnRetrieveAllIfServiceErrorOccursAndLogItAsync()
        {
            // given
            string exceptionMessage = GetRandomString();
            var serviceException = new Exception(exceptionMessage);

            var failedPdsDataServiceException =
                new FailedPdsDataServiceException(
                    message: "Failed pdsData service occurred, please contact support",
                    innerException: serviceException);

            var expectedPdsDataServiceException =
                new PdsDataServiceException(
                    message: "PdsData service error occurred, contact support.",
                    innerException: failedPdsDataServiceException);

            this.storageBroker.Setup(broker =>
                broker.SelectAllPdsDatasAsync())
                    .ThrowsAsync(serviceException);

            // when
            ValueTask<IQueryable<PdsData>> retrieveAllPdsDatasTask =
                this.pdsDataService.RetrieveAllPdsDatasAsync();

            PdsDataServiceException actualPdsDataServiceException =
                await Assert.ThrowsAsync<PdsDataServiceException>(
                    testCode: retrieveAllPdsDatasTask.AsTask);

            // then
            actualPdsDataServiceException.Should()
                .BeEquivalentTo(expectedPdsDataServiceException);

            this.storageBroker.Verify(broker =>
                broker.SelectAllPdsDatasAsync(),
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