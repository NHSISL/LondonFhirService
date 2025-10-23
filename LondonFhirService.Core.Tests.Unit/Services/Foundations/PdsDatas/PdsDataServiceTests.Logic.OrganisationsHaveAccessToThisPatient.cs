// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.PdsDatas;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.PdsDatas
{
    public partial class PdsDataServiceTests
    {
        [Fact]
        public async Task ShouldCheckIfOrganisationsHaveAccessToThisPatientAsync()
        {
            // given
            string randomPseudoNhsNumber = GetRandomString();
            string inputPseudoNhsNumber = randomPseudoNhsNumber;
            List<PdsData> randomPdsDatas = CreateRandomPdsDatas();
            randomPdsDatas.ForEach(pdsData => pdsData.NhsNumber = inputPseudoNhsNumber);
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
                    nhsNumber: inputPseudoNhsNumber, organisationCodes: inputOrganisationCodes);

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
        }

        [Fact]
        public async Task ShouldNotHaveAccessOnCheckIfOrganisationsHaveAccessToThisPatientWithInvalidPseudonumberAsync()
        {
            // given
            string randomPseudoNhsNumber = GetRandomString();
            string inputPseudoNhsNumber = randomPseudoNhsNumber;
            List<PdsData> randomPdsDatas = CreateRandomPdsDatas();
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
                    nhsNumber: inputPseudoNhsNumber, organisationCodes: inputOrganisationCodes);

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
        }

        [Fact]
        public async Task ShouldNotHaveAccessOnCheckIfOrganisationsHaveAccessToThisPatientWithInvalidOrganisationsAsync()
        {
            // given
            string randomPseudoNhsNumber = GetRandomString();
            string inputPseudoNhsNumber = randomPseudoNhsNumber;
            List<PdsData> randomPdsDatas = CreateRandomPdsDatas();
            randomPdsDatas.ForEach(pdsData => pdsData.NhsNumber = inputPseudoNhsNumber);
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
                    nhsNumber: inputPseudoNhsNumber, organisationCodes: inputOrganisationCodes);

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
        }

        [Fact]
        public async Task ShouldNotHaveAccessOnCheckIfOrganisationsHaveAccessToThisPatientWithInvalidInputsAsync()
        {
            // given
            string randomPseudoNhsNumber = GetRandomString();
            string inputPseudoNhsNumber = randomPseudoNhsNumber;
            List<PdsData> randomPdsDatas = CreateRandomPdsDatas();
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
                    nhsNumber: inputPseudoNhsNumber, organisationCodes: inputOrganisationCodes);

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
        }

        [Fact]
        public async Task ShouldNotHaveAccessToThisPatientIfRelationshipIsInactiveAsync()
        {
            // given
            string randomPseudoNhsNumber = GetRandomString();
            string inputPseudoNhsNumber = randomPseudoNhsNumber;
            List<PdsData> randomPdsDatas = CreateRandomPdsDatas();
            randomPdsDatas.ForEach(pdsData =>
            {
                pdsData.NhsNumber = inputPseudoNhsNumber;
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
                    nhsNumber: inputPseudoNhsNumber, organisationCodes: inputOrganisationCodes);

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
        }
    }
}