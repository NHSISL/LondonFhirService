// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.FhirRecords;
using Moq;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Core.Tests.Unit.Services.Orchestrations.CompareQueue
{
    public partial class CompareQueueOrchestrationServiceTests
    {
        [Fact]
        public async Task ShouldCompleteThePrimaryRecordAndMarkItProcessedAsync()
        {
            // given
            Guid inputFhirRecordId = Guid.NewGuid();

            this.fhirRecordServiceMock.Setup(service =>
                service.TryTransitionFhirRecordStatusAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<StatusType>(),
                    It.IsAny<StatusType>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(true);

            // when
            await this.compareQueueOrchestrationService
                .CompletePrimaryFhirRecordAsync(inputFhirRecordId);

            // then
            // Conditional on the row not already being Completed - the primary is shared by every
            // secondary of a correlation, so several workers reach this for the same row and a
            // read-then-write there is a check by one and a write by another.
            //
            // isProcessed: true matters just as much. The read-then-write path this replaced set
            // it for terminal statuses; dropping it left every completed primary marked
            // unprocessed, which anything reading IsProcessed reads as still pending.
            this.fhirRecordServiceMock.Verify(service =>
                service.TryTransitionFhirRecordStatusAsync(
                    inputFhirRecordId,
                    StatusType.Completed,
                    StatusType.Completed,
                    true,
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.fhirRecordServiceMock.VerifyNoOtherCalls();
            this.fhirRecordDifferenceServiceMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
