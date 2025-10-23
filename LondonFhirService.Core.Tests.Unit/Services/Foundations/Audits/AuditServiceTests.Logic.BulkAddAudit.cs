// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.Audits;
using LondonDataServices.IDecide.Core.Services.Foundations.Audits;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Audits
{
    public partial class AuditServiceTests
    {
        [Fact]
        public async Task ShouldBulkAddAuditLogAsync()
        {
            // given
            List<Audit> randomAudits = CreateRandomAudits();
            List<Audit> inputAudits = randomAudits;
            int randomBatchSize = GetRandomNumber();
            int inputBatchSize = randomBatchSize;

            var auditServiceMock = new Mock<AuditService>(
                this.storageBrokerMock.Object,
                this.identifierBrokerMock.Object,
                this.dateTimeBrokerMock.Object,
                this.securityAuditBrokerMock.Object,
                this.loggingBrokerMock.Object)
            { CallBase = true };


            auditServiceMock.Setup(service =>
                service.BatchBulkAddAuditsAsync(randomAudits, randomBatchSize))
                    .Returns(ValueTask.CompletedTask);

            // when
            await auditServiceMock.Object.BulkAddAuditsAsync(inputAudits, inputBatchSize);

            // then
            auditServiceMock.Verify(service =>
                service.BatchBulkAddAuditsAsync(randomAudits, randomBatchSize),
                    Times.Once);

            auditServiceMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
        }
    }
}