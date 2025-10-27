// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
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
        public async Task ShouldThrowValidationExceptionOnRetrieveAllAncestorsByChildIdIsInvalidAndLogItAsync()
        {
            // given
            var invalidOdsDataId = Guid.Empty;

            var invalidOdsDataException =
                new InvalidOdsDataServiceException(
                    message: "Invalid odsData. Please correct the errors and try again.");

            invalidOdsDataException.AddData(
                key: nameof(OdsData.Id),
                values: "Id is invalid");

            var expectedOdsDataValidationException =
                new OdsDataServiceValidationException(
                    message: "OdsData validation error occurred, please fix errors and try again.",
                    innerException: invalidOdsDataException);

            // when
            ValueTask<List<OdsData>> retrieveAncestorsByChildIdTask =
                this.odsDataService.RetrieveAllAncestorsByChildId(invalidOdsDataId);

            OdsDataServiceValidationException actualOdsDataValidationException =
                await Assert.ThrowsAsync<OdsDataServiceValidationException>(
                    testCode: retrieveAncestorsByChildIdTask.AsTask);

            // then
            actualOdsDataValidationException.Should()
                .BeEquivalentTo(expectedOdsDataValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedOdsDataValidationException))),
                        Times.Once);

            this.storageBroker.Verify(broker =>
                broker.SelectAllOdsDatasAsync(),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBroker.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowNotFoundExceptionOnRetrieveAllAncestorsByChildIdIfOdsDataIsNotFoundAndLogItAsync()
        {
            //given
            Guid someOdsDataId = Guid.NewGuid();
            OdsData noOdsData = null;
            var notFoundOdsDataException =
                new NotFoundOdsDataServiceException(message: $"OdsData not found with Id: {someOdsDataId}");

            var expectedOdsDataValidationException =
                new OdsDataServiceValidationException(
                    message: "OdsData validation error occurred, please fix errors and try again.",
                    innerException: notFoundOdsDataException);

            this.storageBroker.Setup(broker =>
                broker.SelectOdsDataByIdAsync(It.IsAny<Guid>()))
                    .ReturnsAsync(noOdsData);

            //when
            ValueTask<List<OdsData>> retrieveAncestorsByChildIdTask =
                this.odsDataService.RetrieveAllAncestorsByChildId(someOdsDataId);

            OdsDataServiceValidationException actualOdsDataValidationException =
                await Assert.ThrowsAsync<OdsDataServiceValidationException>(
                    testCode: retrieveAncestorsByChildIdTask.AsTask);

            //then
            actualOdsDataValidationException.Should().BeEquivalentTo(expectedOdsDataValidationException);

            this.storageBroker.Verify(broker =>
                broker.SelectOdsDataByIdAsync(It.IsAny<Guid>()),
                    Times.Once());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedOdsDataValidationException))),
                        Times.Once);

            this.storageBroker.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
