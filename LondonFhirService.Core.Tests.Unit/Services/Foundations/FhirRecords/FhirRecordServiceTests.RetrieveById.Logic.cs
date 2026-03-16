// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
using LondonFhirService.Core.Models.Foundations.FhirRecords;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.FhirRecords
{
    public partial class FhirRecordServiceTests
    {
        [Fact]
        public async Task ShouldRetrieveFhirRecordByIdAsync()
        {
            // given
            FhirRecord randomFhirRecord = CreateRandomFhirRecord();
            FhirRecord inputFhirRecord = randomFhirRecord;
            FhirRecord storageFhirRecord = randomFhirRecord;
            FhirRecord expectedFhirRecord = storageFhirRecord.DeepClone();

            this.storageBrokerMock.Setup(broker =>
                broker.SelectFhirRecordByIdAsync(inputFhirRecord.Id))
                    .ReturnsAsync(storageFhirRecord);

            // when
            FhirRecord actualFhirRecord =
                await this.fhirRecordService.RetrieveFhirRecordByIdAsync(inputFhirRecord.Id);

            // then
            actualFhirRecord.Should().BeEquivalentTo(expectedFhirRecord);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectFhirRecordByIdAsync(inputFhirRecord.Id),
                    Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}