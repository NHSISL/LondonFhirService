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
using Xeptions;

namespace LondonFhirService.Clients.AuditAndMetrics.Tests.Unit.Audits
{
    public partial class AuditClientTests
    {
        [Theory]
        [MemberData(nameof(ServiceExceptionMappings))]
        public async Task ShouldMapServiceExceptionOnLogAuditAsync(
            Xeption serviceException,
            Xeption expectedClientException)
        {
            // given
            this.auditServiceMock.Setup(service =>
                service.LogAuditAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                        .ThrowsAsync(serviceException);

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
            await AssertMappedAsync(logAudit, expectedClientException);
        }

        [Theory]
        [MemberData(nameof(ServiceExceptionMappings))]
        public async Task ShouldMapServiceExceptionOnBulkLogAuditsAsync(
            Xeption serviceException,
            Xeption expectedClientException)
        {
            // given
            List<IAudit> randomAudits = CreateRandomAudits();

            this.auditServiceMock.Setup(service =>
                service.BulkLogAuditsAsync(
                    It.IsAny<List<IAudit>>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                        .ThrowsAsync(serviceException);

            // when
            Func<Task> bulkLogAudits = async () =>
                await this.auditClient.BulkLogAuditsAsync(
                    randomAudits,
                    cancellationToken: TestContext.Current.CancellationToken);

            // then
            await AssertMappedAsync(bulkLogAudits, expectedClientException);
        }

        /// <summary>
        /// The client exception carries the service exception's inner exception, not the service
        /// exception itself, so callers see the original cause rather than a layer of plumbing.
        /// </summary>
        private static async Task AssertMappedAsync(Func<Task> act, Xeption expectedClientException)
        {
            Xeption actualException = (await act.Should().ThrowAsync<Xeption>()).Which;
            actualException.Should().BeOfType(expectedClientException.GetType());
            actualException.Message.Should().Be(expectedClientException.Message);
            actualException.InnerException.Should().BeSameAs(expectedClientException.InnerException);
        }
    }
}
