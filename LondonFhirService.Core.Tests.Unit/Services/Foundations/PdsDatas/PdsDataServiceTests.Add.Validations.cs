// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

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
        public async Task ShouldThrowValidationExceptionOnAddIfPdsDataIsNullAndLogItAsync()
        {
            // given
            PdsData nullPdsData = null;

            var nullPdsDataException =
                new NullPdsDataServiceException(message: "PdsData is null.");

            var expectedPdsDataValidationException =
                new PdsDataServiceValidationException(
                    message: "PdsData validation error occurred, please fix errors and try again.",
                    innerException: nullPdsDataException);

            // when
            ValueTask<PdsData> addPdsDataTask =
                this.pdsDataService.AddPdsDataAsync(nullPdsData);

            PdsDataServiceValidationException actualPdsDataValidationException =
                await Assert.ThrowsAsync<PdsDataServiceValidationException>(
                    testCode: addPdsDataTask.AsTask);

            // then
            actualPdsDataValidationException.Should()
                .BeEquivalentTo(expectedPdsDataValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedPdsDataValidationException))),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBroker.VerifyNoOtherCalls();
            this.storageBroker.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ShouldThrowValidationExceptionOnAddIfPdsDataIsInvalidAndLogItAsync(string invalidText)
        {
            // given
            var invalidPdsData = new PdsData { NhsNumber = invalidText };

            var invalidPdsDataException =
                new InvalidPdsDataServiceException(
                    message: "Invalid pdsData. Please correct the errors and try again.");

            invalidPdsDataException.AddData(
                key: nameof(PdsData.Id),
                values: "Id is invalid");

            invalidPdsDataException.AddData(
                key: nameof(PdsData.NhsNumber),
                values: "Text is invalid");

            var expectedPdsDataValidationException =
                new PdsDataServiceValidationException(
                    message: "PdsData validation error occurred, please fix errors and try again.",
                    innerException: invalidPdsDataException);

            // when
            ValueTask<PdsData> addPdsDataTask =
                this.pdsDataService.AddPdsDataAsync(invalidPdsData);

            PdsDataServiceValidationException actualPdsDataValidationException =
                await Assert.ThrowsAsync<PdsDataServiceValidationException>(
                    testCode: addPdsDataTask.AsTask);

            // then
            actualPdsDataValidationException.Should()
                .BeEquivalentTo(expectedPdsDataValidationException);

            this.storageBroker.Verify(broker =>
                broker.InsertPdsDataAsync(It.IsAny<PdsData>()),
                    Times.Never);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedPdsDataValidationException))),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBroker.VerifyNoOtherCalls();
            this.storageBroker.VerifyNoOtherCalls();
        }
    }
}