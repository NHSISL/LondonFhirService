// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
using LondonFhirService.Core.Models.Foundations.Consumers;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Consumers
{
    public partial class ConsumerServiceTests
    {
        [Fact]
        public async Task ShouldRetrieveConsumerByIdAsync()
        {
            // given
            Consumer randomConsumer = CreateRandomConsumer();
            Consumer inputConsumer = randomConsumer;
            Consumer storageConsumer = randomConsumer;
            Consumer expectedConsumer = storageConsumer.DeepClone();

            this.storageBrokerMock.Setup(broker =>
                broker.SelectConsumerByIdAsync(inputConsumer.Id))
                    .ReturnsAsync(storageConsumer);

            // when
            Consumer actualConsumer =
                await this.consumerService.RetrieveConsumerByIdAsync(inputConsumer.Id);

            // then
            actualConsumer.Should().BeEquivalentTo(expectedConsumer);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectConsumerByIdAsync(inputConsumer.Id),
                    Times.Once());

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
