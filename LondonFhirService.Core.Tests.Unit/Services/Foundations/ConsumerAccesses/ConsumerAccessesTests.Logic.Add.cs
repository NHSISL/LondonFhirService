// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
using ISL.Security.Client.Models.Foundations.Users;
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
            User randomUser = CreateRandomUser();

            ConsumerAccess randomConsumerAccess =
                CreateRandomConsumerAccess(randomDateOffset, userId: randomUser.UserId.ToString());

            ConsumerAccess inputConsumerAccess = randomConsumerAccess;
            ConsumerAccess storageConsumerAccess = inputConsumerAccess.DeepClone();
            ConsumerAccess expectedConsumerAccess = inputConsumerAccess.DeepClone();

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateOffset);

            this.securityBrokerMock.Setup(broker =>
                broker.GetCurrentUserAsync())
                    .ReturnsAsync(randomUser);

            this.storageBroker.Setup(broker =>
                broker.InsertConsumerAccessAsync(inputConsumerAccess))
                    .ReturnsAsync(storageConsumerAccess);

            // when
            ConsumerAccess actualConsumerAccess = await this.consumerAccessService.AddConsumerAccessAsync(inputConsumerAccess);

            // then
            actualConsumerAccess.Should().BeEquivalentTo(expectedConsumerAccess);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Exactly(2));

            this.securityBrokerMock.Verify(broker =>
                broker.GetCurrentUserAsync(),
                    Times.Exactly(2));

            this.storageBroker.Verify(broker =>
                broker.InsertConsumerAccessAsync(inputConsumerAccess),
                    Times.Once);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBroker.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
        }
    }
}
