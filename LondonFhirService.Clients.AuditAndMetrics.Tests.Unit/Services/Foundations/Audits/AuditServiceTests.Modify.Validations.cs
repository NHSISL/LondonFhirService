// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Clients.AuditAndMetrics.Models.Audits.Exceptions;
using LondonFhirService.Clients.AuditAndMetrics.Tests.Unit.Models.Audits;
using LondonFhirService.Core.Abstractions.Models.Audits;
using Moq;

namespace LondonFhirService.Clients.AuditAndMetrics.Tests.Unit.Services.Foundations.Audits
{
    /// <summary>
    /// An update carries a whole entity, so without a comparison against the stored row a caller
    /// could rewrite who created an audit entry and when. For an access decision that is the
    /// exact record the audit trail exists to protect.
    /// </summary>
    public partial class AuditServiceTests
    {
        [Fact]
        public async Task ShouldThrowValidationExceptionOnModifyIfCreatedByWasChangedAsync()
        {
            // given
            DateTimeOffset createdDate = GetRandomDateTimeOffset();
            TestAudit storedAudit = CreateUnstampedAudit();
            storedAudit.CreatedBy = "original.author@nhs.net";
            storedAudit.CreatedDate = createdDate;
            storedAudit.UpdatedBy = "original.author@nhs.net";
            storedAudit.UpdatedDate = createdDate;

            TestAudit tamperedAudit = CreateUnstampedAudit();
            tamperedAudit.Id = storedAudit.Id;
            tamperedAudit.CreatedBy = "someone.else@nhs.net";
            tamperedAudit.CreatedDate = createdDate;
            tamperedAudit.UpdatedBy = "someone.else@nhs.net";

            this.dateTimeBrokerMock.Setup(broker => broker.GetCurrentDateTimeOffsetAsync())
                .ReturnsAsync(GetRandomDateTimeOffset());

            this.storageBrokerMock.Setup(broker =>
                broker.SelectAuditByIdAsync(storedAudit.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(storedAudit);

            // when
            ValueTask<IAudit> modifyAuditTask =
                this.auditService.ModifyAuditAsync(tamperedAudit, TestContext.Current.CancellationToken);

            // then
            await Assert.ThrowsAsync<AuditValidationException>(modifyAuditTask.AsTask);

            // The row is left exactly as it was; nothing reaches storage.
            this.storageBrokerMock.Verify(broker =>
                broker.UpdateAuditAsync(It.IsAny<IAudit>(), It.IsAny<CancellationToken>()),
                    Times.Never);
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnModifyIfCreatedDateWasChangedAsync()
        {
            // given
            DateTimeOffset createdDate = GetRandomDateTimeOffset();
            TestAudit storedAudit = CreateUnstampedAudit();
            storedAudit.CreatedBy = "original.author@nhs.net";
            storedAudit.CreatedDate = createdDate;

            TestAudit tamperedAudit = CreateUnstampedAudit();
            tamperedAudit.Id = storedAudit.Id;
            tamperedAudit.CreatedBy = "original.author@nhs.net";
            tamperedAudit.CreatedDate = createdDate.AddYears(-5);

            this.dateTimeBrokerMock.Setup(broker => broker.GetCurrentDateTimeOffsetAsync())
                .ReturnsAsync(GetRandomDateTimeOffset());

            this.storageBrokerMock.Setup(broker =>
                broker.SelectAuditByIdAsync(storedAudit.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(storedAudit);

            // when
            ValueTask<IAudit> modifyAuditTask =
                this.auditService.ModifyAuditAsync(tamperedAudit, TestContext.Current.CancellationToken);

            // then
            // Backdating an entry is as damaging as re-attributing one - it moves the record out
            // of the window anybody would look in.
            await Assert.ThrowsAsync<AuditValidationException>(modifyAuditTask.AsTask);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateAuditAsync(It.IsAny<IAudit>(), It.IsAny<CancellationToken>()),
                    Times.Never);
        }

        [Fact]
        public async Task ShouldModifyAuditWhenTheCreationStampIsUnchangedAsync()
        {
            // given
            DateTimeOffset createdDate = GetRandomDateTimeOffset();
            TestAudit storedAudit = CreateUnstampedAudit();
            storedAudit.CreatedBy = "original.author@nhs.net";
            storedAudit.CreatedDate = createdDate;

            TestAudit inputAudit = CreateUnstampedAudit();
            inputAudit.Id = storedAudit.Id;
            inputAudit.CreatedBy = storedAudit.CreatedBy;
            inputAudit.CreatedDate = storedAudit.CreatedDate;
            inputAudit.Message = GetRandomString();

            this.dateTimeBrokerMock.Setup(broker => broker.GetCurrentDateTimeOffsetAsync())
                .ReturnsAsync(GetRandomDateTimeOffset());

            this.storageBrokerMock.Setup(broker =>
                broker.SelectAuditByIdAsync(storedAudit.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(storedAudit);

            this.storageBrokerMock.Setup(broker =>
                broker.UpdateAuditAsync(inputAudit, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(inputAudit);

            // when
            IAudit actualAudit = await this.auditService.ModifyAuditAsync(
                inputAudit, TestContext.Current.CancellationToken);

            // then
            // The guard has to let a legitimate edit through, or it is just an outage.
            actualAudit.Should().BeSameAs(inputAudit);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateAuditAsync(inputAudit, It.IsAny<CancellationToken>()),
                    Times.Once);
        }
    }
}
