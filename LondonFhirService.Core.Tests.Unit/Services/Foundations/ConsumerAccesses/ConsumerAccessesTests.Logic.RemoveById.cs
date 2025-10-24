// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
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
        public async Task ShouldRemoveConsumerAccessByIdAsync()
        {
            // given
            DateTimeOffset randomDateOffset = GetRandomDateTimeOffset();
            string randomUserId = Guid.NewGuid().ToString();
            ConsumerAccess randomConsumerAccess = CreateRandomConsumerAccess();
            ConsumerAccess inputConsumerAccess = randomConsumerAccess;
            Guid inputConsumerAccessId = inputConsumerAccess.Id;
            ConsumerAccess storageConsumerAccess = inputConsumerAccess.DeepClone();
            ConsumerAccess updatedConsumerAccess = storageConsumerAccess.DeepClone();
            updatedConsumerAccess.UpdatedBy = randomUserId;
            updatedConsumerAccess.UpdatedDate = randomDateOffset;
            ConsumerAccess deletedConsumerAccess = updatedConsumerAccess.DeepClone();
            ConsumerAccess expectedConsumerAccess = deletedConsumerAccess.DeepClone();

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateOffset);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            this.storageBroker.Setup(broker =>
                broker.SelectConsumerAccessByIdAsync(inputConsumerAccessId))
                    .ReturnsAsync(storageConsumerAccess);

            this.storageBroker.Setup(broker =>
                broker.UpdateConsumerAccessAsync(It.Is(SameConsumerAccessAs(updatedConsumerAccess))))
                    .ReturnsAsync(updatedConsumerAccess);

            this.storageBroker.Setup(broker =>
                broker.DeleteConsumerAccessAsync(It.Is(SameConsumerAccessAs(updatedConsumerAccess))))
                    .ReturnsAsync(deletedConsumerAccess);

            // when
            ConsumerAccess actualConsumerAccess =
                await this.consumerAccessService.RemoveConsumerAccessByIdAsync(inputConsumerAccessId);

            // then
            actualConsumerAccess.Should().BeEquivalentTo(expectedConsumerAccess);

            this.storageBroker.Verify(broker =>
                broker.SelectConsumerAccessByIdAsync(inputConsumerAccessId),
                    Times.Once());

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Exactly(2));

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once());

            this.storageBroker.Verify(broker =>
                broker.UpdateConsumerAccessAsync(It.Is(SameConsumerAccessAs(updatedConsumerAccess))),
                    Times.Once());

            this.storageBroker.Verify(broker =>
                broker.DeleteConsumerAccessAsync(It.Is(SameConsumerAccessAs(updatedConsumerAccess))),
                    Times.Once);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBroker.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
        }
    }
}
