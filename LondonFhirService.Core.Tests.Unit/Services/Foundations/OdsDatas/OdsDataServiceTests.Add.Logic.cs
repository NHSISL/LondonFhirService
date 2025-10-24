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
        public async Task ShouldAddOdsDataAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset =
                GetRandomDateTimeOffset();

            OdsData randomOdsData = CreateRandomOdsData(randomDateTimeOffset);
            OdsData inputOdsData = randomOdsData;
            OdsData storageOdsData = inputOdsData;
            OdsData expectedOdsData = storageOdsData.DeepClone();

            this.storageBroker.Setup(broker =>
                broker.InsertOdsDataAsync(inputOdsData))
                    .ReturnsAsync(storageOdsData);

            // when
            OdsData actualOdsData = await this.odsDataService
                .AddOdsDataAsync(inputOdsData);

            // then
            actualOdsData.Should().BeEquivalentTo(expectedOdsData);

            this.storageBroker.Verify(broker =>
                broker.InsertOdsDataAsync(inputOdsData),
                    Times.Once);

            this.storageBroker.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}