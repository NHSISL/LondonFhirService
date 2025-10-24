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
        public async Task ShouldModifyConsumerAccessAsync()
        {
            // given
            DateTimeOffset randomDateOffset = GetRandomDateTimeOffset();
            string randomUserId = Guid.NewGuid().ToString();

            ConsumerAccess randomModifyConsumerAccess =
                CreateRandomModifyConsumerAccess(randomDateOffset, userId: randomUserId.ToString());

            ConsumerAccess inputConsumerAccess = randomModifyConsumerAccess.DeepClone();
            ConsumerAccess storageConsumerAccess = randomModifyConsumerAccess.DeepClone();
            storageConsumerAccess.UpdatedDate = storageConsumerAccess.CreatedDate;
            ConsumerAccess updatedConsumerAccess = inputConsumerAccess.DeepClone();
            ConsumerAccess expectedConsumerAccess = updatedConsumerAccess.DeepClone();

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(inputConsumerAccess))
                    .ReturnsAsync(inputConsumerAccess);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateOffset);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            this.storageBroker.Setup(broker =>
                broker.SelectConsumerAccessByIdAsync(inputConsumerAccess.Id))
                    .ReturnsAsync(storageConsumerAccess);

            this.storageBroker.Setup(broker =>
                broker.UpdateConsumerAccessAsync(inputConsumerAccess))
                    .ReturnsAsync(updatedConsumerAccess);

            // when
            ConsumerAccess actualConsumerAccess =
                await this.consumerAccessService.ModifyConsumerAccessAsync(inputConsumerAccess);

            // then
            actualConsumerAccess.Should().BeEquivalentTo(expectedConsumerAccess);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(inputConsumerAccess),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once);

            this.storageBroker.Verify(broker =>
                broker.SelectConsumerAccessByIdAsync(inputConsumerAccess.Id),
                    Times.Once);

            this.storageBroker.Verify(broker =>
                broker.UpdateConsumerAccessAsync(inputConsumerAccess),
                    Times.Once);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBroker.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
        }
    }
}
