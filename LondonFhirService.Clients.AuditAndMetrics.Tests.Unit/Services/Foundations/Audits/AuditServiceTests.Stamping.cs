// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Clients.AuditAndMetrics.Tests.Unit.Models.Audits;
using LondonFhirService.Core.Abstractions.Models.Audits;
using Moq;

namespace LondonFhirService.Clients.AuditAndMetrics.Tests.Unit.Services.Foundations.Audits
{
    /// <summary>
    /// The write is dispatched to the background, so the only thing that fixes when an entry
    /// happened and who caused it is the stamp applied before dispatch. These tests pin that
    /// down: every path stamps on the caller's thread, and a caller's own stamp is never
    /// overwritten with the write time.
    /// </summary>
    public partial class AuditServiceTests
    {
        [Fact]
        public async Task ShouldStampCreationTimeAndUserBeforeDispatchingTheWriteAsync()
        {
            // given
            DateTimeOffset createdDate = GetRandomDateTimeOffset();
            string currentUserId = GetRandomString();
            Guid auditId = GetRandomGuid();
            var audit = new TestAudit();

            this.auditBrokerMock.Setup(broker => broker.CreateAudit())
                .Returns(audit);

            this.dateTimeBrokerMock.Setup(broker => broker.GetCurrentDateTimeOffsetAsync())
                .ReturnsAsync(createdDate);

            this.auditUserBrokerMock.Setup(broker => broker.GetCurrentUserIdAsync())
                .ReturnsAsync(currentUserId);

            this.identifierBrokerMock.Setup(broker => broker.GetIdentifierAsync())
                .ReturnsAsync(auditId);

            // when
            await this.auditService.LogAuditAsync(
                auditType: GetRandomString(),
                title: GetRandomString(),
                message: GetRandomString(),
                fileName: GetRandomString(),
                correlationId: GetRandomString(),
                cancellationToken: TestContext.Current.CancellationToken);

            // then
            // Asserted on the entity rather than on the write, because the write has not
            // necessarily happened yet - which is the whole point. The stamp is already on it.
            audit.Id.Should().Be(auditId);
            audit.CreatedDate.Should().Be(createdDate);
            audit.UpdatedDate.Should().Be(createdDate);
            audit.CreatedBy.Should().Be(currentUserId);
            audit.UpdatedBy.Should().Be(currentUserId);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.auditUserBrokerMock.Verify(broker =>
                broker.GetCurrentUserIdAsync(),
                    Times.Once);
        }

        [Fact]
        public async Task ShouldStampCreationTimeAndUserOnRecordAuditAsync()
        {
            // given
            DateTimeOffset createdDate = GetRandomDateTimeOffset();
            string currentUserId = GetRandomString();
            var audit = new TestAudit();

            this.auditBrokerMock.Setup(broker => broker.CreateAudit())
                .Returns(audit);

            this.dateTimeBrokerMock.Setup(broker => broker.GetCurrentDateTimeOffsetAsync())
                .ReturnsAsync(createdDate);

            this.auditUserBrokerMock.Setup(broker => broker.GetCurrentUserIdAsync())
                .ReturnsAsync(currentUserId);

            this.identifierBrokerMock.Setup(broker => broker.GetIdentifierAsync())
                .ReturnsAsync(GetRandomGuid());

            // when
            await this.auditService.RecordAuditAsync(
                auditType: GetRandomString(),
                title: GetRandomString(),
                message: GetRandomString(),
                fileName: GetRandomString(),
                correlationId: GetRandomString(),
                cancellationToken: TestContext.Current.CancellationToken);

            // then
            // The awaited path stamps identically. Only the waiting differs.
            this.auditBrokerMock.Verify(broker =>
                broker.InsertAuditAsync(
                    It.Is<IAudit>(inserted =>
                        inserted.CreatedDate == createdDate
                        && inserted.UpdatedDate == createdDate
                        && inserted.CreatedBy == currentUserId
                        && inserted.UpdatedBy == currentUserId),
                    It.IsAny<CancellationToken>()),
                        Times.Once);
        }

