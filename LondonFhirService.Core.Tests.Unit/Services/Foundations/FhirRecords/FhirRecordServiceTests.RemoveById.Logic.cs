// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
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
        public async Task ShouldRemoveFhirRecordByIdAsync()
        {
            // given
            Guid randomId = Guid.NewGuid();
            Guid inputFhirRecordId = randomId;
            FhirRecord randomFhirRecord = CreateRandomFhirRecord();
            FhirRecord storageFhirRecord = randomFhirRecord;
            FhirRecord expectedInputFhirRecord = storageFhirRecord;
            FhirRecord deletedFhirRecord = expectedInputFhirRecord;
            FhirRecord expectedFhirRecord = deletedFhirRecord.DeepClone();

            this.storageBrokerMock.Setup(broker =>
                broker.SelectFhirRecordByIdAsync(inputFhirRecordId))
                    .ReturnsAsync(storageFhirRecord);

            this.storageBrokerMock.Setup(broker =>
                broker.DeleteFhirRecordAsync(expectedInputFhirRecord))
                    .ReturnsAsync(deletedFhirRecord);

            // when
            FhirRecord actualFhirRecord = await this.fhirRecordService
                .RemoveFhirRecordByIdAsync(inputFhirRecordId);

            // then
            actualFhirRecord.Should().BeEquivalentTo(expectedFhirRecord);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectFhirRecordByIdAsync(inputFhirRecordId),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.DeleteFhirRecordAsync(expectedInputFhirRecord),
                    Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}