// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.Audits;
using Moq;
using IAudit = LondonFhirService.Core.Abstractions.Models.Audits.IAudit;

namespace LondonFhirService.Core.Tests.Unit.Brokers.AuditAndMetrics
{
    /// <summary>
    /// The storage port accepts any IAudit, because the library that calls it holds no concrete
    /// implementation and must not need one. Storage only knows the Core entity, so anything else
    /// has to be copied onto one - a cast would throw on a call the contract says is legal.
    ///
    /// Every audit write in the application passes through this mapping, so a dropped field here
    /// is a column that silently stops being written.
    /// </summary>
    public partial class AuditStorageBrokerTests
    {
        [Fact]
        public async Task ShouldCopyEveryAuditFieldWhenTheContractIsNotTheEntityAsync()
        {
            // given
            var foreignAudit = new ForeignAudit
            {
                Id = Guid.NewGuid(),
                CorrelationId = GetRandomString(),
                AuditType = GetRandomString(),
                Title = GetRandomString(),
                Message = GetRandomString(),
                FileName = GetRandomString(),
                LogLevel = GetRandomString(),
                CreatedBy = GetRandomString(),
                CreatedDate = GetRandomDateTimeOffset(),
                UpdatedBy = GetRandomString(),
                UpdatedDate = GetRandomDateTimeOffset()
            };

            Audit capturedAudit = null;

            this.storageBrokerMock.Setup(broker =>
                broker.InsertAuditAsync(It.IsAny<IAudit>(), It.IsAny<CancellationToken>()))
                    .Callback<IAudit, CancellationToken>((audit, _) => capturedAudit = audit as Audit)
                    .ReturnsAsync(foreignAudit);

            // when
            await this.auditStorageBroker.InsertAuditAsync(
                foreignAudit, TestContext.Current.CancellationToken);

            // then
            // Field for field, because a mapping that quietly drops one produces a column that
            // stops being written with nothing failing anywhere.
            capturedAudit.Should().NotBeNull();
            capturedAudit.Should().BeOfType<Audit>();
            capturedAudit.Id.Should().Be(foreignAudit.Id);
            capturedAudit.CorrelationId.Should().Be(foreignAudit.CorrelationId);
            capturedAudit.AuditType.Should().Be(foreignAudit.AuditType);
            capturedAudit.Title.Should().Be(foreignAudit.Title);
            capturedAudit.Message.Should().Be(foreignAudit.Message);
            capturedAudit.FileName.Should().Be(foreignAudit.FileName);
            capturedAudit.LogLevel.Should().Be(foreignAudit.LogLevel);
            capturedAudit.CreatedBy.Should().Be(foreignAudit.CreatedBy);
            capturedAudit.CreatedDate.Should().Be(foreignAudit.CreatedDate);
            capturedAudit.UpdatedBy.Should().Be(foreignAudit.UpdatedBy);
            capturedAudit.UpdatedDate.Should().Be(foreignAudit.UpdatedDate);
        }

        [Fact]
        public async Task ShouldPassTheSameAuditInstanceThroughWhenItIsAlreadyTheEntityAsync()
        {
            // given
            var audit = new Audit { Id = Guid.NewGuid(), AuditType = GetRandomString() };
            Audit capturedAudit = null;

            this.storageBrokerMock.Setup(broker =>
                broker.InsertAuditAsync(It.IsAny<IAudit>(), It.IsAny<CancellationToken>()))
                    .Callback<IAudit, CancellationToken>((inserted, _) => capturedAudit = inserted as Audit)
                    .ReturnsAsync(audit);

            // when
            await this.auditStorageBroker.InsertAuditAsync(
                audit, TestContext.Current.CancellationToken);

            // then
            // Not a copy. EF tracks the instance it was handed, so copying one that is already
            // the entity would hand back a different object than the caller holds.
            capturedAudit.Should().BeSameAs(audit);
        }

        [Fact]
        public async Task ShouldMapEveryAuditInABulkWriteAsync()
        {
            // given
            List<IAudit> audits = new List<IAudit>
            {
                new ForeignAudit { Id = Guid.NewGuid(), AuditType = GetRandomString() },
                new Audit { Id = Guid.NewGuid(), AuditType = GetRandomString() },
                new ForeignAudit { Id = Guid.NewGuid(), AuditType = GetRandomString() }
            };

            List<IAudit> capturedAudits = null;

            this.storageBrokerMock.Setup(broker =>
                broker.BulkInsertAuditsAsync(It.IsAny<List<IAudit>>(), It.IsAny<CancellationToken>()))
                    .Callback<List<IAudit>, CancellationToken>((batch, _) => capturedAudits = batch);

            // when
            await this.auditStorageBroker.BulkInsertAuditsAsync(
                audits, TestContext.Current.CancellationToken);

            // then
            // The broker casts the whole batch, so one foreign entry left unmapped would take the
            // entire write down rather than just itself.
            capturedAudits.Should().HaveCount(3);
            capturedAudits.Should().AllBeOfType<Audit>();

            capturedAudits.Select(audit => audit.Id)
                .Should().Equal(audits.Select(audit => audit.Id));
        }

        /// <summary>
        /// An IAudit that is not Core's Audit. Exactly what the port promises to accept and what
        /// the old cast would have thrown on.
        /// </summary>
        private class ForeignAudit : IAudit
        {
            public Guid Id { get; set; }
            public string CorrelationId { get; set; }
            public string AuditType { get; set; }
            public string Title { get; set; }
            public string Message { get; set; }
            public string FileName { get; set; }
            public string LogLevel { get; set; }
            public string CreatedBy { get; set; }
            public DateTimeOffset CreatedDate { get; set; }
            public string UpdatedBy { get; set; }
            public DateTimeOffset UpdatedDate { get; set; }
        }
    }
}
