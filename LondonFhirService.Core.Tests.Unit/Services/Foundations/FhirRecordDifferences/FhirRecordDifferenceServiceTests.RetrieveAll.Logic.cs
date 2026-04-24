// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.FhirRecordDifferences;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.FhirRecordDifferences
{
    public partial class FhirRecordDifferenceServiceTests
    {
        [Fact]
        public async Task ShouldReturnFhirRecordDifferences()
        {
            // given
            IQueryable<FhirRecordDifference> randomFhirRecordDifferences = CreateRandomFhirRecordDifferences();
            IQueryable<FhirRecordDifference> storageFhirRecordDifferences = randomFhirRecordDifferences;
            IQueryable<FhirRecordDifference> expectedFhirRecordDifferences = storageFhirRecordDifferences;

            this.storageBrokerMock.Setup(broker =>
                broker.SelectAllFhirRecordDifferencesAsync())
                    .ReturnsAsync(storageFhirRecordDifferences);

            // when
            IQueryable<FhirRecordDifference> actualFhirRecordDifferences =
                await this.fhirRecordDifferenceService.RetrieveAllFhirRecordDifferencesAsync();

            // then
            actualFhirRecordDifferences.Should().BeEquivalentTo(expectedFhirRecordDifferences);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAllFhirRecordDifferencesAsync(),
                    Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}