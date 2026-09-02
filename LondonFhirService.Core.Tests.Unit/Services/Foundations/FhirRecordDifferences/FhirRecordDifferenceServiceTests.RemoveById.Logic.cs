// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
using LondonFhirService.Core.Models.Foundations.FhirRecordDifferences;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.FhirRecordDifferences
{
    public partial class FhirRecordDifferenceServiceTests
    {
        [Fact]
        public async Task ShouldRemoveFhirRecordDifferenceByIdAsync()
        {
            // given
            Guid randomId = Guid.NewGuid();
            Guid inputFhirRecordDifferenceId = randomId;
            FhirRecordDifference randomFhirRecordDifference = CreateRandomFhirRecordDifference();
            FhirRecordDifference storageFhirRecordDifference = randomFhirRecordDifference;
            FhirRecordDifference expectedInputFhirRecordDifference = storageFhirRecordDifference;
            FhirRecordDifference deletedFhirRecordDifference = expectedInputFhirRecordDifference;
            FhirRecordDifference expectedFhirRecordDifference = deletedFhirRecordDifference.DeepClone();

            this.storageBrokerMock.Setup(broker =>
                broker.SelectFhirRecordDifferenceByIdAsync(inputFhirRecordDifferenceId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(storageFhirRecordDifference);

            this.storageBrokerMock.Setup(broker =>
                broker.DeleteFhirRecordDifferenceAsync(
                    expectedInputFhirRecordDifference, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(deletedFhirRecordDifference);

            // when
            FhirRecordDifference actualFhirRecordDifference = await this.fhirRecordDifferenceService
                .RemoveFhirRecordDifferenceByIdAsync(
                    inputFhirRecordDifferenceId, TestContext.Current.CancellationToken);

            // then
            actualFhirRecordDifference.Should().BeEquivalentTo(expectedFhirRecordDifference);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectFhirRecordDifferenceByIdAsync(inputFhirRecordDifferenceId, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.DeleteFhirRecordDifferenceAsync(
                    expectedInputFhirRecordDifference, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}