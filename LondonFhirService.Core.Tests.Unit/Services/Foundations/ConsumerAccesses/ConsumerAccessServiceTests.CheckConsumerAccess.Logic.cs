// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Net;
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

            ConsumerAccessResponse returnedConsumerAccessResponse =
                CreateConsumerAccessResponse(HttpStatusCode.OK, returnedConsumerAccess);

            using var cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            this.consumerAccessBrokerMock.Setup(broker =>
                broker.CheckConsumerAccessAsync(inputValidateAccessRequest, cancellationToken))
                    .ReturnsAsync(returnedConsumerAccessResponse);

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

            ConsumerAccessResponse returnedConsumerAccessResponse =
                CreateConsumerAccessResponse(HttpStatusCode.OK, returnedConsumerAccess);

            this.consumerAccessBrokerMock.Setup(broker =>
                broker.CheckConsumerAccessAsync(inputValidateAccessRequest, default))
                    .ReturnsAsync(returnedConsumerAccessResponse);

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

        [Fact]
        public async Task ShouldCheckConsumerAccessAsDeniedIfAccessIsForbiddenAsync()
        {
            // given
            ValidateAccessRequest randomValidateAccessRequest = CreateRandomValidateAccessRequest();
            ValidateAccessRequest inputValidateAccessRequest = randomValidateAccessRequest;
            ConsumerAccess randomConsumerAccess = CreateRandomConsumerAccess();
            randomConsumerAccess.IsAccessAllowed = false;
            randomConsumerAccess.AllowedViaOrganisations = new List<string>();
            randomConsumerAccess.AllowedViaInformationSharingAgreements = new List<string>();
            ConsumerAccess returnedConsumerAccess = randomConsumerAccess;
            ConsumerAccess expectedConsumerAccess = returnedConsumerAccess.DeepClone();

            ConsumerAccessResponse returnedConsumerAccessResponse =
                CreateConsumerAccessResponse(HttpStatusCode.Forbidden, returnedConsumerAccess);

            using var cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            this.consumerAccessBrokerMock.Setup(broker =>
                broker.CheckConsumerAccessAsync(inputValidateAccessRequest, cancellationToken))
                    .ReturnsAsync(returnedConsumerAccessResponse);

            // when
            ConsumerAccess actualConsumerAccess = await this.consumerAccessService
                .CheckConsumerAccessAsync(inputValidateAccessRequest, cancellationToken);

            // then
            // A 403 is an answer, not a failure: the same Access body a 200 carries, refusing. It
            // comes back exactly as a 200 with IsAccessAllowed false would, reasons and all, so
            // the denial is audited upstream instead of surfacing as a dependency error.
            actualConsumerAccess.Should().BeEquivalentTo(expectedConsumerAccess);

            this.consumerAccessBrokerMock.Verify(broker =>
                broker.CheckConsumerAccessAsync(inputValidateAccessRequest, cancellationToken),
                    Times.Once);

            this.consumerAccessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldCheckConsumerAccessAsDeniedIfAccessIsForbiddenWhateverTheBodyClaimsAsync()
        {
            // given
            ValidateAccessRequest randomValidateAccessRequest = CreateRandomValidateAccessRequest();
            ValidateAccessRequest inputValidateAccessRequest = randomValidateAccessRequest;
            ConsumerAccess randomConsumerAccess = CreateRandomConsumerAccess();
            randomConsumerAccess.IsAccessAllowed = true;
            ConsumerAccess returnedConsumerAccess = randomConsumerAccess;
            ConsumerAccess expectedConsumerAccess = returnedConsumerAccess.DeepClone();
            expectedConsumerAccess.IsAccessAllowed = false;

            ConsumerAccessResponse returnedConsumerAccessResponse =
                CreateConsumerAccessResponse(HttpStatusCode.Forbidden, returnedConsumerAccess);

            using var cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            this.consumerAccessBrokerMock.Setup(broker =>
                broker.CheckConsumerAccessAsync(inputValidateAccessRequest, cancellationToken))
                    .ReturnsAsync(returnedConsumerAccessResponse);

            // when
            ConsumerAccess actualConsumerAccess = await this.consumerAccessService
                .CheckConsumerAccessAsync(inputValidateAccessRequest, cancellationToken);

            // then
            // The status is the decision. A refusal whose body contradicts it fails closed, rather
            // than a 403 being read as permission to release a patient's record.
            actualConsumerAccess.Should().BeEquivalentTo(expectedConsumerAccess);

            this.consumerAccessBrokerMock.Verify(broker =>
                broker.CheckConsumerAccessAsync(inputValidateAccessRequest, cancellationToken),
                    Times.Once);

            this.consumerAccessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
