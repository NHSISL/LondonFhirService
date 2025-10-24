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
        public async Task ShouldModifyPdsDataAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            PdsData randomPdsData = CreateRandomModifyPdsData(randomDateTimeOffset);
            PdsData inputPdsData = randomPdsData;
            PdsData storagePdsData = inputPdsData.DeepClone();
            PdsData updatedPdsData = inputPdsData;
            PdsData expectedPdsData = updatedPdsData.DeepClone();
            Guid pdsDataId = inputPdsData.Id;

            this.storageBroker.Setup(broker =>
                broker.SelectPdsDataByIdAsync(pdsDataId))
                    .ReturnsAsync(storagePdsData);

            this.storageBroker.Setup(broker =>
                broker.UpdatePdsDataAsync(inputPdsData))
                    .ReturnsAsync(updatedPdsData);

            // when
            PdsData actualPdsData =
                await this.pdsDataService.ModifyPdsDataAsync(inputPdsData);

            // then
            actualPdsData.Should().BeEquivalentTo(expectedPdsData);

            this.storageBroker.Verify(broker =>
                broker.SelectPdsDataByIdAsync(inputPdsData.Id),
                    Times.Once);

            this.storageBroker.Verify(broker =>
                broker.UpdatePdsDataAsync(inputPdsData),
                    Times.Once);

            this.storageBroker.VerifyNoOtherCalls();
            this.dateTimeBroker.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}