// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ISL.Security.Client.Models.Foundations.Users;
using LondonFhirService.Core.Models.Brokers.ConsumerAccesses;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Orchestrations.Accesses
{
    public partial class AccessOrchestrationServiceTests
    {
        [Fact]
        public async Task ShouldValidateAccessAsync()
        {
            // given
            string randomUserId = GetRandomString();
            User randomUser = CreateRandomUser(randomUserId);
            string randomNhsNumber = GetRandomString();
            Guid randomCorrelationId = GetRandomGuid();
            ConsumerAccess randomConsumerAccess = CreateRandomConsumerAccess(isAccessAllowed: true);
            JsonSerializerOptions options = CreateJsonSerializerOptions();
            string currentUserJson = JsonSerializer.Serialize(randomUser, options);

            string expectedAccessAllowedMessagePrefix =
                $"{randomUserId} is allowed to access patient with " +
                $"NHS number {randomNhsNumber} via " + Environment.NewLine +

                $"Subscriber Aggreement(s): " +
                    $"{string.Join(", ", randomConsumerAccess.AllowedViaInformationSharingAgreements)}, " +
                        Environment.NewLine +

                $"Org Codes: {string.Join(", ", randomConsumerAccess.AllowedViaOrganisations)},  " +
                    Environment.NewLine +

                $"Consumer Access: " + Environment.NewLine;

            this.securityBrokerMock.Setup(broker =>
                broker.GetCurrentUserAsync())
                    .ReturnsAsync(randomUser);

            this.consumerAccessServiceMock.Setup(service =>
                service.CheckConsumerAccessAsync(
                    randomNhsNumber,
                    randomCorrelationId,
                    CancellationToken.None))
                        .ReturnsAsync(randomConsumerAccess);

            // when
            await this.accessOrchestrationService.ValidateAccess(randomNhsNumber, randomCorrelationId);

            // then
            this.securityBrokerMock.Verify(broker =>
                broker.GetCurrentUserAsync(),
                    Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    "Access",
                    "Check Access Permissons",
                    currentUserJson,
                    null,
                    randomCorrelationId.ToString()),
                        Times.Once);

            this.consumerAccessServiceMock.Verify(service =>
                service.CheckConsumerAccessAsync(
                    randomNhsNumber,
                    randomCorrelationId,
                    CancellationToken.None),
                        Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    "Access",
                    "Access Allowed",
                    It.Is<string>(s => s.StartsWith(expectedAccessAllowedMessagePrefix)),
                    null,
                    randomCorrelationId.ToString()),
                        Times.Once);

            this.securityBrokerMock.VerifyNoOtherCalls();
            this.consumerAccessServiceMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.hashBrokerMock.VerifyNoOtherCalls();
        }
    }
}
