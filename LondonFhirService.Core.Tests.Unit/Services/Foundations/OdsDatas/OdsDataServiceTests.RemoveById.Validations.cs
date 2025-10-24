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
        public async Task ShouldThrowValidationExceptionOnRemoveIfIdIsInvalidAndLogItAsync()
        {
            // given
            Guid invalidOdsDataId = Guid.Empty;

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
            ValueTask<OdsData> removeOdsDataByIdTask =
                this.odsDataService.RemoveOdsDataByIdAsync(invalidOdsDataId);

            OdsDataServiceValidationException actualOdsDataValidationException =
                await Assert.ThrowsAsync<OdsDataServiceValidationException>(
                    testCode: removeOdsDataByIdTask.AsTask);

            // then
            actualOdsDataValidationException.Should()
                .BeEquivalentTo(expectedOdsDataValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedOdsDataValidationException))),
                        Times.Once);

            this.storageBroker.Verify(broker =>
                broker.DeleteOdsDataAsync(It.IsAny<OdsData>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBroker.VerifyNoOtherCalls();
        }
    }
}