// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Force.DeepCloner;
using ISL.Security.Client.Models.Foundations.Users;
using LondonFhirService.Core.Models.Foundations.Consumers;
using LondonFhirService.Core.Models.Orchestrations.Accesses;
using LondonFhirService.Core.Services.Orchestrations.Accesses;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Orchestrations.Accesses
{
    public partial class AccessOrchestrationServiceTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ShouldValidateAccess(bool useHashedNhsNumber)
        {
            // given
            string userId = GetRandomString();
            User randomUser = CreateRandomUser(userId);
            User outputUser = randomUser;
            Consumer randomConsumer = CreateRandomConsumer(userId);
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            DateTimeOffset validActiveFromDate = randomDateTimeOffset.AddDays(-2);
            DateTimeOffset validActiveToDate = randomDateTimeOffset.AddDays(2);
            randomConsumer.ActiveFrom = validActiveFromDate;
            randomConsumer.ActiveTo = validActiveToDate;
            Consumer inputConsumer = randomConsumer.DeepClone();
            Guid correlationId = Guid.NewGuid();

            JsonSerializerOptions options = new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };

            string currentUserJson = JsonSerializer.Serialize(outputUser, options);

            IQueryable<Consumer> storageConsumers =
                new List<Consumer> { inputConsumer }.AsQueryable();

            Guid randomGuid = Guid.NewGuid();
            string randomNhsNumber = GetRandomStringWithLength(5);
            string inputNhsNumber = randomNhsNumber;
            string userOrganisation = GetRandomStringWithLength(5);

            List<string> userOrganisations =
                new List<string> { userOrganisation };

            var accessConfigurations = new AccessConfigurations
            {
                UseHashedNhsNumber = useHashedNhsNumber,
                HashPepper = GetRandomString(),
            };

            var accessOrchestrationService = new AccessOrchestrationService(
                consumerService: this.consumerServiceMock.Object,
                consumerAccessService: this.consumerAccessServiceMock.Object,
                pdsDataService: this.pdsDataServiceMock.Object,
                auditBroker: this.auditBrokerMock.Object,
                securityBroker: this.securityBrokerMock.Object,
                dateTimeBroker: this.dateTimeBrokerMock.Object,
                identifierBroker: this.identifierBrokerMock.Object,
                loggingBroker: this.loggingBrokerMock.Object,
                hashBroker: this.hashBrokerMock.Object,
                accessConfigurations: accessConfigurations);

            this.securityBrokerMock.Setup(broker =>
                broker.GetCurrentUserAsync())
                    .ReturnsAsync(outputUser);

            this.hashBrokerMock.Setup(broker =>
                broker.GenerateSha256HashAsync(inputNhsNumber, accessConfigurations.HashPepper))
                    .ReturnsAsync(inputNhsNumber);

            this.consumerServiceMock.Setup(service =>
                service.RetrieveAllConsumersAsync())
                    .ReturnsAsync(storageConsumers);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.consumerAccessServiceMock.Setup(service =>
                service.RetrieveAllActiveOrganisationsUserHasAccessToAsync(inputConsumer.Id))
                    .ReturnsAsync(userOrganisations);

            this.pdsDataServiceMock.Setup(service =>
                service.OrganisationsHaveAccessToThisPatient(inputNhsNumber, userOrganisations))
                    .ReturnsAsync(true);

            // when
            await accessOrchestrationService.ValidateAccess(inputNhsNumber, correlationId);

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
                    correlationId.ToString()),
                        Times.Once);

            this.consumerServiceMock.Verify(service =>
                service.RetrieveAllConsumersAsync(),
                    Times.Once);

            if (useHashedNhsNumber)
            {
                this.hashBrokerMock.Verify(broker =>
                    broker.GenerateSha256HashAsync(inputNhsNumber, accessConfigurations.HashPepper),
                        Times.Once);
            }

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.consumerAccessServiceMock.Verify(service =>
                service.RetrieveAllActiveOrganisationsUserHasAccessToAsync(inputConsumer.Id),
                    Times.Once);

            this.pdsDataServiceMock.Verify(service =>
                service.OrganisationsHaveAccessToThisPatient(inputNhsNumber, userOrganisations),
                    Times.Once);

            this.consumerServiceMock.VerifyNoOtherCalls();
            this.consumerAccessServiceMock.VerifyNoOtherCalls();
            this.pdsDataServiceMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.hashBrokerMock.VerifyNoOtherCalls();
        }
    }
}
