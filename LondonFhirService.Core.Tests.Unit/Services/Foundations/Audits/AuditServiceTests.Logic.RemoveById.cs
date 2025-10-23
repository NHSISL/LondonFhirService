// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
using LondonFhirService.Core.Models.Foundations.Audits;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Audits
{
    public partial class AuditServiceTests
    {
        [Fact]
        public async Task ShouldRemoveAuditByIdAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            string randomUserId = GetRandomStringWithLengthOf(50);
            Audit randomAudit = CreateRandomAudit(randomDateTimeOffset, randomUserId);
            Guid inputAuditId = randomAudit.Id;
            Audit storageAudit = randomAudit;
            Audit ingestionTrackingWithDeleteAuditApplied = storageAudit.DeepClone();
            ingestionTrackingWithDeleteAuditApplied.UpdatedBy = randomUserId;
            ingestionTrackingWithDeleteAuditApplied.UpdatedDate = randomDateTimeOffset;
            Audit updatedAudit = storageAudit;
            Audit deletedAudit = updatedAudit;
            Audit expectedAudit = deletedAudit.DeepClone();

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectAuditByIdAsync(inputAuditId))
                    .ReturnsAsync(storageAudit);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyRemoveAuditValuesAsync(storageAudit))
                    .ReturnsAsync(storageAudit);

            this.storageBrokerMock.Setup(broker =>
                broker.UpdateAuditAsync(randomAudit))
                    .ReturnsAsync(updatedAudit);

            this.storageBrokerMock.Setup(broker =>
                broker.DeleteAuditAsync(updatedAudit))
                    .ReturnsAsync(deletedAudit);

            // when
            Audit actualAudit = await this.auditService
                .RemoveAuditByIdAsync(inputAuditId);

            // then
            actualAudit.Should().BeEquivalentTo(expectedAudit);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAuditByIdAsync(inputAuditId),
                    Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyRemoveAuditValuesAsync(storageAudit),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateAuditAsync(randomAudit),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.DeleteAuditAsync(updatedAudit),
                    Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}