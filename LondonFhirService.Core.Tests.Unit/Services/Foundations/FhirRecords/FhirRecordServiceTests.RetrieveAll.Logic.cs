// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.FhirRecords;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.FhirRecords
{
    public partial class FhirRecordServiceTests
    {
        [Fact]
        public async Task ShouldReturnFhirRecords()
        {
            // given
            IQueryable<FhirRecord> randomFhirRecords = CreateRandomFhirRecords();
            IQueryable<FhirRecord> storageFhirRecords = randomFhirRecords;
            IQueryable<FhirRecord> expectedFhirRecords = storageFhirRecords;

            this.storageBrokerMock.Setup(broker =>
                broker.SelectAllFhirRecordsAsync())
                    .ReturnsAsync(storageFhirRecords);

            // when
            IQueryable<FhirRecord> actualFhirRecords =
                await this.fhirRecordService.RetrieveAllFhirRecordsAsync();

            // then
            actualFhirRecords.Should().BeEquivalentTo(expectedFhirRecords);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAllFhirRecordsAsync(),
                    Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}