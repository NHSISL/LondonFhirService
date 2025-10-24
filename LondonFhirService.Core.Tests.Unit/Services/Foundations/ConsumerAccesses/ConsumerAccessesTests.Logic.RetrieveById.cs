// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
using LondonFhirService.Core.Models.Foundations.ConsumerAccesses;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ConsumerAccesses
{
    public partial class ConsumerAccessesTests
    {
        [Fact]
        public async Task ShouldRetrieveByIdConsumerAccessAsync()
        {
            // given
            ConsumerAccess randomConsumerAccess = CreateRandomConsumerAccess();
            ConsumerAccess inputConsumerAccess = randomConsumerAccess;
            ConsumerAccess storageConsumerAccess = inputConsumerAccess.DeepClone();

            this.storageBroker.Setup(broker =>
                broker.SelectConsumerAccessByIdAsync(inputConsumerAccess.Id))
                    .ReturnsAsync(storageConsumerAccess);

            // when
            ConsumerAccess actualConsumerAccess = await this.consumerAccessService.RetrieveConsumerAccessByIdAsync(inputConsumerAccess.Id);

            // then
            actualConsumerAccess.Should().BeEquivalentTo(storageConsumerAccess);

            this.storageBroker.Verify(broker =>
                broker.SelectConsumerAccessByIdAsync(inputConsumerAccess.Id),
                    Times.Once);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBroker.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
        }
    }
}
