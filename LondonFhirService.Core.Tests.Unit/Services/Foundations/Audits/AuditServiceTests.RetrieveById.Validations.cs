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
        public async Task ShouldThrowValidationExceptionOnRetrieveByIdIfIdIsInvalidAndLogItAsync()
        {
            // given
            var invalidAuditId = Guid.Empty;

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
            ValueTask<Audit> retrieveAuditByIdTask =
                this.auditService.RetrieveAuditByIdAsync(invalidAuditId);

            AuditServiceValidationException actualAuditValidationException =
                await Assert.ThrowsAsync<AuditServiceValidationException>(
                    retrieveAuditByIdTask.AsTask);

            // then
            actualAuditValidationException.Should()
                .BeEquivalentTo(expectedAuditValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedAuditValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAuditByIdAsync(It.IsAny<Guid>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowNotFoundExceptionOnRetrieveByIdIfAuditIsNotFoundAndLogItAsync()
        {
            //given
            Guid someAuditId = Guid.NewGuid();
            Audit noAudit = null;

            var notFoundAuditException =
                new NotFoundAuditServiceException($"Couldn't find audit with auditId: {someAuditId}.");

            var expectedAuditValidationException =
                new AuditServiceValidationException(
                    message: "Audit validation errors occurred, please try again.",
                    innerException: notFoundAuditException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectAuditByIdAsync(It.IsAny<Guid>()))
                    .ReturnsAsync(noAudit);

            //when
            ValueTask<Audit> retrieveAuditByIdTask =
                this.auditService.RetrieveAuditByIdAsync(someAuditId);

            AuditServiceValidationException actualAuditValidationException =
                await Assert.ThrowsAsync<AuditServiceValidationException>(
                    retrieveAuditByIdTask.AsTask);

            //then
            actualAuditValidationException.Should().BeEquivalentTo(expectedAuditValidationException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAuditByIdAsync(It.IsAny<Guid>()),
                    Times.Once());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedAuditValidationException))),
                        Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
        }
    }
}