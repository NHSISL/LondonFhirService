// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
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
        public async Task ShouldModifyOdsDataAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            OdsData randomOdsData = CreateRandomModifyOdsData(randomDateTimeOffset);
            OdsData inputOdsData = randomOdsData;
            OdsData storageOdsData = inputOdsData.DeepClone();
            OdsData updatedOdsData = inputOdsData;
            OdsData expectedOdsData = updatedOdsData.DeepClone();
            Guid odsDataId = inputOdsData.Id;

            this.storageBroker.Setup(broker =>
                broker.SelectOdsDataByIdAsync(odsDataId))
                    .ReturnsAsync(storageOdsData);

            this.storageBroker.Setup(broker =>
                broker.UpdateOdsDataAsync(inputOdsData))
                    .ReturnsAsync(updatedOdsData);

            // when
            OdsData actualOdsData =
                await this.odsDataService.ModifyOdsDataAsync(inputOdsData);

            // then
            actualOdsData.Should().BeEquivalentTo(expectedOdsData);

            this.storageBroker.Verify(broker =>
                broker.SelectOdsDataByIdAsync(inputOdsData.Id),
                    Times.Once);

            this.storageBroker.Verify(broker =>
                broker.UpdateOdsDataAsync(inputOdsData),
                    Times.Once);

            this.storageBroker.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}