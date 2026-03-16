// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.FhirRecordDifferences;
using LondonFhirService.Core.Models.Foundations.FhirRecordDifferences.Exceptions;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.FhirRecordDifferences
{
    public partial class FhirRecordDifferenceServiceTests
    {
        [Fact]
        public async Task ShouldThrowValidationExceptionOnRetrieveByIdIfIdIsInvalidAndLogItAsync()
        {
            // given
            var invalidFhirRecordDifferenceId = Guid.Empty;

            var invalidFhirRecordDifferenceException =
                new InvalidFhirRecordDifferenceException(
                    message: "Invalid fhirRecordDifference. Please correct the errors and try again.");

            invalidFhirRecordDifferenceException.AddData(
                key: nameof(FhirRecordDifference.Id),
                values: "Id is required");

            var expectedFhirRecordDifferenceValidationException =
                new FhirRecordDifferenceValidationException(
                    message: "FhirRecordDifference validation errors occurred, please try again.",
                    innerException: invalidFhirRecordDifferenceException);

            // when
            ValueTask<FhirRecordDifference> retrieveFhirRecordDifferenceByIdTask =
                this.fhirRecordDifferenceService.RetrieveFhirRecordDifferenceByIdAsync(invalidFhirRecordDifferenceId);

            FhirRecordDifferenceValidationException actualFhirRecordDifferenceValidationException =
                await Assert.ThrowsAsync<FhirRecordDifferenceValidationException>(
                    retrieveFhirRecordDifferenceByIdTask.AsTask);

            // then
            actualFhirRecordDifferenceValidationException.Should()
                .BeEquivalentTo(expectedFhirRecordDifferenceValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedFhirRecordDifferenceValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectFhirRecordDifferenceByIdAsync(It.IsAny<Guid>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowNotFoundExceptionOnRetrieveByIdIfFhirRecordDifferenceIsNotFoundAndLogItAsync()
        {
            //given
            Guid someFhirRecordDifferenceId = Guid.NewGuid();
            FhirRecordDifference noFhirRecordDifference = null;

            var notFoundFhirRecordDifferenceException = new NotFoundFhirRecordDifferenceException(
                $"Couldn't find fhirRecordDifference with Id: {someFhirRecordDifferenceId}.");

            var expectedFhirRecordDifferenceValidationException =
                new FhirRecordDifferenceValidationException(
                    message: "FhirRecordDifference validation errors occurred, please try again.",
                    innerException: notFoundFhirRecordDifferenceException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectFhirRecordDifferenceByIdAsync(It.IsAny<Guid>()))
                    .ReturnsAsync(noFhirRecordDifference);

            //when
            ValueTask<FhirRecordDifference> retrieveFhirRecordDifferenceByIdTask =
                this.fhirRecordDifferenceService.RetrieveFhirRecordDifferenceByIdAsync(someFhirRecordDifferenceId);

            FhirRecordDifferenceValidationException actualFhirRecordDifferenceValidationException =
                await Assert.ThrowsAsync<FhirRecordDifferenceValidationException>(
                    retrieveFhirRecordDifferenceByIdTask.AsTask);

            //then
            actualFhirRecordDifferenceValidationException.Should().BeEquivalentTo(expectedFhirRecordDifferenceValidationException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectFhirRecordDifferenceByIdAsync(It.IsAny<Guid>()),
                    Times.Once());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedFhirRecordDifferenceValidationException))),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }
    }
}