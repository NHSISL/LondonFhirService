// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.OdsDatas;
using LondonFhirService.Core.Models.Foundations.OdsDatas.Exceptions;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.OdsDatas
{
    public partial class OdsDataServiceTests
    {
        [Fact]
        public async Task ShouldThrowValidationExceptionOnAddIfOdsDataIsNullAndLogItAsync()
        {
            // given
            OdsData nullOdsData = null;

            var nullOdsDataException =
                new NullOdsDataException(message: "OdsData is null.");

            var expectedOdsDataValidationException =
                new OdsDataValidationException(
                    message: "OdsData validation error occurred, please fix errors and try again.",
                    innerException: nullOdsDataException);

            // when
            ValueTask<OdsData> addOdsDataTask =
                this.odsDataService.AddOdsDataAsync(nullOdsData);

            OdsDataValidationException actualOdsDataValidationException =
                await Assert.ThrowsAsync<OdsDataValidationException>(
                    testCode: addOdsDataTask.AsTask);

            // then
            actualOdsDataValidationException.Should()
                .BeEquivalentTo(expectedOdsDataValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedOdsDataValidationException))),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBroker.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ShouldThrowValidationExceptionOnAddIfOdsDataIsInvalidAndLogItAsync(string invalidText)
        {
            // given
            var invalidOdsData = new OdsData
            {
                OrganisationCode = invalidText
            };

            var invalidOdsDataException =
                new InvalidOdsDataException(
                    message: "Invalid odsData. Please correct the errors and try again.");

            invalidOdsDataException.AddData(
                key: nameof(OdsData.Id),
                values: "Id is invalid");

            invalidOdsDataException.AddData(
                key: nameof(OdsData.OrganisationCode),
                values: "Text is invalid");

            var expectedOdsDataValidationException =
                new OdsDataValidationException(
                    message: "OdsData validation error occurred, please fix errors and try again.",
                    innerException: invalidOdsDataException);

            // when
            ValueTask<OdsData> addOdsDataTask =
                this.odsDataService.AddOdsDataAsync(invalidOdsData);

            OdsDataValidationException actualOdsDataValidationException =
                await Assert.ThrowsAsync<OdsDataValidationException>(
                    testCode: addOdsDataTask.AsTask);

            // then
            actualOdsDataValidationException.Should()
                .BeEquivalentTo(expectedOdsDataValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedOdsDataValidationException))),
                        Times.Once);

            this.storageBroker.Verify(broker =>
                broker.InsertOdsDataAsync(It.IsAny<OdsData>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBroker.VerifyNoOtherCalls();
        }
    }
}