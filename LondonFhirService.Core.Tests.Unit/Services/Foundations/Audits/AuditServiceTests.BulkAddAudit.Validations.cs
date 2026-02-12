// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.Audits;
using LondonFhirService.Core.Models.Foundations.Audits.Exceptions;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Audits
{
    public partial class AuditServiceTests
    {
        [Fact]
        public async Task ShouldThrowValidationExceptionOnBulkAddIfAuditsIsNullAndLogItAsync()
        {
            // given
            List<Audit> nullAudit = null;
            int randomBatchSize = GetRandomNumber();
            int inputBatchSize = randomBatchSize;

            var nullAuditException =
                new NullAuditServiceException(message: "Audits is null.");

            var expectedAuditValidationException =
                new AuditServiceValidationException(
                    message: "Audit validation errors occurred, please try again.",
                    innerException: nullAuditException);

            // when
            ValueTask bulkAddAuditTask =
                this.auditService.BulkAddAuditsAsync(nullAudit);

            AuditServiceValidationException actualAuditValidationException =
                await Assert.ThrowsAsync<AuditServiceValidationException>(bulkAddAuditTask.AsTask);

            // then
            actualAuditValidationException.Should()
                .BeEquivalentTo(expectedAuditValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedAuditValidationException))),
                        Times.Once);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerFactoryMock.VerifyNoOtherCalls();
        }
    }
}