// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.FhirRecords;
using Moq;
using Xunit;

namespace LondonFhirService.Core.Tests.Unit.Services.Orchestrations.CompareQueue
{
    public partial class CompareQueueOrchestrationServiceTests
    {
        [Fact]
        public async Task ShouldChangeFhirRecordStatusAsync()
        {
            // given
            Guid randomFhirRecordId = Guid.NewGuid();
            Guid inputFhirRecordId = randomFhirRecordId;
            StatusType inputStatus = StatusType.Processing;

            FhirRecord randomFhirRecord = CreateRandomFhirRecord();
            randomFhirRecord.IsProcessed = false;
            FhirRecord storageFhirRecord = randomFhirRecord;

            this.fhirRecordServiceMock.Setup(service =>
                service.RetrieveFhirRecordByIdAsync(inputFhirRecordId))
                    .ReturnsAsync(storageFhirRecord);

            this.fhirRecordServiceMock.Setup(service =>
                service.ModifyFhirRecordAsync(storageFhirRecord))
                    .ReturnsAsync(storageFhirRecord);

            // when
            await this.compareQueueOrchestrationService
                .ChangeFhirRecordStatusAsync(inputFhirRecordId, inputStatus);

            // then
            storageFhirRecord.Status.Should().Be(inputStatus);
            storageFhirRecord.IsProcessed.Should().BeFalse();

            this.fhirRecordServiceMock.Verify(service =>
                service.RetrieveFhirRecordByIdAsync(inputFhirRecordId),
                    Times.Once);

            this.fhirRecordServiceMock.Verify(service =>
                service.ModifyFhirRecordAsync(storageFhirRecord),
                    Times.Once);

            this.fhirRecordServiceMock.VerifyNoOtherCalls();
            this.fhirRecordDifferenceServiceMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(StatusType.Completed)]
        [InlineData(StatusType.Failed)]
        public async Task ShouldChangeFhirRecordStatusAndMarkAsProcessedWhenTerminalStatusAsync(
            StatusType terminalStatus)
        {
            // given
            Guid randomFhirRecordId = Guid.NewGuid();
            Guid inputFhirRecordId = randomFhirRecordId;
            StatusType inputStatus = terminalStatus;

            FhirRecord randomFhirRecord = CreateRandomFhirRecord();
            randomFhirRecord.IsProcessed = false;
            FhirRecord storageFhirRecord = randomFhirRecord;

            this.fhirRecordServiceMock.Setup(service =>
                service.RetrieveFhirRecordByIdAsync(inputFhirRecordId))
                    .ReturnsAsync(storageFhirRecord);

            this.fhirRecordServiceMock.Setup(service =>
                service.ModifyFhirRecordAsync(storageFhirRecord))
                    .ReturnsAsync(storageFhirRecord);

            // when
            await this.compareQueueOrchestrationService
                .ChangeFhirRecordStatusAsync(inputFhirRecordId, inputStatus);

            // then
            storageFhirRecord.Status.Should().Be(inputStatus);
            storageFhirRecord.IsProcessed.Should().BeTrue();

            this.fhirRecordServiceMock.Verify(service =>
                service.RetrieveFhirRecordByIdAsync(inputFhirRecordId),
                    Times.Once);

            this.fhirRecordServiceMock.Verify(service =>
                service.ModifyFhirRecordAsync(storageFhirRecord),
                    Times.Once);

            this.fhirRecordServiceMock.VerifyNoOtherCalls();
            this.fhirRecordDifferenceServiceMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
