// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
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
                broker.SelectFhirRecordByIdAsync(inputFhirRecordId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(storageFhirRecord);

            this.storageBrokerMock.Setup(broker =>
                broker.DeleteFhirRecordAsync(expectedInputFhirRecord, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(deletedFhirRecord);

            // when
            FhirRecord actualFhirRecord = await this.fhirRecordService
                .RemoveFhirRecordByIdAsync(inputFhirRecordId, TestContext.Current.CancellationToken);

            // then
            actualFhirRecord.Should().BeEquivalentTo(expectedFhirRecord);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectFhirRecordByIdAsync(inputFhirRecordId, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.DeleteFhirRecordAsync(expectedInputFhirRecord, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}