        [Fact]
        public async Task ShouldStampTheEntryBeforeInsertingItOnAddAsync()
        {
            // given
            DateTimeOffset createdDate = GetRandomDateTimeOffset();
            string currentUserId = GetRandomString();
            TestAudit audit = CreateUnstampedAudit();

            this.dateTimeBrokerMock.Setup(broker => broker.GetCurrentDateTimeOffsetAsync())
                .ReturnsAsync(createdDate);

            this.auditUserBrokerMock.Setup(broker => broker.GetCurrentUserIdAsync())
                .ReturnsAsync(currentUserId);

            // when
            await this.auditService.AddAuditAsync(audit, TestContext.Current.CancellationToken);

            // then
            audit.CreatedDate.Should().Be(createdDate);
            audit.UpdatedDate.Should().Be(createdDate);
            audit.CreatedBy.Should().Be(currentUserId);
            audit.UpdatedBy.Should().Be(currentUserId);

            this.auditBrokerMock.Verify(broker =>
                broker.InsertAuditAsync(audit, It.IsAny<CancellationToken>()),
                    Times.Once);
        }

        [Fact]
        public async Task ShouldNotOverwriteAStampTheCallerAlreadySetOnAddAsync()
        {
            // given
            DateTimeOffset writeTime = GetRandomDateTimeOffset();
            DateTimeOffset eventTime = writeTime.AddMinutes(-5);
            string callerUserId = GetRandomString();
            TestAudit audit = CreateUnstampedAudit();
            audit.CreatedDate = eventTime;
            audit.UpdatedDate = eventTime;
            audit.CreatedBy = callerUserId;
            audit.UpdatedBy = callerUserId;

            this.dateTimeBrokerMock.Setup(broker => broker.GetCurrentDateTimeOffsetAsync())
                .ReturnsAsync(writeTime);

            this.auditUserBrokerMock.Setup(broker => broker.GetCurrentUserIdAsync())
                .ReturnsAsync(GetRandomString());

            // when
            await this.auditService.AddAuditAsync(audit, TestContext.Current.CancellationToken);

            // then
            // Fill gaps, never overwrite. A caller that recorded when the event happened and who
            // caused it is authoritative; this service only knows when it got round to writing.
            audit.CreatedDate.Should().Be(eventTime);
            audit.CreatedBy.Should().Be(callerUserId);
            audit.UpdatedBy.Should().Be(callerUserId);

            this.auditUserBrokerMock.Verify(broker =>
                broker.GetCurrentUserIdAsync(),
                    Times.Never);
        }

        [Fact]
        public async Task ShouldPreserveEachEntrysOwnStampOnBulkAddAsync()
        {
            // given
            DateTimeOffset writeTime = GetRandomDateTimeOffset();
            string currentUserId = GetRandomString();
            List<IAudit> audits = CreateUnstampedAudits(count: 3);

            // Created at three distinct moments, in order, as a request accumulates them.
            DateTimeOffset firstCreated = writeTime.AddSeconds(-30);
            audits[0].CreatedDate = firstCreated;
            audits[1].CreatedDate = firstCreated.AddSeconds(10);
            audits[2].CreatedDate = firstCreated.AddSeconds(20);

            this.dateTimeBrokerMock.Setup(broker => broker.GetCurrentDateTimeOffsetAsync())
                .ReturnsAsync(writeTime);

            this.auditUserBrokerMock.Setup(broker => broker.GetCurrentUserIdAsync())
                .ReturnsAsync(currentUserId);

            // when
            await this.auditService.BulkAddAuditsAsync(
                audits,
                cancellationToken: TestContext.Current.CancellationToken);

            // then
            // Submitting must not flatten them to one instant. If it did, the order they happened
            // in would be unrecoverable from the stored rows.
            audits[0].CreatedDate.Should().Be(firstCreated);
            audits[1].CreatedDate.Should().Be(firstCreated.AddSeconds(10));
            audits[2].CreatedDate.Should().Be(firstCreated.AddSeconds(20));
            audits.Should().NotContain(audit => audit.CreatedDate == writeTime);

            // Gaps still get filled: none of these carried a user.
            audits.Should().OnlyContain(audit => audit.CreatedBy == currentUserId);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogWarningAsync(It.IsAny<string>()),
                    Times.Never);

