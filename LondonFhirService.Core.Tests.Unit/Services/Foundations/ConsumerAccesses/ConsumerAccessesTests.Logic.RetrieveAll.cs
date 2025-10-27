// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.ConsumerAccesses;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ConsumerAccesses
{
    public partial class ConsumerAccessesTests
    {
        [Fact]
        public async Task ShouldRetrieveAllConsumerAccessesAsync()
        {
            // given
            List<ConsumerAccess> randomConsumerAccesses = CreateRandomConsumerAccesses();
            IQueryable<ConsumerAccess> storageConsumerAccesses = randomConsumerAccesses.AsQueryable();
            IQueryable<ConsumerAccess> expectedConsumerAccesses = storageConsumerAccesses;

            this.storageBroker.Setup(broker =>
                broker.SelectAllConsumerAccessesAsync())
                    .ReturnsAsync(storageConsumerAccesses);

            // when
            IQueryable<ConsumerAccess> actualConsumerAccesses = await this.consumerAccessService.RetrieveAllConsumerAccessesAsync();

            // then
            actualConsumerAccesses.Should().BeEquivalentTo(expectedConsumerAccesses);

            this.storageBroker.Verify(broker =>
                broker.SelectAllConsumerAccessesAsync(),
                    Times.Once);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBroker.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
        }
    }
}
