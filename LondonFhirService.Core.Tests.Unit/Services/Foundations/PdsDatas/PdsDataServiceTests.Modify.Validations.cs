// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.PdsDatas;
using LondonFhirService.Core.Models.Foundations.PdsDatas.Exceptions;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.PdsDatas
{
    public partial class PdsDataServiceTests
    {
        [Fact]
        public async Task ShouldThrowValidationExceptionOnModifyIfPdsDataIsNullAndLogItAsync()
        {
            // given
            PdsData nullPdsData = null;
            var nullPdsDataException = new NullPdsDataException(message: "PdsData is null.");

            var expectedPdsDataValidationException =
                new PdsDataValidationException(
                    message: "PdsData validation error occurred, please fix errors and try again.",
                    innerException: nullPdsDataException);

            // when
            ValueTask<PdsData> modifyPdsDataTask =
                this.pdsDataService.ModifyPdsDataAsync(nullPdsData);

            PdsDataValidationException actualPdsDataValidationException =
                await Assert.ThrowsAsync<PdsDataValidationException>(
                    testCode: modifyPdsDataTask.AsTask);

            // then
            actualPdsDataValidationException.Should()
                .BeEquivalentTo(expectedPdsDataValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedPdsDataValidationException))),
                        Times.Once);

            this.storageBroker.Verify(broker =>
                broker.UpdatePdsDataAsync(It.IsAny<PdsData>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBroker.VerifyNoOtherCalls();
            this.storageBroker.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ShouldThrowValidationExceptionOnModifyIfPdsDataIsInvalidAndLogItAsync(string invalidText)
        {
            // given 
            var invalidPdsData = new PdsData { NhsNumber = invalidText };

            var invalidPdsDataException =
                new InvalidPdsDataException(
                    message: "Invalid pdsData. Please correct the errors and try again.");

            invalidPdsDataException.AddData(
                key: nameof(PdsData.Id),
                values: "Id is invalid");

            invalidPdsDataException.AddData(
                key: nameof(PdsData.NhsNumber),
                values: "Text is invalid");

            var expectedPdsDataValidationException =
                new PdsDataValidationException(
                    message: "PdsData validation error occurred, please fix errors and try again.",
                    innerException: invalidPdsDataException);

            // when
            ValueTask<PdsData> modifyPdsDataTask =
                this.pdsDataService.ModifyPdsDataAsync(invalidPdsData);

            PdsDataValidationException actualPdsDataValidationException =
                await Assert.ThrowsAsync<PdsDataValidationException>(
                    testCode: modifyPdsDataTask.AsTask);

            //then
            actualPdsDataValidationException.Should()
                .BeEquivalentTo(expectedPdsDataValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedPdsDataValidationException))),
                        Times.Once());

            this.storageBroker.Verify(broker =>
                broker.UpdatePdsDataAsync(It.IsAny<PdsData>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBroker.VerifyNoOtherCalls();
            this.storageBroker.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnModifyIfPdsDataDoesNotExistAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            PdsData randomPdsData = CreateRandomModifyPdsData(randomDateTimeOffset);
            PdsData nonExistPdsData = randomPdsData;
            PdsData nullPdsData = null;

            var notFoundPdsDataException =
                new NotFoundPdsDataException(message: $"PdsData not found with Id: {nonExistPdsData.Id}");

            var expectedPdsDataValidationException =
                new PdsDataValidationException(
                    message: "PdsData validation error occurred, please fix errors and try again.",
                    innerException: notFoundPdsDataException);

            this.storageBroker.Setup(broker =>
                broker.SelectPdsDataByIdAsync(nonExistPdsData.Id))
                .ReturnsAsync(nullPdsData);

            // when 
            ValueTask<PdsData> modifyPdsDataTask =
                this.pdsDataService.ModifyPdsDataAsync(nonExistPdsData);

            PdsDataValidationException actualPdsDataValidationException =
                await Assert.ThrowsAsync<PdsDataValidationException>(
                    testCode: modifyPdsDataTask.AsTask);

            // then
            actualPdsDataValidationException.Should()
                .BeEquivalentTo(expectedPdsDataValidationException);

            this.storageBroker.Verify(broker =>
                broker.SelectPdsDataByIdAsync(nonExistPdsData.Id),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedPdsDataValidationException))),
                        Times.Once);

            this.storageBroker.VerifyNoOtherCalls();
            this.dateTimeBroker.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}