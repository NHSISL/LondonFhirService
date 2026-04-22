// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;

namespace LondonFhirService.Api.Tests.Unit.Workers
{
    public partial class ComparisonWorkerTests
    {
        [Fact]
        public async Task ShouldLogErrorAndContinueWhenProcessFhirRecordsThrowsAsync()
        {
            // given
            var cts = new CancellationTokenSource();
            string someMessage = GetRandomString();
            var someException = new Exception(someMessage);
            int callCount = 0;

            comparisonCoordinationServiceMock
                .Setup(service => service.ProcessFhirRecordsAsync())
                .Returns(() =>
                {
                    callCount++;

                    if (callCount == 1)
                        return ValueTask.FromException(someException);

                    cts.Cancel();

                    return ValueTask.CompletedTask;
                });

            // when
            await worker.ExecuteAsync(cts.Token);

            // then
            comparisonCoordinationServiceMock.Verify(
                service => service.ProcessFhirRecordsAsync(),
                Times.Exactly(2));

            loggerMock.Verify(logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((value, type) => true),
                someException,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            comparisonCoordinationServiceMock.VerifyNoOtherCalls();
        }
    }
}
