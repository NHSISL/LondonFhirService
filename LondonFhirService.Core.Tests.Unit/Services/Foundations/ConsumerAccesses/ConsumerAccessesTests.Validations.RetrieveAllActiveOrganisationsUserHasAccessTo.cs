// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.ConsumerAccesses;
using LondonFhirService.Core.Models.Foundations.ConsumerAccesses.Exceptions;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ConsumerAccesses
{
    public partial class ConsumerAccessesTests
    {
        [Fact]
        public async Task
            ShouldThrowValidationExceptionOnRetrieveAllActiveOrganisationsUserHasAccessToWhenArgsInvalidAsync()
        {
            // given
            Guid invalidConsumerAccessId = Guid.Empty;

            var invalidConsumerAccessException = new InvalidConsumerAccessException(
                message: "Invalid consumer access. Please correct the errors and try again.");

            invalidConsumerAccessException.AddData(
                key: nameof(ConsumerAccess.ConsumerId),
                values: "Id is invalid");

            var expectedConsumerAccessValidationException =
                new ConsumerAccessValidationException(
                    message: "ConsumerAccess validation error occurred, please fix errors and try again.",
                    innerException: invalidConsumerAccessException);

            // when
            ValueTask<List<string>> retrieveAllActiveOrganisationsUserHasAccessToTask =
                this.consumerAccessService.RetrieveAllActiveOrganisationsUserHasAccessToAsync(invalidConsumerAccessId);

            ConsumerAccessValidationException actualConsumerAccessValidationException =
                await Assert.ThrowsAsync<ConsumerAccessValidationException>(
                    testCode: retrieveAllActiveOrganisationsUserHasAccessToTask.AsTask);

            // then
            actualConsumerAccessValidationException.Should().BeEquivalentTo(expectedConsumerAccessValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConsumerAccessValidationException))), Times.Once());

            this.storageBroker.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
        }
    }
}
