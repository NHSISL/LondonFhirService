// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
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
        public async Task ShouldThrowValidationExceptionOnModifyIfOdsDataIsNullAndLogItAsync()
        {
            // given
            OdsData nullOdsData = null;
            var nullOdsDataException = new NullOdsDataServiceException(message: "OdsData is null.");

            var expectedOdsDataValidationException =
                new OdsDataServiceValidationException(
                    message: "OdsData validation error occurred, please fix errors and try again.",
                    innerException: nullOdsDataException);

            // when
            ValueTask<OdsData> modifyOdsDataTask =
                this.odsDataService.ModifyOdsDataAsync(nullOdsData);

            OdsDataServiceValidationException actualOdsDataValidationException =
                await Assert.ThrowsAsync<OdsDataServiceValidationException>(
                    testCode: modifyOdsDataTask.AsTask);

            // then
            actualOdsDataValidationException.Should()
                .BeEquivalentTo(expectedOdsDataValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedOdsDataValidationException))),
                        Times.Once);

            this.storageBroker.Verify(broker =>
                broker.UpdateOdsDataAsync(It.IsAny<OdsData>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBroker.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ShouldThrowValidationExceptionOnModifyIfOdsDataIsInvalidAndLogItAsync(string invalidText)
        {
            // given 
            var invalidOdsData = new OdsData
            {
                OrganisationCode = invalidText
            };

            var invalidOdsDataException =
                new InvalidOdsDataServiceException(
                    message: "Invalid odsData. Please correct the errors and try again.");

            invalidOdsDataException.AddData(
                key: nameof(OdsData.Id),
                values: "Id is invalid");

            invalidOdsDataException.AddData(
                key: nameof(OdsData.OrganisationCode),
                values: "Text is invalid");

            var expectedOdsDataValidationException =
                new OdsDataServiceValidationException(
                    message: "OdsData validation error occurred, please fix errors and try again.",
                    innerException: invalidOdsDataException);

            // when
            ValueTask<OdsData> modifyOdsDataTask =
                this.odsDataService.ModifyOdsDataAsync(invalidOdsData);

            OdsDataServiceValidationException actualOdsDataValidationException =
                await Assert.ThrowsAsync<OdsDataServiceValidationException>(
                    testCode: modifyOdsDataTask.AsTask);

            //then
            actualOdsDataValidationException.Should()
                .BeEquivalentTo(expectedOdsDataValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedOdsDataValidationException))),
                        Times.Once());

            this.storageBroker.Verify(broker =>
                broker.UpdateOdsDataAsync(It.IsAny<OdsData>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBroker.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnModifyIfOdsDataDoesNotExistAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            OdsData randomOdsData = CreateRandomModifyOdsData(randomDateTimeOffset);
            OdsData nonExistOdsData = randomOdsData;
            OdsData nullOdsData = null;

            var notFoundOdsDataException =
                new NotFoundOdsDataServiceException(message: $"OdsData not found with Id: {nonExistOdsData.Id}");

            var expectedOdsDataValidationException =
                new OdsDataServiceValidationException(
                    message: "OdsData validation error occurred, please fix errors and try again.",
                    innerException: notFoundOdsDataException);

            this.storageBroker.Setup(broker =>
                broker.SelectOdsDataByIdAsync(nonExistOdsData.Id))
                .ReturnsAsync(nullOdsData);

            // when 
            ValueTask<OdsData> modifyOdsDataTask =
                this.odsDataService.ModifyOdsDataAsync(nonExistOdsData);

            OdsDataServiceValidationException actualOdsDataValidationException =
                await Assert.ThrowsAsync<OdsDataServiceValidationException>(
                    testCode: modifyOdsDataTask.AsTask);

            // then
            actualOdsDataValidationException.Should()
                .BeEquivalentTo(expectedOdsDataValidationException);

            this.storageBroker.Verify(broker =>
                broker.SelectOdsDataByIdAsync(nonExistOdsData.Id),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedOdsDataValidationException))),
                        Times.Once);

            this.storageBroker.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}