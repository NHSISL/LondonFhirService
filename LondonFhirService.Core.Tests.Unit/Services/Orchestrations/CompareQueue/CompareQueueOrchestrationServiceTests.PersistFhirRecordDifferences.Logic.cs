// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.FhirRecordDifferences;
using LondonFhirService.Core.Models.Orchestrations.CompareQueue;
using Moq;
using Xunit;

namespace LondonFhirService.Core.Tests.Unit.Services.Orchestrations.CompareQueue
{
    public partial class CompareQueueOrchestrationServiceTests
    {
        [Fact]
        public async Task ShouldPersistFhirRecordDifferencesAsync()
        {
            // given
            CompareQueueItem randomCompareQueueItem = CreateRandomCompareQueueItem();
            CompareQueueItem inputCompareQueueItem = randomCompareQueueItem;
            FhirRecordDifference inputFhirRecordDifference = inputCompareQueueItem.FhirRecordDifference;
            FhirRecordDifference storedFhirRecordDifference = inputFhirRecordDifference;

            this.fhirRecordDifferenceServiceMock.Setup(service =>
                service.AddFhirRecordDifferenceAsync(inputFhirRecordDifference))
                    .ReturnsAsync(storedFhirRecordDifference);

            // when
            await this.compareQueueOrchestrationService
                .PersistFhirRecordDifferencesAsync(inputCompareQueueItem);

            // then
            this.fhirRecordDifferenceServiceMock.Verify(service =>
                service.AddFhirRecordDifferenceAsync(inputFhirRecordDifference),
                    Times.Once);

            this.fhirRecordServiceMock.VerifyNoOtherCalls();
            this.fhirRecordDifferenceServiceMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
