// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.PdsDatas.Exceptions;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.PdsDatas
{
    public partial class PdsDataServiceTests
    {
        private const List<string> emptyList = default;

        [Theory]
        [InlineData(null, null)]
        [InlineData("", emptyList)]
        [InlineData(" ", emptyList)]
        public async Task ShouldThrowValidationExceptionOnOrganisationsHaveAccessToThisPatientAndLogItAsync(
            string invalidItem,
            List<string> invalidList)
        {
            // given
            string invalidPatientIdentifier = invalidItem;
            string invalidNhsNumber = invalidItem;
            Guid invalidCorrelationId = Guid.Empty;

            var invalidPdsDataException =
                new InvalidPdsDataServiceException(
                    message: "Invalid argument(s), please correct the errors and try again.");

            invalidPdsDataException.AddData(
                key: "nhsNumber",
                values: "Text is invalid");

            invalidPdsDataException.AddData(
                key: "patientIdentifier",
                values: "Text is invalid");

            invalidPdsDataException.AddData(
                key: "organisationCodes",
                values: "Items is invalid");

            invalidPdsDataException.AddData(
                key: "correlationId",
                values: "Id is invalid");

            var expectedPdsDataValidationException =
                new PdsDataServiceValidationException(
                    message: "PdsData validation error occurred, please fix errors and try again.",
                    innerException: invalidPdsDataException);

            // when
            ValueTask<bool> retrievePdsDataByIdTask =
                this.pdsDataService.OrganisationsHaveAccessToThisPatient(
                    invalidPatientIdentifier,
                    invalidNhsNumber,
                    invalidList,
                    invalidCorrelationId);

            PdsDataServiceValidationException actualPdsDataValidationException =
                await Assert.ThrowsAsync<PdsDataServiceValidationException>(
                    testCode: retrievePdsDataByIdTask.AsTask);

            // then
            actualPdsDataValidationException.Should()
                .BeEquivalentTo(expectedPdsDataValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedPdsDataValidationException))),
                        Times.Once);

            this.storageBroker.Verify(broker =>
                broker.SelectPdsDataByIdAsync(It.IsAny<Guid>()),
                    Times.Never);

            this.dateTimeBroker.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBroker.VerifyNoOtherCalls();
            this.storageBroker.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
        }
    }
}