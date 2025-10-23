// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.Consumers;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Consumers
{
    public partial class ConsumerServiceTests
    {
        [Fact]
        public async Task ShouldReturnConsumers()
        {
            // given
            IQueryable<Consumer> randomConsumers = CreateRandomConsumers();
            IQueryable<Consumer> storageConsumers = randomConsumers;
            IQueryable<Consumer> expectedConsumers = storageConsumers;

            this.storageBrokerMock.Setup(broker =>
                    broker.SelectAllConsumersAsync())
                .ReturnsAsync(storageConsumers);

            // when
            IQueryable<Consumer> actualConsumers = await this.consumerService.RetrieveAllConsumersAsync();

            // then
            actualConsumers.Should().BeEquivalentTo(expectedConsumers);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAllConsumersAsync(),
                    Times.Once());

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}

