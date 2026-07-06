// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Brokers.ConsumerAccesses;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ConsumerAccesses
{
    public partial class ConsumerAccessesTests
    {
        [Fact]
        public async Task ShouldCheckConsumerAccessAsync()
        {
            // given
            string randomUserId = GetRandomString();
            string randomNhsNumber = GetRandomString();
            Guid randomCorrelationId = GetRandomGuid();
            CancellationToken cancellationToken = CancellationToken.None;
            ConsumerAccess randomConsumerAccess = CreateRandomConsumerAccess();
            ConsumerAccess expectedConsumerAccess = randomConsumerAccess;

            var expectedValidateAccessRequest = new ValidateAccessRequest
            {
                ConsumerUserId = randomUserId,
                NhsNumber = randomNhsNumber,
                CorrelationId = randomCorrelationId
            };

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            this.consumerAccessBrokerMock.Setup(broker =>
                broker.CheckConsumerAccessAsync(
                    It.Is(SameValidateAccessRequestAs(expectedValidateAccessRequest)),
                    cancellationToken))
                        .ReturnsAsync(randomConsumerAccess);

            // when
            ConsumerAccess actualConsumerAccess =
                await this.consumerAccessService.CheckConsumerAccessAsync(
                    NhsNumber: randomNhsNumber,
                    CorrelationId: randomCorrelationId,
                    cancellationToken: cancellationToken);

            // then
            actualConsumerAccess.Should().BeEquivalentTo(expectedConsumerAccess);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Exactly(2));

            this.consumerAccessBrokerMock.Verify(broker =>
                broker.CheckConsumerAccessAsync(
                    It.Is(SameValidateAccessRequestAs(expectedValidateAccessRequest)),
                    cancellationToken),
                        Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.consumerAccessBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
