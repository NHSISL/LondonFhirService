// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using LondonFhirService.Core.Extensions.Exceptions;
using Microsoft.Extensions.Logging;

namespace LondonFhirService.Core.Brokers.Loggings
{
    public class LoggingBroker : ILoggingBroker
    {
        private readonly ILogger<LoggingBroker> logger;

        public LoggingBroker(ILogger<LoggingBroker> logger) =>
            this.logger = logger;

        public async ValueTask LogInformationAsync(string message) =>
            logger.LogInformation(message);

        public async ValueTask LogTraceAsync(string message) =>
            logger.LogTrace(message);

        public async ValueTask LogDebugAsync(string message) =>
            logger.LogDebug(message);

        public async ValueTask LogWarningAsync(string message) =>
            logger.LogWarning(message);

        public async ValueTask LogErrorAsync(Exception exception) =>
            logger.LogError(exception, $"{exception.Message} {exception.GetValidationSummary()}");

        public async ValueTask LogCriticalAsync(Exception exception) =>
            logger.LogCritical(exception, $"{exception.Message} {exception.GetValidationSummary()}");
    }
}