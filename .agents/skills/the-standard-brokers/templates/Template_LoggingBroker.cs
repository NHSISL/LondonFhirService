// ---
// skill: the-standard-brokers
// type: template
// source-section: "1. Brokers"
// ---

// ILoggingBroker.cs — interface
using System;
using System.Threading.Tasks;

namespace {Namespace}.Brokers.Loggings
{
    public interface ILoggingBroker
    {
        ValueTask LogInformationAsync(string message);
        ValueTask LogTraceAsync(string message);
        ValueTask LogDebugAsync(string message);
        ValueTask LogWarningAsync(string message);
        ValueTask LogErrorAsync(Exception exception);
        ValueTask LogCriticalAsync(Exception exception);
    }
}

// LoggingBroker.cs — implementation
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace {Namespace}.Brokers.Loggings
{
    public class LoggingBroker : ILoggingBroker
    {
        private readonly ILogger<LoggingBroker> logger;

        public LoggingBroker(ILogger<LoggingBroker> logger) =>
            this.logger = logger;

        public async ValueTask LogInformationAsync(string message) =>
            this.logger.LogInformation(message);

        public async ValueTask LogTraceAsync(string message) =>
            this.logger.LogTrace(message);

        public async ValueTask LogDebugAsync(string message) =>
            this.logger.LogDebug(message);

        public async ValueTask LogWarningAsync(string message) =>
            this.logger.LogWarning(message);

        public async ValueTask LogErrorAsync(Exception exception) =>
            this.logger.LogError(exception, message: exception.Message);

        public async ValueTask LogCriticalAsync(Exception exception) =>
            this.logger.LogCritical(exception, message: exception.Message);
    }
}
