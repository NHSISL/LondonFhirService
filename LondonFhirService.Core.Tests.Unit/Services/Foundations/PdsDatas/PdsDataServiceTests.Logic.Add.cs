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
        public async Task ShouldAddPdsDataAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset =
                GetRandomDateTimeOffset();

            PdsData randomPdsData = CreateRandomPdsData(randomDateTimeOffset);
            PdsData inputPdsData = randomPdsData;
            PdsData storagePdsData = inputPdsData;
            PdsData expectedPdsData = storagePdsData.DeepClone();

            this.storageBroker.Setup(broker =>
                broker.InsertPdsDataAsync(inputPdsData))
                    .ReturnsAsync(storagePdsData);

            // when
            PdsData actualPdsData = await this.pdsDataService
                .AddPdsDataAsync(inputPdsData);

            // then
            actualPdsData.Should().BeEquivalentTo(expectedPdsData);

            this.storageBroker.Verify(broker =>
                broker.InsertPdsDataAsync(inputPdsData),
                    Times.Once);

            this.storageBroker.VerifyNoOtherCalls();
            this.dateTimeBroker.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}