// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
using LondonFhirService.Core.Models.Foundations.FhirRecords;
using LondonFhirService.Core.Models.Orchestrations.CompareQueue;
using Moq;
using Xunit;

namespace LondonFhirService.Core.Tests.Unit.Services.Orchestrations.CompareQueue
{
    public partial class CompareQueueOrchestrationServiceTests
    {
        [Fact]
        public async Task ShouldGetUnprocessedRecordAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            DateTimeOffset inputDateTimeOffset = randomDateTimeOffset;
            DateTimeOffset bufferedDateTimeOffset = inputDateTimeOffset.AddMinutes(-5);

            FhirRecord randomSecondaryFhirRecord =
                CreateRandomFhirRecord(bufferedDateTimeOffset.AddMinutes(-1));

            randomSecondaryFhirRecord.Status = StatusType.Pending;
            randomSecondaryFhirRecord.IsPrimarySource = false;
            FhirRecord inputSecondaryFhirRecord = randomSecondaryFhirRecord;

            FhirRecord storedSecondaryFhirRecord = inputSecondaryFhirRecord.DeepClone();
            storedSecondaryFhirRecord.Status = StatusType.Processing;

            FhirRecord randomPrimaryFhirRecord = CreateRandomFhirRecord(inputDateTimeOffset);
            randomPrimaryFhirRecord.IsPrimarySource = true;
            randomPrimaryFhirRecord.CorrelationId = storedSecondaryFhirRecord.CorrelationId;
            FhirRecord inputPrimaryFhirRecord = randomPrimaryFhirRecord;

            IQueryable<FhirRecord> secondaryFhirRecords =
                new List<FhirRecord> { inputSecondaryFhirRecord }.AsQueryable();

            IQueryable<FhirRecord> primaryFhirRecords =
                new List<FhirRecord> { inputPrimaryFhirRecord }.AsQueryable();

            var expectedCompareQueueItem = new CompareQueueItem();
            expectedCompareQueueItem.PrimaryFhirRecord = inputPrimaryFhirRecord;
            expectedCompareQueueItem.SecondaryFhirRecord = storedSecondaryFhirRecord;

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(inputDateTimeOffset);

            this.fhirRecordServiceMock.SetupSequence(service =>
                service.RetrieveAllFhirRecordsAsync())
                    .ReturnsAsync(secondaryFhirRecords)
                    .ReturnsAsync(primaryFhirRecords);

            this.fhirRecordServiceMock.Setup(service =>
                service.ModifyFhirRecordAsync(inputSecondaryFhirRecord))
                    .ReturnsAsync(storedSecondaryFhirRecord);

            // when
            CompareQueueItem actualCompareQueueItem =
                await this.compareQueueOrchestrationService.GetUnprocessedRecordAsync();

            // then
            actualCompareQueueItem.Should().BeEquivalentTo(expectedCompareQueueItem);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.fhirRecordServiceMock.Verify(service =>
                service.RetrieveAllFhirRecordsAsync(),
                    Times.Exactly(2));

            this.fhirRecordServiceMock.Verify(service =>
                service.ModifyFhirRecordAsync(inputSecondaryFhirRecord),
                    Times.Once);

            this.fhirRecordServiceMock.VerifyNoOtherCalls();
            this.fhirRecordDifferenceServiceMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnNullWhenNoUnprocessedRecordsExistAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            DateTimeOffset inputDateTimeOffset = randomDateTimeOffset;

            IQueryable<FhirRecord> emptyFhirRecords =
                new List<FhirRecord>().AsQueryable();

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(inputDateTimeOffset);

            this.fhirRecordServiceMock.Setup(service =>
                service.RetrieveAllFhirRecordsAsync())
                    .ReturnsAsync(emptyFhirRecords);

            // when
            CompareQueueItem actualCompareQueueItem =
                await this.compareQueueOrchestrationService.GetUnprocessedRecordAsync();

            // then
            actualCompareQueueItem.Should().BeNull();

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.fhirRecordServiceMock.Verify(service =>
                service.RetrieveAllFhirRecordsAsync(),
                    Times.Once);

            this.fhirRecordServiceMock.VerifyNoOtherCalls();
            this.fhirRecordDifferenceServiceMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
