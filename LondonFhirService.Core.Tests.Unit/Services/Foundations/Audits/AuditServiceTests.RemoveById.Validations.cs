// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
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
        public async Task ShouldThrowValidationExceptionOnRemoveIfIdIsInvalidAndLogItAsync()
        {
            // given
            Guid invalidAuditId = Guid.Empty;

            var invalidAuditException =
                new InvalidAuditServiceException(
                    message: "Invalid audit. Please correct the errors and try again.");

            invalidAuditException.AddData(
                key: nameof(Audit.Id),
                values: "Id is required");

            var expectedAuditValidationException =
                new AuditServiceValidationException(
                    message: "Audit validation errors occurred, please try again.",
                    innerException: invalidAuditException);

            // when
            ValueTask<Audit> removeAuditByIdTask =
                this.auditService.RemoveAuditByIdAsync(invalidAuditId);

            AuditServiceValidationException actualAuditValidationException =
                await Assert.ThrowsAsync<AuditServiceValidationException>(
                    removeAuditByIdTask.AsTask);

            // then
            actualAuditValidationException.Should()
                .BeEquivalentTo(expectedAuditValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedAuditValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.DeleteAuditAsync(It.IsAny<Audit>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
        }
    }
}