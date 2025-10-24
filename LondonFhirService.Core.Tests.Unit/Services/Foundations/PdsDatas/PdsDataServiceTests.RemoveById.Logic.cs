// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
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
        public async Task ShouldRemovePdsDataByIdAsync()
        {
            // given
            Guid randomId = Guid.NewGuid();
            Guid inputPdsDataId = randomId;
            PdsData randomPdsData = CreateRandomPdsData();
            PdsData storagePdsData = randomPdsData;
            PdsData expectedInputPdsData = storagePdsData;
            PdsData deletedPdsData = expectedInputPdsData;
            PdsData expectedPdsData = deletedPdsData.DeepClone();

            this.storageBroker.Setup(broker =>
                broker.SelectPdsDataByIdAsync(inputPdsDataId))
                    .ReturnsAsync(storagePdsData);

            this.storageBroker.Setup(broker =>
                broker.DeletePdsDataAsync(expectedInputPdsData))
                    .ReturnsAsync(deletedPdsData);

            // when
            PdsData actualPdsData = await this.pdsDataService
                .RemovePdsDataByIdAsync(inputPdsDataId);

            // then
            actualPdsData.Should().BeEquivalentTo(expectedPdsData);

            this.storageBroker.Verify(broker =>
                broker.SelectPdsDataByIdAsync(inputPdsDataId),
                    Times.Once);

            this.storageBroker.Verify(broker =>
                broker.DeletePdsDataAsync(expectedInputPdsData),
                    Times.Once);

            this.storageBroker.VerifyNoOtherCalls();
            this.dateTimeBroker.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}