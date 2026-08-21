// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Api.Workers;
using Microsoft.Extensions.Logging;
using Moq;

namespace LondonFhirService.Api.Tests.Unit.Workers
{
    public class MetricTelemetryPublisherTests
    {
        [Fact]
        public async Task ShouldStartWithoutATelemetryClientAsync()
        {
            // given
            var loggerMock = new Mock<ILogger<MetricTelemetryPublisher>>();

            var publisher = new MetricTelemetryPublisher(
                telemetryClient: null,
                logger: loggerMock.Object,
                activitySourceName: "LondonFhirService.Metrics");

            // when
            Func<Task> startPublisher = async () =>
                await publisher.StartAsync(CancellationToken.None);

            // then
            // Application Insights is excluded from some hosts on purpose. A hosted service that
            // throws while starting takes the whole host down, so the absence of a client has to
            // be a quiet no-op rather than a failure to boot.
            await startPublisher.Should().NotThrowAsync();

            publisher.Dispose();
        }
    }
}
