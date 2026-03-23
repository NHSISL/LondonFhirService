// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.PdsDatas.Exceptions;
using Microsoft.Data.SqlClient;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.PdsDatas
{
    public partial class PdsDataServiceTests
    {
        [Fact]
        public async Task ShouldThrowCriticalDependencyExceptionOnOrganisationsHaveAccessToThisPatientAndLogItAsync()
        {
            // given
            Guid someCorrelationId = Guid.NewGuid();
            string somePatientIdentifier = GetRandomString();
            string someNhsNumber = GetRandomString();
            List<string> someOrganisations = GetRandomStringsWithLengthOf(10);
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
            ValueTask<bool> retrieveAllPdsDatasTask =
                this.pdsDataService.OrganisationsHaveAccessToThisPatient(
                    patientIdentifier: somePatientIdentifier,
                    nhsNumber: someNhsNumber,
                    organisationCodes: someOrganisations,
                    correlationId: someCorrelationId);

            PdsDataServiceDependencyException actualPdsDataDependencyException =
                await Assert.ThrowsAsync<PdsDataServiceDependencyException>(
                    testCode: retrieveAllPdsDatasTask.AsTask);

            // then
            actualPdsDataDependencyException.Should()
                .BeEquivalentTo(expectedPdsDataDependencyException);

            this.storageBroker.Verify(broker =>
                broker.SelectAllPdsDatasAsync(),
                    Times.Once);

            this.dateTimeBroker.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                Times.Never);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedPdsDataDependencyException))),
                        Times.Once);

            this.storageBroker.VerifyNoOtherCalls();
            this.dateTimeBroker.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnOrganisationsHaveAccessToThisPatientAndLogItAsync()
        {
            // given
            string somePatientIdentifier = GetRandomString();
            string someNhsNumber = GetRandomString();
            Guid someCorrelationId = Guid.NewGuid();
            List<string> someOrganisations = GetRandomStringsWithLengthOf(10);
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
            ValueTask<bool> retrieveAllPdsDatasTask =
                this.pdsDataService.OrganisationsHaveAccessToThisPatient(
                    patientIdentifier: somePatientIdentifier,
                    nhsNumber: someNhsNumber,
                    organisationCodes: someOrganisations,
                    correlationId: someCorrelationId);

            PdsDataServiceException actualPdsDataServiceException =
                await Assert.ThrowsAsync<PdsDataServiceException>(
                    testCode: retrieveAllPdsDatasTask.AsTask);

            // then
            actualPdsDataServiceException.Should()
                .BeEquivalentTo(expectedPdsDataServiceException);

            this.storageBroker.Verify(broker =>
                broker.SelectAllPdsDatasAsync(),
                    Times.Once);

            this.dateTimeBroker.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                Times.Never);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedPdsDataServiceException))),
                        Times.Once);

            this.storageBroker.VerifyNoOtherCalls();
            this.dateTimeBroker.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
        }
    }
}