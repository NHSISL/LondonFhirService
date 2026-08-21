// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Clients.AuditAndMetrics.Tests.Unit.Models.Audits;
using LondonFhirService.Core.Abstractions.Models.Audits;
using Moq;

namespace LondonFhirService.Clients.AuditAndMetrics.Tests.Unit.Audits
{
    /// <summary>
    /// Cancellation is not translated into a client exception. A token already cancelled on the
    /// way in stops the call before the service is touched, and cancellation raised by the
    /// service travels out untouched.
    /// </summary>
    public partial class AuditClientTests
    {
        [Fact]
        public async Task ShouldThrowOperationCanceledOnLogAuditIfTokenIsAlreadyCancelledAsync()
        {
            // given
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();
            CancellationToken cancelledToken = cancellationTokenSource.Token;

            // when
            Func<Task> logAudit = async () =>
                await this.auditClient.LogAuditAsync(
                    auditType: GetRandomString(),
                    title: GetRandomString(),
                    message: GetRandomString(),
                    fileName: GetRandomString(),
                    correlationId: GetRandomString(),
                    cancellationToken: cancelledToken);

            // then
            await logAudit.Should().ThrowAsync<OperationCanceledException>();

            this.auditServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowOperationCanceledOnBulkLogAuditsIfTokenIsAlreadyCancelledAsync()
        {
            // given
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();
            CancellationToken cancelledToken = cancellationTokenSource.Token;
            List<IAudit> randomAudits = CreateRandomAudits();

            // when
            Func<Task> bulkLogAudits = async () =>
                await this.auditClient.BulkLogAuditsAsync(randomAudits, cancellationToken: cancelledToken);

            // then
            await bulkLogAudits.Should().ThrowAsync<OperationCanceledException>();

            this.auditServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldNotWrapAnOperationCanceledExceptionRaisedByTheServiceAsync()
        {
            // given
            var operationCanceledException = new OperationCanceledException();

            this.auditServiceMock.Setup(service =>
                service.LogAuditAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                        .ThrowsAsync(operationCanceledException);

            // when
            Func<Task> logAudit = async () =>
                await this.auditClient.LogAuditAsync(
                    auditType: GetRandomString(),
                    title: GetRandomString(),
                    message: GetRandomString(),
                    fileName: GetRandomString(),
                    correlationId: GetRandomString(),
                    cancellationToken: TestContext.Current.CancellationToken);

            // then
            (await logAudit.Should().ThrowAsync<OperationCanceledException>())
                .Which.Should().BeSameAs(operationCanceledException);

            this.auditServiceMock.Verify(service =>
                service.LogAuditAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.auditServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldPassTheCallersTokenThroughToTheServiceAsync()
        {
            // given
            using var cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            List<IAudit> randomAudits = CreateRandomAudits();

            // when
            await this.auditClient.BulkLogAuditsAsync(randomAudits, cancellationToken: cancellationToken);

            // then
            // The exact token, so the client cannot quietly call the service with
            // CancellationToken.None and leave the write uncancellable.
            this.auditServiceMock.Verify(service =>
                service.BulkAddAuditsAsync(randomAudits, It.IsAny<int>(), cancellationToken),
                    Times.Once);

            this.auditServiceMock.VerifyNoOtherCalls();
        }
    }
}
