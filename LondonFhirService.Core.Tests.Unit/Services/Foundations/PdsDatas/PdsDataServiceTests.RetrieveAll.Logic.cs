// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

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
        public async Task ShouldReturnPdsDatas()
        {
            // given
            List<PdsData> randomPdsDatas = CreateRandomPdsDatas();
            IQueryable<PdsData> storagePdsDatas = randomPdsDatas.AsQueryable();
            IQueryable<PdsData> expectedPdsDatas = storagePdsDatas;

            this.storageBroker.Setup(broker =>
                broker.SelectAllPdsDatasAsync())
                    .ReturnsAsync(storagePdsDatas);

            // when
            IQueryable<PdsData> actualPdsDatas =
                await this.pdsDataService.RetrieveAllPdsDatasAsync();

            // then
            actualPdsDatas.Should().BeEquivalentTo(expectedPdsDatas);

            this.storageBroker.Verify(broker =>
                broker.SelectAllPdsDatasAsync(),
                    Times.Once);

            this.storageBroker.VerifyNoOtherCalls();
            this.dateTimeBroker.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}