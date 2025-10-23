// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.OdsDatas;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.OdsDatas
{
    public partial class OdsDataServiceTests
    {
        [Fact]
        public async Task ShouldRetrieveAllAncestorsByChildIdAsync()
        {
            // given
            OdsData randomOdsData = CreateRandomOdsData();
            OdsData inputOdsData = randomOdsData;
            OdsData storageOdsData = randomOdsData;
            List<OdsData> childrenOdsDatas = CreateRandomOdsDataChildren(storageOdsData.OdsHierarchy, 1);
            List<OdsData> grandChildrenOdsDatas = CreateRandomOdsDataChildren(childrenOdsDatas[0].OdsHierarchy, 1);
            List<OdsData> storageOdsDatas = new List<OdsData> { storageOdsData };
            storageOdsDatas.AddRange(childrenOdsDatas);
            storageOdsDatas.AddRange(grandChildrenOdsDatas);
            List<OdsData> expectedOdsDatas = storageOdsDatas;

            this.storageBroker.Setup(broker =>
                broker.SelectOdsDataByIdAsync(grandChildrenOdsDatas[0].Id))
                    .ReturnsAsync(grandChildrenOdsDatas[0]);

            this.storageBroker.Setup(broker =>
                broker.SelectAllOdsDatasAsync())
                    .ReturnsAsync(storageOdsDatas.AsQueryable());

            // when
            List<OdsData> actualOdsDatas =
                await this.odsDataService.RetrieveAllAncestorsByChildId(grandChildrenOdsDatas[0].Id);

            // then
            actualOdsDatas.Should().BeEquivalentTo(expectedOdsDatas);

            this.storageBroker.Verify(broker =>
                broker.SelectOdsDataByIdAsync(grandChildrenOdsDatas[0].Id),
                    Times.Once);

            this.storageBroker.Verify(broker =>
                broker.SelectAllOdsDatasAsync(),
                    Times.Once);

            this.storageBroker.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
