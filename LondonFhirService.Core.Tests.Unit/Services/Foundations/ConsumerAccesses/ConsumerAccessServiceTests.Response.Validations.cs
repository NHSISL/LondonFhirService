// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
using LondonFhirService.Core.Models.Brokers.ConsumerAccesses;
using LondonFhirService.Core.Models.Foundations.ConsumerAccesses.Exceptions;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ConsumerAccesses
{
    /// <summary>
    /// The response belongs to a third party, so it is checked here rather than dereferenced
    /// upstream. A 2xx carrying the literal JSON null deserialises to null, and an explicit null
    /// list overwrites the model's initialisers - either one used to surface as a
    /// NullReferenceException in the orchestration, which lost the compliance audit for that
    /// access decision on the way past.
    /// </summary>
    public partial class ConsumerAccessServiceTests
    {
        [Fact]
        public async Task ShouldThrowValidationExceptionOnCheckConsumerAccessIfResponseIsNullAndLogItAsync()
        {
            // given
            ValidateAccessRequest randomValidateAccessRequest = CreateRandomValidateAccessRequest();
            ValidateAccessRequest inputValidateAccessRequest = randomValidateAccessRequest;
            ConsumerAccess nullConsumerAccess = null;

            var nullConsumerAccessServiceException =
                new NullConsumerAccessServiceException(
                    message: "Consumer access response is null.");

            var expectedConsumerAccessServiceValidationException =
                new ConsumerAccessServiceValidationException(
                    message: "ConsumerAccess validation error occurred, please fix errors and try again.",
                    innerException: nullConsumerAccessServiceException);

            this.consumerAccessBrokerMock.Setup(broker =>
                broker.CheckConsumerAccessAsync(
                    It.IsAny<ValidateAccessRequest>(),
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(nullConsumerAccess);

            // when
            ValueTask<ConsumerAccess> checkConsumerAccessTask =
                this.consumerAccessService.CheckConsumerAccessAsync(
                    inputValidateAccessRequest, TestContext.Current.CancellationToken);

            ConsumerAccessServiceValidationException actualConsumerAccessServiceValidationException =
                await Assert.ThrowsAsync<ConsumerAccessServiceValidationException>(
                    testCode: checkConsumerAccessTask.AsTask);

            // then
            // Localised here, so an unusable access response is a ConsumerAccess validation
            // failure rather than a NullReferenceException surfacing three layers up.
            actualConsumerAccessServiceValidationException.Should()
                .BeEquivalentTo(expectedConsumerAccessServiceValidationException);

            this.consumerAccessBrokerMock.Verify(broker =>
                broker.CheckConsumerAccessAsync(
                    inputValidateAccessRequest, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConsumerAccessServiceValidationException))),
                        Times.Once);

            this.consumerAccessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnEmptyListsOnCheckConsumerAccessIfResponseListsAreNullAsync()
        {
            // given
            ValidateAccessRequest randomValidateAccessRequest = CreateRandomValidateAccessRequest();
            ValidateAccessRequest inputValidateAccessRequest = randomValidateAccessRequest;
            ConsumerAccess randomConsumerAccess = CreateRandomConsumerAccess();
            randomConsumerAccess.Reasons = null;
            randomConsumerAccess.AllowedViaOrganisations = null;
            randomConsumerAccess.AllowedViaInformationSharingAgreements = null;
            ConsumerAccess returnedConsumerAccess = randomConsumerAccess;

            // Snapshotted before the call, because the service normalises the response in place -
            // reading the expectation back off the same instance afterwards would assert nothing.
            ConsumerAccess expectedConsumerAccess = returnedConsumerAccess.DeepClone();
            expectedConsumerAccess.Reasons = new List<AccessReason>();
            expectedConsumerAccess.AllowedViaOrganisations = new List<string>();
            expectedConsumerAccess.AllowedViaInformationSharingAgreements = new List<string>();
            using var cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            this.consumerAccessBrokerMock.Setup(broker =>
                broker.CheckConsumerAccessAsync(inputValidateAccessRequest, cancellationToken))
                    .ReturnsAsync(returnedConsumerAccess);

            // when
            ConsumerAccess actualConsumerAccess = await this.consumerAccessService
                .CheckConsumerAccessAsync(inputValidateAccessRequest, cancellationToken);

            // then
            // Empty rather than null: callers enumerate these to build the audit of the access
            // decision, and an absent list is not the same thing as an unusable response.
            actualConsumerAccess.Reasons.Should().NotBeNull().And.BeEmpty();
            actualConsumerAccess.AllowedViaOrganisations.Should().NotBeNull().And.BeEmpty();
            actualConsumerAccess.AllowedViaInformationSharingAgreements.Should().NotBeNull().And.BeEmpty();

            // Nothing else about the decision is touched on the way through.
            actualConsumerAccess.Should().BeEquivalentTo(expectedConsumerAccess);

            this.consumerAccessBrokerMock.Verify(broker =>
                broker.CheckConsumerAccessAsync(inputValidateAccessRequest, cancellationToken),
                    Times.Once);

            this.consumerAccessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
