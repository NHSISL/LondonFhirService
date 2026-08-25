// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
using LondonFhirService.Core.Models.Brokers.ConsumerAccesses;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ConsumerAccesses
{
    public partial class ConsumerAccessServiceTests
    {
        [Fact]
        public async Task ShouldCheckConsumerAccessAsync()
        {
            // given
            ValidateAccessRequest randomValidateAccessRequest = CreateRandomValidateAccessRequest();
            ValidateAccessRequest inputValidateAccessRequest = randomValidateAccessRequest;
            ConsumerAccess randomConsumerAccess = CreateRandomConsumerAccess();
            ConsumerAccess returnedConsumerAccess = randomConsumerAccess;
            ConsumerAccess expectedConsumerAccess = returnedConsumerAccess.DeepClone();
            using var cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            this.consumerAccessBrokerMock.Setup(broker =>
                broker.CheckConsumerAccessAsync(inputValidateAccessRequest, cancellationToken))
                    .ReturnsAsync(returnedConsumerAccess);

            // when
            ConsumerAccess actualConsumerAccess = await this.consumerAccessService
                .CheckConsumerAccessAsync(inputValidateAccessRequest, cancellationToken);

            // then
            actualConsumerAccess.Should().BeEquivalentTo(expectedConsumerAccess);

            this.consumerAccessBrokerMock.Verify(broker =>
                broker.CheckConsumerAccessAsync(inputValidateAccessRequest, cancellationToken),
                    Times.Once);

            this.consumerAccessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldCheckConsumerAccessWithDefaultCancellationTokenAsync()
        {
            // given
            ValidateAccessRequest randomValidateAccessRequest = CreateRandomValidateAccessRequest();
            ValidateAccessRequest inputValidateAccessRequest = randomValidateAccessRequest;
            ConsumerAccess randomConsumerAccess = CreateRandomConsumerAccess();
            ConsumerAccess returnedConsumerAccess = randomConsumerAccess;
            ConsumerAccess expectedConsumerAccess = returnedConsumerAccess.DeepClone();

            this.consumerAccessBrokerMock.Setup(broker =>
                broker.CheckConsumerAccessAsync(inputValidateAccessRequest, default))
                    .ReturnsAsync(returnedConsumerAccess);

            // when
            // The omitted token is the subject of this test, so the analyzer prompt to pass
            // TestContext.Current.CancellationToken does not apply here.
#pragma warning disable xUnit1051
            ConsumerAccess actualConsumerAccess = await this.consumerAccessService
                .CheckConsumerAccessAsync(inputValidateAccessRequest);
#pragma warning restore xUnit1051

            // then
            actualConsumerAccess.Should().BeEquivalentTo(expectedConsumerAccess);

            this.consumerAccessBrokerMock.Verify(broker =>
                broker.CheckConsumerAccessAsync(inputValidateAccessRequest, default),
                    Times.Once);

            this.consumerAccessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
