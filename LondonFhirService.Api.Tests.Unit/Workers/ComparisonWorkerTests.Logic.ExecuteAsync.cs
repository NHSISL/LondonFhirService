// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;
using Moq;

namespace LondonFhirService.Api.Tests.Unit.Workers
{
    public partial class ComparisonWorkerTests
    {
        [Fact]
        public async Task ShouldCallProcessFhirRecordsOnExecuteAsync()
        {
            // given
            var cts = new CancellationTokenSource();

            comparisonCoordinationServiceMock
                .Setup(service => service.ProcessFhirRecords())
                .Callback(cts.Cancel)
                .Returns(ValueTask.CompletedTask);

            // when
            await worker.ExecuteAsync(cts.Token);

            // then
            serviceScopeFactoryMock.Verify(
                factory => factory.CreateScope(),
                Times.Once);

            comparisonCoordinationServiceMock.Verify(
                service => service.ProcessFhirRecords(),
                Times.Once);

            comparisonCoordinationServiceMock.VerifyNoOtherCalls();
        }
    }
}
