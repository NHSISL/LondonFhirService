// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.ConsumerAccesses;
using LondonFhirService.Core.Models.Foundations.OdsDatas;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ConsumerAccesses
{
    public partial class ConsumerAccessesTests
    {
        [Fact]
        public async Task ShouldRetrieveAllActiveOrganisationsUserHasAccessToAsync()
        {
            // given
            Guid randomConsumerId = Guid.NewGuid();
            Guid inputConsumerId = randomConsumerId;
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            List<ConsumerAccess> validConsumerAccesses = CreateConsumerAccesses(count: GetRandomNumber());
            validConsumerAccesses.ForEach(consumerAccess => consumerAccess.ConsumerId = inputConsumerId);
            List<ConsumerAccess> invalidConsumerAccesses = CreateConsumerAccesses(count: GetRandomNumber());
            List<ConsumerAccess> storageConsumerAccess = [.. validConsumerAccesses, .. invalidConsumerAccesses];
            List<OdsData> userOdsDatas = new List<OdsData>();

            foreach (var consumerAccess in storageConsumerAccess)
            {
                List<OdsData> odsDatas = CreateRandomOdsDatasByOrgCode(
                    orgCode: consumerAccess.OrgCode,
                    dateTimeOffset: randomDateTimeOffset,
                    childrenCount: GetRandomNumber());

                userOdsDatas.AddRange(odsDatas);
            }

            List<string> validOrgCodes = validConsumerAccesses
                .Select(consumerAccess => consumerAccess.OrgCode).ToList();

            OdsData validOdsDataItem = userOdsDatas
                .Where(odsData => validOrgCodes.Contains(odsData.OrganisationCode)).FirstOrDefault();

            List<OdsData> expectedOrganisations = userOdsDatas
                .Where(odsData =>
                    (odsData.OrganisationCode == validOdsDataItem.OrganisationCode
                        || odsData.OdsHierarchy.IsDescendantOf(validOdsDataItem.OdsHierarchy))
                    && (odsData.RelationshipWithParentStartDate == null
                        || odsData.RelationshipWithParentStartDate <= randomDateTimeOffset)
                    && (odsData.RelationshipWithParentEndDate == null ||
                        odsData.RelationshipWithParentEndDate > randomDateTimeOffset)).ToList();

            List<string> expectedOrganisationCodes = expectedOrganisations
                .Select(odsData => odsData.OrganisationCode).Distinct().ToList();

            this.storageBroker.Setup(broker =>
                broker.SelectAllConsumerAccessesAsync())
                    .ReturnsAsync(storageConsumerAccess.AsQueryable());

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.storageBroker.Setup(broker =>
                broker.SelectAllOdsDatasAsync())
                    .ReturnsAsync(userOdsDatas.AsQueryable());

            // when
            List<string> actualOrganisationCodes = await this.consumerAccessService
                .RetrieveAllActiveOrganisationsUserHasAccessToAsync(inputConsumerId);

            // then
            actualOrganisationCodes.Should().BeEquivalentTo(expectedOrganisationCodes);

            this.storageBroker.Verify(broker =>
                broker.SelectAllConsumerAccessesAsync(),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.storageBroker.Verify(broker =>
                broker.SelectAllOdsDatasAsync(),
                    Times.Exactly(validConsumerAccesses.Count() * 2));

            this.storageBroker.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
        }
    }
}
