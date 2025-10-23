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
        public async Task ShouldModifyConsumerAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            string randomUserId = GetRandomString();
            Consumer randomConsumer = CreateRandomModifyConsumer(randomDateTimeOffset);
            Consumer inputConsumer = randomConsumer;
            Consumer storageConsumer = inputConsumer.DeepClone();
            storageConsumer.UpdatedDate = randomConsumer.CreatedDate;
            Consumer auditAppliedConsumer = inputConsumer.DeepClone();
            auditAppliedConsumer.UpdatedBy = randomUserId;
            auditAppliedConsumer.UpdatedDate = randomDateTimeOffset;
            Consumer auditEnsuredConsumer = auditAppliedConsumer.DeepClone();
            Consumer updatedConsumer = inputConsumer;
            Consumer expectedConsumer = updatedConsumer.DeepClone();
            Guid consumerId = inputConsumer.Id;

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(inputConsumer))
                    .ReturnsAsync(auditAppliedConsumer);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectConsumerByIdAsync(consumerId))
                    .ReturnsAsync(storageConsumer);

            this.securityAuditBrokerMock.Setup(broker => broker
                .EnsureAddAuditValuesRemainsUnchangedOnModifyAsync(auditAppliedConsumer, storageConsumer))
                    .ReturnsAsync(auditEnsuredConsumer);

            this.storageBrokerMock.Setup(broker =>
                broker.UpdateConsumerAsync(auditEnsuredConsumer))
                    .ReturnsAsync(updatedConsumer);

            // when
            Consumer actualConsumer =
                await this.consumerService.ModifyConsumerAsync(inputConsumer);

            // then
            actualConsumer.Should().BeEquivalentTo(expectedConsumer);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(inputConsumer),
                    Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectConsumerByIdAsync(consumerId),
                    Times.Once);

            this.securityAuditBrokerMock.Verify(broker => broker
                .EnsureAddAuditValuesRemainsUnchangedOnModifyAsync(auditAppliedConsumer, storageConsumer),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateConsumerAsync(auditEnsuredConsumer),
                    Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}

