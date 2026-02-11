// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
using LondonFhirService.Core.Brokers.Storages.Sql;
using LondonFhirService.Core.Models.Foundations.Audits;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Audits
{
    public partial class AuditServiceTests
    {
        [Fact]
        public async Task ShouldModifyAuditAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            string randomUserId = GetRandomStringWithLengthOf(50);
            Audit randomAudit = CreateRandomModifyAudit(randomDateTimeOffset, randomUserId);
            Audit inputAudit = randomAudit;
            Audit storageAudit = inputAudit.DeepClone();
            storageAudit.UpdatedDate = randomAudit.CreatedDate;
            Audit updatedAudit = inputAudit;
            Audit expectedAudit = updatedAudit.DeepClone();
            Guid auditId = inputAudit.Id;

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(inputAudit))
                    .ReturnsAsync(inputAudit);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            this.storageBrokerFactoryMock.Setup(broker =>
                broker.CreateDbContextAsync())
                    .ReturnsAsync(this.storageBrokerMock.Object as StorageBroker);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectAuditByIdAsync(auditId))
                    .ReturnsAsync(storageAudit);

            this.storageBrokerMock.Setup(broker =>
                broker.UpdateAuditAsync(inputAudit))
                    .ReturnsAsync(updatedAudit);

            // when
            Audit actualAudit =
                await this.auditService.ModifyAuditAsync(inputAudit);

            // then
            actualAudit.Should().BeEquivalentTo(expectedAudit);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(inputAudit),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once);

            this.storageBrokerFactoryMock.Verify(broker =>
                broker.CreateDbContextAsync(),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAuditByIdAsync(inputAudit.Id),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateAuditAsync(inputAudit),
                    Times.Once);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerFactoryMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}