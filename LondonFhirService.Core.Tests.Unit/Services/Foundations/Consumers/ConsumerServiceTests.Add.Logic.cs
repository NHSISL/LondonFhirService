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
        public async Task ShouldAddConsumerAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            string randomUserId = GetRandomString();
            Consumer randomConsumer = CreateRandomConsumer(randomDateTimeOffset);
            Consumer inputConsumer = randomConsumer;
            Consumer auditAppliedConsumer = inputConsumer.DeepClone();
            auditAppliedConsumer.CreatedBy = randomUserId;
            auditAppliedConsumer.CreatedDate = randomDateTimeOffset;
            auditAppliedConsumer.UpdatedBy = randomUserId;
            auditAppliedConsumer.UpdatedDate = randomDateTimeOffset;
            Consumer storageConsumer = auditAppliedConsumer.DeepClone();
            Consumer expectedConsumer = storageConsumer.DeepClone();

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(inputConsumer))
                    .ReturnsAsync(auditAppliedConsumer);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.storageBrokerMock.Setup(broker =>
                broker.InsertConsumerAsync(auditAppliedConsumer))
                    .ReturnsAsync(storageConsumer);

            // when
            Consumer actualConsumer = await this.consumerService.AddConsumerAsync(inputConsumer);

            // then
            actualConsumer.Should().BeEquivalentTo(expectedConsumer);

            this.securityAuditBrokerMock.Verify(broker =>
                    broker.ApplyAddAuditValuesAsync(inputConsumer),
                Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                    broker.GetUserIdAsync(),
                Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                    broker.GetCurrentDateTimeOffsetAsync(),
                Times.Once());

            this.storageBrokerMock.Verify(broker =>
                    broker.InsertConsumerAsync(auditAppliedConsumer),
                Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
