// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

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
        public async Task ShouldRetrieveFhirRecordDifferenceByIdAsync()
        {
            // given
            FhirRecordDifference randomFhirRecordDifference = CreateRandomFhirRecordDifference();
            FhirRecordDifference inputFhirRecordDifference = randomFhirRecordDifference;
            FhirRecordDifference storageFhirRecordDifference = randomFhirRecordDifference;
            FhirRecordDifference expectedFhirRecordDifference = storageFhirRecordDifference.DeepClone();

            this.storageBrokerMock.Setup(broker =>
                broker.SelectFhirRecordDifferenceByIdAsync(inputFhirRecordDifference.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(storageFhirRecordDifference);

            // when
            FhirRecordDifference actualFhirRecordDifference =
                await this.fhirRecordDifferenceService.RetrieveFhirRecordDifferenceByIdAsync(
                    inputFhirRecordDifference.Id, TestContext.Current.CancellationToken);

            // then
            actualFhirRecordDifference.Should().BeEquivalentTo(expectedFhirRecordDifference);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectFhirRecordDifferenceByIdAsync(inputFhirRecordDifference.Id, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}