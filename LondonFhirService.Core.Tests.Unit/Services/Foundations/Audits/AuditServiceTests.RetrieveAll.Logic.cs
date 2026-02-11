// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Brokers.Storages.Sql;
using LondonFhirService.Core.Models.Foundations.Audits;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Audits
{
    public partial class AuditServiceTests
    {
        [Fact]
        public async Task ShouldReturnAuditsAsync()
        {
            // given
            List<Audit> randomAudits = CreateRandomAudits();
            List<Audit> storageAudits = randomAudits;
            List<Audit> expectedAudits = storageAudits;

            this.storageBrokerFactoryMock.Setup(broker =>
                broker.CreateDbContextAsync())
                    .ReturnsAsync(this.storageBrokerMock.Object as StorageBroker);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectAllAuditsAsync())
                    .ReturnsAsync(storageAudits.AsQueryable());

            // when
            IQueryable<Audit> actualAudits =
                await this.auditService.RetrieveAllAuditsAsync();

            // then
            actualAudits.Should().BeEquivalentTo(expectedAudits);

            this.storageBrokerFactoryMock.Verify(broker =>
                broker.CreateDbContextAsync(),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAllAuditsAsync(),
                    Times.Once);

            this.storageBrokerFactoryMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
        }
    }
}