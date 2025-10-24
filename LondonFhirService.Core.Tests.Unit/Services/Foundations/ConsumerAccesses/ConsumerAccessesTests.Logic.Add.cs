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
        public async Task ShouldAddConsumerAccessAsync()
        {
            // given
            DateTimeOffset randomDateOffset = GetRandomDateTimeOffset();
            string randomUserId = Guid.NewGuid().ToString();

            ConsumerAccess randomConsumerAccess =
                CreateRandomConsumerAccess(randomDateOffset, userId: randomUserId.ToString());

            ConsumerAccess inputConsumerAccess = randomConsumerAccess;
            ConsumerAccess storageConsumerAccess = inputConsumerAccess.DeepClone();
            ConsumerAccess expectedConsumerAccess = inputConsumerAccess.DeepClone();

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(inputConsumerAccess))
                    .ReturnsAsync(inputConsumerAccess);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateOffset);

            this.storageBroker.Setup(broker =>
                broker.InsertConsumerAccessAsync(inputConsumerAccess))
                    .ReturnsAsync(storageConsumerAccess);

            // when
            ConsumerAccess actualConsumerAccess = await this.consumerAccessService
                .AddConsumerAccessAsync(inputConsumerAccess);

            // then
            actualConsumerAccess.Should().BeEquivalentTo(expectedConsumerAccess);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(inputConsumerAccess),
                    Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.storageBroker.Verify(broker =>
                broker.InsertConsumerAccessAsync(inputConsumerAccess),
                    Times.Once);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBroker.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
        }
    }
}
