// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
using LondonFhirService.Core.Models.Foundations.PdsDatas;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.PdsDatas
{
    public partial class PdsDataServiceTests
    {
        [Fact]
        public async Task ShouldRetrievePdsDataByIdAsync()
        {
            // given
            PdsData randomPdsData = CreateRandomPdsData();
            PdsData inputPdsData = randomPdsData;
            PdsData storagePdsData = randomPdsData;
            PdsData expectedPdsData = storagePdsData.DeepClone();

            this.storageBroker.Setup(broker =>
                broker.SelectPdsDataByIdAsync(inputPdsData.Id))
                    .ReturnsAsync(storagePdsData);

            // when
            PdsData actualPdsData =
                await this.pdsDataService.RetrievePdsDataByIdAsync(inputPdsData.Id);

            // then
            actualPdsData.Should().BeEquivalentTo(expectedPdsData);

            this.storageBroker.Verify(broker =>
                broker.SelectPdsDataByIdAsync(inputPdsData.Id),
                    Times.Once);

            this.storageBroker.VerifyNoOtherCalls();
            this.dateTimeBroker.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}