// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.FhirReconciliations.Exceptions;
using LondonFhirService.Core.Models.Foundations.PdsDatas;
using LondonFhirService.Core.Models.Foundations.PdsDatas.Exceptions;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.PdsDatas
{
    public partial class PdsDataServiceTests
    {
        [Fact]
        public async Task ShouldCheckIfOrganisationsHaveAccessToThisPatientAsync()
        {
            // given
            Guid inputCorrelationId = Guid.NewGuid();
            List<PdsData> randomPdsDatas = CreateRandomPdsDatas();
            string inputNhsNumber = randomPdsDatas.First().NhsNumber;
            string inputPatientIdentifier = randomPdsDatas.First().NhsNumber;
            randomPdsDatas.ForEach(pdsData => pdsData.NhsNumber = inputNhsNumber);
            List<PdsData> storagePdsDatas = randomPdsDatas;
            List<string> inputOrganisationCodes = randomPdsDatas.Select(pdsData => pdsData.OrgCode).ToList();
            bool expectedResult = true;

            this.storageBroker.Setup(broker =>
                broker.SelectAllPdsDatasAsync())
                    .ReturnsAsync(storagePdsDatas.AsQueryable());

            this.dateTimeBroker.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(DateTimeOffset.UtcNow);

            // when
            bool actualResult =
                await this.pdsDataService.OrganisationsHaveAccessToThisPatient(
                    patientIdentifier: inputPatientIdentifier,
                    nhsNumber: inputNhsNumber,
                    organisationCodes: inputOrganisationCodes,
                    correlationId: inputCorrelationId);

            // then
            actualResult.Should().Be(expectedResult);

            this.storageBroker.Verify(broker =>
                broker.SelectAllPdsDatasAsync(),
                    Times.Once);

            this.dateTimeBroker.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.storageBroker.VerifyNoOtherCalls();
            this.dateTimeBroker.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldNotHaveAccessOnCheckIfOrganisationsHaveAccessToThisPatientWithInvalidOrganisationsAsync()
        {
            // given
            Guid inputCorrelationId = Guid.NewGuid();
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;
            string inputPatientIdentifier = GetRandomString();
            List<PdsData> randomPdsDatas = CreateRandomPdsDatas();
            randomPdsDatas.ForEach(pdsData => pdsData.NhsNumber = inputNhsNumber);
            List<PdsData> storagePdsDatas = randomPdsDatas;
            List<string> inputOrganisationCodes = GetRandomStringsWithLengthOf(10);
            bool expectedResult = false;

            this.storageBroker.Setup(broker =>
                broker.SelectAllPdsDatasAsync())
                    .ReturnsAsync(storagePdsDatas.AsQueryable());

            this.dateTimeBroker.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(DateTimeOffset.UtcNow);

            // when
            bool actualResult =
                await this.pdsDataService.OrganisationsHaveAccessToThisPatient(
                    patientIdentifier: inputPatientIdentifier,
                    nhsNumber: inputNhsNumber,
                    organisationCodes: inputOrganisationCodes,
                    correlationId: inputCorrelationId);

            // then
            actualResult.Should().Be(expectedResult);

            this.storageBroker.Verify(broker =>
                broker.SelectAllPdsDatasAsync(),
                    Times.Once);

            this.dateTimeBroker.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.storageBroker.VerifyNoOtherCalls();
            this.dateTimeBroker.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldNotHaveAccessOnCheckIfOrganisationsHaveAccessToThisPatientWithMissingConfigAsync()
        {
            // given
            Guid inputCorrelationId = Guid.NewGuid();
            string inputNhsNumber = GetRandomString();
            string inputPatientIdentifier = GetRandomString();
            List<string> inputOrganisationCodes = GetRandomStringsWithLengthOf(10);

            this.storageBroker.Setup(broker =>
                broker.SelectAllPdsDatasAsync())
                    .ReturnsAsync(new List<PdsData>().AsQueryable());

            this.dateTimeBroker.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(DateTimeOffset.UtcNow);

            var resourceNotFoundException =
                new ResourceNotFoundException(
                    message:
                        $"NotFound:Patient resource with id = '{inputNhsNumber}' not found.  (PDS)  " +
                        $"CorrelationId: {inputCorrelationId.ToString()}");

            PdsDataServiceValidationException expectedPdsDataServiceValidationException =
                new PdsDataServiceValidationException(
                    message: "PdsData validation error occurred, please fix errors and try again.",
                    innerException: resourceNotFoundException);

            // when
            ValueTask<bool> retrieveAllPdsDatasTask =
                this.pdsDataService.OrganisationsHaveAccessToThisPatient(
                    patientIdentifier: inputPatientIdentifier,
                    nhsNumber: inputNhsNumber,
                    organisationCodes: inputOrganisationCodes,
                    correlationId: inputCorrelationId);

            PdsDataServiceValidationException actualPdsDataServiceValidationException =
                await Assert.ThrowsAsync<PdsDataServiceValidationException>(
                    testCode: retrieveAllPdsDatasTask.AsTask);

            // then
            actualPdsDataServiceValidationException.Should().BeEquivalentTo(expectedPdsDataServiceValidationException);

            this.storageBroker.Verify(broker =>
                broker.SelectAllPdsDatasAsync(),
                    Times.Once);

            this.dateTimeBroker.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedPdsDataServiceValidationException))),
                        Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    "Access",
                    "PDS Configuration",

                    $"Patient resource with NHS Number: '{inputNhsNumber}', does not have a corresponding hash entry in the PDS table.  " +
                        $"CorrelationId: {inputCorrelationId.ToString()}",

                    null,
                    inputCorrelationId.ToString()),
                        Times.Once);

            this.storageBroker.VerifyNoOtherCalls();
            this.dateTimeBroker.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldNotHaveAccessOnCheckIfOrganisationsHaveAccessToThisPatientWithInvalidInputsAsync()
        {
            // given
            Guid inputCorrelationId = Guid.NewGuid();
            List<PdsData> randomPdsDatas = CreateRandomPdsDatas();
            string inputNhsNumber = randomPdsDatas.First().NhsNumber;
            string inputPatientIdentifier = randomPdsDatas.First().NhsNumber;
            List<PdsData> storagePdsDatas = randomPdsDatas;
            List<string> inputOrganisationCodes = GetRandomStringsWithLengthOf(10);
            bool expectedResult = false;

            this.storageBroker.Setup(broker =>
                broker.SelectAllPdsDatasAsync())
                    .ReturnsAsync(storagePdsDatas.AsQueryable());

            this.dateTimeBroker.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(DateTimeOffset.UtcNow);

            // when
            bool actualResult =
                await this.pdsDataService.OrganisationsHaveAccessToThisPatient(
                    patientIdentifier: inputPatientIdentifier,
                    nhsNumber: inputNhsNumber,
                    organisationCodes: inputOrganisationCodes,
                    correlationId: inputCorrelationId);

            // then
            actualResult.Should().Be(expectedResult);

            this.storageBroker.Verify(broker =>
                broker.SelectAllPdsDatasAsync(),
                    Times.Once);

            this.dateTimeBroker.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.storageBroker.VerifyNoOtherCalls();
            this.dateTimeBroker.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldNotHaveAccessToThisPatientIfRelationshipIsInactiveAsync()
        {
            // given
            string inputNhsNumber = GetRandomString();
            string inputPatientIdentifier = GetRandomString();
            Guid inputCorrelationId = Guid.NewGuid();
            List<PdsData> randomPdsDatas = CreateRandomPdsDatas();

            randomPdsDatas.ForEach(pdsData =>
            {
                pdsData.NhsNumber = inputNhsNumber;
                pdsData.RelationshipWithOrganisationEffectiveFromDate = GetRandomFutureDateTimeOffset();
            });

            List<PdsData> storagePdsDatas = randomPdsDatas;
            List<string> inputOrganisationCodes = randomPdsDatas.Select(pdsData => pdsData.OrgCode).ToList();
            bool expectedResult = false;

            this.storageBroker.Setup(broker =>
                broker.SelectAllPdsDatasAsync())
                    .ReturnsAsync(storagePdsDatas.AsQueryable());

            this.dateTimeBroker.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(DateTimeOffset.UtcNow);

            // when
            bool actualResult =
                await this.pdsDataService.OrganisationsHaveAccessToThisPatient(
                    patientIdentifier: inputPatientIdentifier,
                    nhsNumber: inputNhsNumber,
                    organisationCodes: inputOrganisationCodes,
                    correlationId: inputCorrelationId);

            // then
            actualResult.Should().Be(expectedResult);

            this.storageBroker.Verify(broker =>
                broker.SelectAllPdsDatasAsync(),
                    Times.Once);

            this.dateTimeBroker.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.storageBroker.VerifyNoOtherCalls();
            this.dateTimeBroker.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
        }
    }
}