            this.auditBrokerMock.Verify(broker =>
                broker.BulkInsertAuditsAsync(It.IsAny<List<IAudit>>(), It.IsAny<CancellationToken>()),
                    Times.Once);
        }

        [Fact]
        public async Task ShouldWarnOnceForTheBatchWhenEntriesArriveUnstampedAsync()
        {
            // given
            DateTimeOffset writeTime = GetRandomDateTimeOffset();
            string currentUserId = GetRandomString();
            List<IAudit> audits = CreateUnstampedAudits(count: 5);
            string unstampedAuditType = GetRandomString();
            string unstampedCorrelationId = GetRandomString();

            // Two of the five were stamped at creation; three arrived with nothing.
            audits[0].CreatedDate = writeTime.AddSeconds(-20);
            audits[1].CreatedDate = writeTime.AddSeconds(-10);

            foreach (IAudit audit in audits.Skip(2))
            {
                audit.AuditType = unstampedAuditType;
                audit.CorrelationId = unstampedCorrelationId;
            }

            this.dateTimeBrokerMock.Setup(broker => broker.GetCurrentDateTimeOffsetAsync())
                .ReturnsAsync(writeTime);

            this.auditUserBrokerMock.Setup(broker => broker.GetCurrentUserIdAsync())
                .ReturnsAsync(currentUserId);

            // when
            await this.auditService.BulkAddAuditsAsync(
                audits,
                cancellationToken: TestContext.Current.CancellationToken);

            // then
            // One line for the batch, naming how many were affected and where they came from -
            // not one per entry, which would bury the signal when a caller stamps nothing at all.
            this.loggingBrokerMock.Verify(broker =>
                broker.LogWarningAsync(It.Is<string>(message =>
                    message.Contains("3 of 5")
                    && message.Contains("without a creation")
                    && message.Contains(unstampedAuditType)
                    && message.Contains(unstampedCorrelationId))),
                        Times.Once);

            // The unstamped three fell back to the write time; the stamped two were left alone.
            audits[0].CreatedDate.Should().Be(writeTime.AddSeconds(-20));
            audits[1].CreatedDate.Should().Be(writeTime.AddSeconds(-10));
            audits[2].CreatedDate.Should().Be(writeTime);
            audits[3].CreatedDate.Should().Be(writeTime);
            audits[4].CreatedDate.Should().Be(writeTime);
        }

        [Fact]
        public async Task ShouldNotWarnWhenEveryEntryInTheBatchArrivesStampedAsync()
        {
            // given
            DateTimeOffset writeTime = GetRandomDateTimeOffset();
            List<IAudit> audits = CreateUnstampedAudits(count: 4);

            for (int index = 0; index < audits.Count; index++)
            {
                audits[index].CreatedDate = writeTime.AddSeconds(-40 + (index * 10));
                audits[index].CreatedBy = GetRandomString();
                audits[index].UpdatedBy = audits[index].CreatedBy;
            }

            this.dateTimeBrokerMock.Setup(broker => broker.GetCurrentDateTimeOffsetAsync())
                .ReturnsAsync(writeTime);

            // when
            await this.auditService.BulkAddAuditsAsync(
                audits,
                cancellationToken: TestContext.Current.CancellationToken);

            // then
            // Nothing to report, so nothing is logged. A warning that fires on a healthy batch is
            // a warning nobody reads.
            this.loggingBrokerMock.Verify(broker =>
                broker.LogWarningAsync(It.IsAny<string>()),
                    Times.Never);

            this.auditUserBrokerMock.Verify(broker =>
                broker.GetCurrentUserIdAsync(),
                    Times.Never);
        }
    }
}
