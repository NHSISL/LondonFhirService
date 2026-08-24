// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace LondonFhirService.Clients.AuditAndMetrics.Brokers.Loggings
{
    /// <summary>
    /// Wraps whatever logger the consuming application supplies. When it supplies none the
    /// library falls back to a null logger rather than refusing to start, so a consumer that
    /// only wants the storage side is not forced to wire logging.
    /// </summary>
    internal class LoggingBroker : ILoggingBroker
    {
        private readonly ILogger logger;

        public LoggingBroker(ILogger<LoggingBroker> logger) =>
            this.logger = logger ?? NullLogger<LoggingBroker>.Instance;

        public async ValueTask LogInformationAsync(string message) =>
            this.logger.LogInformation("{Message}", message);

        public async ValueTask LogTraceAsync(string message) =>
            this.logger.LogTrace("{Message}", message);

        public async ValueTask LogDebugAsync(string message) =>
            this.logger.LogDebug("{Message}", message);

        public async ValueTask LogWarningAsync(string message) =>
            this.logger.LogWarning("{Message}", message);

        public async ValueTask LogErrorAsync(Exception exception) =>
            this.logger.LogError(exception, "{Message}", exception.Message);

        public async ValueTask LogCriticalAsync(Exception exception) =>
            this.logger.LogCritical(exception, "{Message}", exception.Message);
    }
}
