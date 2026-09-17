// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Abstractions.Models.Audits;
using Moq;

namespace LondonFhirService.Clients.AuditAndMetrics.Tests.Unit.Services.Foundations.Audits
{
    /// <summary>
    /// The audit kill switch. It is separate from the metric one because the two are not the
    /// same obligation, so these pin that turning audit recording off stops every write path
    /// without touching storage - and that reads still work, which is what makes an environment
    /// able to stop recording and still investigate what was already recorded.
    /// </summary>
    public partial class AuditServiceTests
    {
        [Fact]
        public async Task ShouldNotLogAuditFromDetailsIfAuditingIsDisabledAsync()
        {
            // given
            this.auditServiceConfigurations.IsAuditEnabled = false;

            // when
            await this.auditService.LogAuditAsync(
                auditType: GetRandomString(),
                title: GetRandomString(),
                message: GetRandomString(),
                fileName: GetRandomString(),
                correlationId: GetRandomString(),
                cancellationToken: TestContext.Current.CancellationToken);

            // then
            this.storageBrokerMock.Verify(broker =>
                broker.InsertAuditAsync(It.IsAny<IAudit>(), It.IsAny<CancellationToken>()),
                    Times.Never);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never);

            this.dispatcherMock.Verify(dispatcher =>
                dispatcher.TryDispatch(It.IsAny<Func<CancellationToken, ValueTask>>()),
                    Times.Never);

            VerifyNoOtherCallsOnAllDependencies();
        }

        [Fact]
        public async Task ShouldReturnTheUnwrittenEntryOnRecordAuditIfAuditingIsDisabledAsync()
        {
            // given
            string auditType = GetRandomString();
            string title = GetRandomString();

            // Stubbed as they are for the enabled path. The gate here sits after the build, so
            // an entry is still assembled and validated - disabling auditing stops the write,
            // not the reporting of a caller that passed something unwritable.
            this.dateTimeBrokerMock.Setup(broker => broker.GetCurrentDateTimeOffsetAsync())
                .ReturnsAsync(GetRandomDateTimeOffset());

            this.auditUserBrokerMock.Setup(broker => broker.GetCurrentUserIdAsync())
                .ReturnsAsync(GetRandomString());

            this.identifierBrokerMock.Setup(broker => broker.GetIdentifierAsync())
                .ReturnsAsync(GetRandomGuid());

            this.auditServiceConfigurations.IsAuditEnabled = false;

            // when
            IAudit actualAudit = await this.auditService.RecordAuditAsync(
                auditType: auditType,
                title: title,
                message: GetRandomString(),
                fileName: GetRandomString(),
                correlationId: GetRandomString(),
                cancellationToken: TestContext.Current.CancellationToken);

            // then
            // Awaited for the entry it returns, so it still hands one back - it just never
            // reaches storage.
            actualAudit.Should().NotBeNull();
            actualAudit.AuditType.Should().Be(auditType);
            actualAudit.Title.Should().Be(title);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertAuditAsync(It.IsAny<IAudit>(), It.IsAny<CancellationToken>()),
                    Times.Never);
        }

        [Fact]
        public async Task ShouldNotAddAuditIfAuditingIsDisabledAsync()
        {
            // given
            IAudit inputAudit = CreateUnstampedAudit();
            this.auditServiceConfigurations.IsAuditEnabled = false;

            // when
            IAudit actualAudit = await this.auditService.AddAuditAsync(
                inputAudit,
                TestContext.Current.CancellationToken);

            // then
            actualAudit.Should().BeSameAs(inputAudit);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertAuditAsync(It.IsAny<IAudit>(), It.IsAny<CancellationToken>()),
                    Times.Never);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never);

            VerifyNoOtherCallsOnAllDependencies();
        }

        [Fact]
        public async Task ShouldNotLogAuditIfAuditingIsDisabledAsync()
        {
            // given
            IAudit inputAudit = CreateUnstampedAudit();
            this.auditServiceConfigurations.IsAuditEnabled = false;

            // when
            await this.auditService.LogAuditAsync(inputAudit, TestContext.Current.CancellationToken);

            // then
            this.storageBrokerMock.Verify(broker =>
                broker.InsertAuditAsync(It.IsAny<IAudit>(), It.IsAny<CancellationToken>()),
                    Times.Never);

            this.dispatcherMock.Verify(dispatcher =>
                dispatcher.TryDispatch(It.IsAny<Func<CancellationToken, ValueTask>>()),
                    Times.Never);

            VerifyNoOtherCallsOnAllDependencies();
        }

        [Fact]
        public async Task ShouldNotBulkLogAuditsIfAuditingIsDisabledAsync()
        {
            // given
            List<IAudit> inputAudits = CreateUnstampedAudits(count: GetRandomNumber());
            this.auditServiceConfigurations.IsAuditEnabled = false;

            // when
            await this.auditService.BulkLogAuditsAsync(
                inputAudits,
                cancellationToken: TestContext.Current.CancellationToken);

            // then
            this.storageBrokerMock.Verify(broker =>
                broker.BulkInsertAuditsAsync(It.IsAny<List<IAudit>>(), It.IsAny<CancellationToken>()),
                    Times.Never);

            this.dispatcherMock.Verify(dispatcher =>
                dispatcher.TryDispatch(It.IsAny<Func<CancellationToken, ValueTask>>()),
                    Times.Never);

            VerifyNoOtherCallsOnAllDependencies();
        }

        [Fact]
        public async Task ShouldNotBulkAddAuditsIfAuditingIsDisabledAsync()
        {
            // given
            List<IAudit> inputAudits = CreateUnstampedAudits(count: GetRandomNumber());
            this.auditServiceConfigurations.IsAuditEnabled = false;

            // when
            await this.auditService.BulkAddAuditsAsync(
                inputAudits,
                cancellationToken: TestContext.Current.CancellationToken);

            // then
            this.storageBrokerMock.Verify(broker =>
                broker.BulkInsertAuditsAsync(It.IsAny<List<IAudit>>(), It.IsAny<CancellationToken>()),
                    Times.Never);

            VerifyNoOtherCallsOnAllDependencies();
        }

        [Fact]
        public async Task ShouldStillRetrieveAuditsWhenAuditingIsDisabledAsync()
        {
            // given
            // Recording off is not reading off. An environment that has stopped writing must
            // still be able to see what it already wrote.
            IQueryable<IAudit> storageAudits =
                CreateUnstampedAudits(count: GetRandomNumber()).AsQueryable();

            this.auditServiceConfigurations.IsAuditEnabled = false;

            this.storageBrokerMock.Setup(broker =>
                broker.SelectAllAuditsAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(storageAudits);

            // when
            IQueryable<IAudit> actualAudits = await this.auditService.RetrieveAllAuditsAsync(
                TestContext.Current.CancellationToken);

            // then
            actualAudits.Should().BeSameAs(storageAudits);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAllAuditsAsync(It.IsAny<CancellationToken>()),
                    Times.Once);

            VerifyNoOtherCallsOnAllDependencies();
        }

        /// <summary>
        /// The kill switch is about what does not happen, so every one of these asserts against
        /// the whole set of dependencies rather than the one broker the path would have used.
        /// The storage broker's CreateAudit is excused: the harness stubs it for every test, and
        /// a disabled path never calls it.
        /// </summary>
        private void VerifyNoOtherCallsOnAllDependencies()
        {
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.auditUserBrokerMock.VerifyNoOtherCalls();
            this.dispatcherMock.VerifyNoOtherCalls();
        }
    }
}
