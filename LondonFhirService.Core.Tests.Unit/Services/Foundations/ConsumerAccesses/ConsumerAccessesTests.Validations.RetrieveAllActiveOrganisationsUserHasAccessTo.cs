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

            var invalidConsumerAccessServiceException = new InvalidConsumerAccessServiceException(
                message: "Invalid consumer access. Please correct the errors and try again.");

            invalidConsumerAccessServiceException.AddData(
                key: nameof(ConsumerAccess.ConsumerId),
                values: "Id is invalid");

            var expectedConsumerAccessServiceValidationException =
                new ConsumerAccessServiceValidationException(
                    message: "ConsumerAccess validation error occurred, please fix errors and try again.",
                    innerException: invalidConsumerAccessServiceException);

            // when
            ValueTask<List<string>> retrieveAllActiveOrganisationsUserHasAccessToTask =
                this.consumerAccessService.RetrieveAllActiveOrganisationsUserHasAccessToAsync(invalidConsumerAccessId);

            ConsumerAccessServiceValidationException actualConsumerAccessServiceValidationException =
                await Assert.ThrowsAsync<ConsumerAccessServiceValidationException>(
                    testCode: retrieveAllActiveOrganisationsUserHasAccessToTask.AsTask);

            // then
            actualConsumerAccessServiceValidationException.Should().BeEquivalentTo(expectedConsumerAccessServiceValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConsumerAccessServiceValidationException))), Times.Once());

            this.storageBroker.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
        }
    }
}
