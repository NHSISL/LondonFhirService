// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
using LondonFhirService.Core.Models.Foundations.OdsDatas;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.OdsDatas
{
    public partial class OdsDataServiceTests
    {
        [Fact]
        public async Task ShouldRetrieveAllDecendentsByParentIdAsync()
        {
            // given
            OdsData randomOdsData = CreateRandomOdsData();
            OdsData inputOdsData = randomOdsData;
            OdsData storageOdsData = randomOdsData;
            List<OdsData> randomOdsDatas = CreateRandomOdsDataChildren(storageOdsData.OdsHierarchy);
            List<OdsData> childen = randomOdsDatas;
            List<OdsData> expectedOdsDatas = childen.DeepClone();

            this.storageBroker.Setup(broker =>
                broker.SelectOdsDataByIdAsync(inputOdsData.Id))
                    .ReturnsAsync(storageOdsData);

            this.storageBroker.Setup(broker =>
                broker.SelectAllOdsDatasAsync())
                    .ReturnsAsync(childen.AsQueryable());

            // when
            List<OdsData> actualOdsDatas =
                await this.odsDataService.RetrieveAllDescendantsByParentId(inputOdsData.Id);

            // then
            actualOdsDatas.Should().BeEquivalentTo(expectedOdsDatas);

            this.storageBroker.Verify(broker =>
                broker.SelectOdsDataByIdAsync(inputOdsData.Id),
                    Times.Once);

            this.storageBroker.Verify(broker =>
                broker.SelectAllOdsDatasAsync(),
                    Times.Once);

            this.storageBroker.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}