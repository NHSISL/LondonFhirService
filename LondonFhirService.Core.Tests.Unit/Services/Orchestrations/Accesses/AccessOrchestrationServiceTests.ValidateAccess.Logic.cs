// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Force.DeepCloner;
using ISL.Security.Client.Models.Foundations.Users;
using LondonFhirService.Core.Models.Foundations.Consumers;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Orchestrations.Accesses
{
    public partial class AccessOrchestrationServiceTests
    {
        [Fact]
        public async Task ShouldValidateAccess()
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

            IQueryable<Consumer> storageConsumers =
                new List<Consumer> { inputConsumer }.AsQueryable();

            Guid randomGuid = Guid.NewGuid();
            string randomNhsNumber = GetRandomStringWithLength(5);
            string inputNhsNumber = randomNhsNumber;

            string userOrganisation = GetRandomStringWithLength(5);

            List<string> userOrganisations =
                new List<string> { userOrganisation };

            this.securityBrokerMock.Setup(broker =>
                broker.GetCurrentUserAsync())
                    .ReturnsAsync(outputUser);

            this.consumerServiceMock.Setup(service =>
                service.RetrieveAllConsumersAsync())
                    .ReturnsAsync(storageConsumers);

            this.identifierBrokerMock.Setup(broker =>
                broker.GetIdentifierAsync())
                    .ReturnsAsync(randomGuid);

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

            this.consumerServiceMock.Verify(service =>
                service.RetrieveAllConsumersAsync(),
                    Times.Once);

            this.identifierBrokerMock.Verify(broker =>
                broker.GetIdentifierAsync(),
                    Times.Once);

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
        }
    }
}
