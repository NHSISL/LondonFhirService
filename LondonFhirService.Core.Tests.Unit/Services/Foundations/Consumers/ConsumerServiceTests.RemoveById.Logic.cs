// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
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
        public async Task ShouldRemoveConsumerByIdAsync()
        {
            // given
            Guid randomId = Guid.NewGuid();
            Guid inputConsumerId = randomId;
            Consumer randomConsumer = CreateRandomConsumer();
            Consumer storageConsumer = randomConsumer;
            Consumer expectedInputConsumer = storageConsumer;
            Consumer deletedConsumer = expectedInputConsumer;
            Consumer expectedConsumer = deletedConsumer.DeepClone();

            this.storageBrokerMock.Setup(broker =>
                broker.SelectConsumerByIdAsync(inputConsumerId))
                    .ReturnsAsync(storageConsumer);

            this.storageBrokerMock.Setup(broker =>
                broker.DeleteConsumerAsync(expectedInputConsumer))
                    .ReturnsAsync(deletedConsumer);

            // when
            Consumer actualConsumer = await this.consumerService
                .RemoveConsumerByIdAsync(inputConsumerId);

            // then
            actualConsumer.Should().BeEquivalentTo(expectedConsumer);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectConsumerByIdAsync(inputConsumerId),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.DeleteConsumerAsync(expectedInputConsumer),
                    Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
