// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.FhirRecords;
using LondonFhirService.Core.Models.Foundations.FhirRecords.Exceptions;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.FhirRecords
{
    public partial class FhirRecordServiceTests
    {
        [Fact]
        public async Task ShouldThrowValidationExceptionOnRetrieveByIdIfIdIsInvalidAndLogItAsync()
        {
            // given
            var invalidFhirRecordId = Guid.Empty;

            var invalidFhirRecordException =
                new InvalidFhirRecordException(
                    message: "Invalid fhirRecord. Please correct the errors and try again.");

            invalidFhirRecordException.AddData(
                key: nameof(FhirRecord.Id),
                values: "Id is required");

            var expectedFhirRecordValidationException =
                new FhirRecordValidationException(
                    message: "FhirRecord validation errors occurred, please try again.",
                    innerException: invalidFhirRecordException);

            // when
            ValueTask<FhirRecord> retrieveFhirRecordByIdTask =
                this.fhirRecordService.RetrieveFhirRecordByIdAsync(invalidFhirRecordId);

            FhirRecordValidationException actualFhirRecordValidationException =
                await Assert.ThrowsAsync<FhirRecordValidationException>(
                    retrieveFhirRecordByIdTask.AsTask);

            // then
            actualFhirRecordValidationException.Should()
                .BeEquivalentTo(expectedFhirRecordValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedFhirRecordValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectFhirRecordByIdAsync(It.IsAny<Guid>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowNotFoundExceptionOnRetrieveByIdIfFhirRecordIsNotFoundAndLogItAsync()
        {
            //given
            Guid someFhirRecordId = Guid.NewGuid();
            FhirRecord noFhirRecord = null;

            var notFoundFhirRecordException = new NotFoundFhirRecordException(
                $"Couldn't find fhirRecord with fhirRecordId: {someFhirRecordId}.");

            var expectedFhirRecordValidationException =
                new FhirRecordValidationException(
                    message: "FhirRecord validation errors occurred, please try again.",
                    innerException: notFoundFhirRecordException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectFhirRecordByIdAsync(It.IsAny<Guid>()))
                    .ReturnsAsync(noFhirRecord);

            //when
            ValueTask<FhirRecord> retrieveFhirRecordByIdTask =
                this.fhirRecordService.RetrieveFhirRecordByIdAsync(someFhirRecordId);

            FhirRecordValidationException actualFhirRecordValidationException =
                await Assert.ThrowsAsync<FhirRecordValidationException>(
                    retrieveFhirRecordByIdTask.AsTask);

            //then
            actualFhirRecordValidationException.Should().BeEquivalentTo(expectedFhirRecordValidationException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectFhirRecordByIdAsync(It.IsAny<Guid>()),
                    Times.Once());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedFhirRecordValidationException))),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }
    }
}