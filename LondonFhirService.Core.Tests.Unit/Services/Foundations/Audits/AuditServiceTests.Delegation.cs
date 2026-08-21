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

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Audits
{
    public partial class AuditServiceTests
    {
        [Fact]
        public async Task ShouldBuildTheEntryAndAddItOnAddAsync()
        {
            // given
            string auditType = GetRandomString();
            string title = GetRandomString();
            string message = GetRandomString();
            string fileName = GetRandomString();
            string correlationId = GetRandomString();
            string logLevel = GetRandomString();
            Audit expectedAudit = CreateRandomAudit();

            this.auditAndMetricBrokerMock.Setup(broker =>
                broker.AddAuditAsync(It.IsAny<Audit>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expectedAudit);

            // when
            Audit actualAudit = await this.auditService.AddAuditAsync(
                auditType, title, message, fileName, correlationId, logLevel,
                TestContext.Current.CancellationToken);

            // then
            actualAudit.Should().BeSameAs(expectedAudit);

            // The service fills only what the caller passed. CreatedDate, CreatedBy and the Id
            // are stamped inside the library, so leaving them unset here is correct rather than
            // an omission.
            this.auditAndMetricBrokerMock.Verify(broker =>
                broker.AddAuditAsync(
                    It.Is<Audit>(audit =>
                        audit.AuditType == auditType
                        && audit.Title == title
                        && audit.Message == message
                        && audit.FileName == fileName
                        && audit.CorrelationId == correlationId
                        && audit.LogLevel == logLevel),
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldAddAnEntityTheCallerSuppliedOnAddAsync()
        {
            // given
            Audit randomAudit = CreateRandomAudit();

            this.auditAndMetricBrokerMock.Setup(broker =>
                broker.AddAuditAsync(randomAudit, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(randomAudit);

            // when
            Audit actualAudit = await this.auditService.AddAuditAsync(
                randomAudit, TestContext.Current.CancellationToken);

            // then
            actualAudit.Should().BeSameAs(randomAudit);

            this.auditAndMetricBrokerMock.Verify(broker =>
                broker.AddAuditAsync(randomAudit, It.IsAny<CancellationToken>()),
                    Times.Once);

            // Stamped before it reaches storage, so a request body cannot claim to have been
            // created by somebody else.
            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(randomAudit),
                    Times.Once);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldPassTheBatchSizeAndTokenThroughOnBulkAddAsync()
        {
            // given
            List<Audit> randomAudits = CreateRandomAudits();
            int batchSize = GetRandomNumber();
            using var cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            // when
            await this.auditService.BulkAddAuditsAsync(randomAudits, batchSize, cancellationToken);

            // then
            // The exact token, so the service cannot quietly call with CancellationToken.None and
            // leave the write uncancellable.
            this.auditAndMetricBrokerMock.Verify(broker =>
                broker.BulkLogAuditsAsync(randomAudits, batchSize, cancellationToken),
                    Times.Once);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldRetrieveAllAuditsAsync()
        {
            // given
            IQueryable<Audit> expectedAudits = CreateRandomAuditsQueryable();

            this.auditAndMetricBrokerMock.Setup(broker =>
                broker.RetrieveAllAuditsAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expectedAudits);

            // when
            IQueryable<Audit> actualAudits =
                await this.auditService.RetrieveAllAuditsAsync(TestContext.Current.CancellationToken);

            // then
            actualAudits.Should().BeSameAs(expectedAudits);

            this.auditAndMetricBrokerMock.Verify(broker =>
                broker.RetrieveAllAuditsAsync(It.IsAny<CancellationToken>()),
                    Times.Once);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldRetrieveAuditByIdAsync()
        {
            // given
            Audit expectedAudit = CreateRandomAudit();

            this.auditAndMetricBrokerMock.Setup(broker =>
                broker.RetrieveAuditByIdAsync(expectedAudit.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expectedAudit);

            // when
            Audit actualAudit = await this.auditService.RetrieveAuditByIdAsync(
                expectedAudit.Id, TestContext.Current.CancellationToken);

            // then
            actualAudit.Should().BeSameAs(expectedAudit);

            this.auditAndMetricBrokerMock.Verify(broker =>
                broker.RetrieveAuditByIdAsync(expectedAudit.Id, It.IsAny<CancellationToken>()),
                    Times.Once);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldModifyAuditAsync()
        {
            // given
            Audit randomAudit = CreateRandomAudit();

            this.auditAndMetricBrokerMock.Setup(broker =>
                broker.ModifyAuditAsync(randomAudit, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(randomAudit);

            // when
            Audit actualAudit = await this.auditService.ModifyAuditAsync(
                randomAudit, TestContext.Current.CancellationToken);

            // then
            actualAudit.Should().BeSameAs(randomAudit);

            this.auditAndMetricBrokerMock.Verify(broker =>
                broker.ModifyAuditAsync(randomAudit, It.IsAny<CancellationToken>()),
                    Times.Once);

            // UpdatedBy and UpdatedDate come from the principal, never the request body.
            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(randomAudit),
                    Times.Once);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldRemoveAuditByIdAsync()
        {
            // given
            Audit expectedAudit = CreateRandomAudit();

            this.auditAndMetricBrokerMock.Setup(broker =>
                broker.RemoveAuditByIdAsync(expectedAudit.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expectedAudit);

            // when
            Audit actualAudit = await this.auditService.RemoveAuditByIdAsync(
                expectedAudit.Id, TestContext.Current.CancellationToken);

            // then
            actualAudit.Should().BeSameAs(expectedAudit);

            this.auditAndMetricBrokerMock.Verify(broker =>
                broker.RemoveAuditByIdAsync(expectedAudit.Id, It.IsAny<CancellationToken>()),
                    Times.Once);

            VerifyNoOtherCallsOnAllBrokers();
        }
    }
}
