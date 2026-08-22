// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using LondonFhirService.Clients.AuditAndMetrics.Brokers.DateTimes;
using LondonFhirService.Clients.AuditAndMetrics.Brokers.Dispatchers;
using LondonFhirService.Clients.AuditAndMetrics.Brokers.Identifiers;
using LondonFhirService.Clients.AuditAndMetrics.Brokers.Loggings;
using LondonFhirService.Clients.AuditAndMetrics.Brokers.Metrics;
using LondonFhirService.Core.Abstractions.Brokers;
using LondonFhirService.Clients.AuditAndMetrics.Clients.Audits;
using LondonFhirService.Clients.AuditAndMetrics.Clients.Metrics;
using LondonFhirService.Clients.AuditAndMetrics.Models.Configurations;
using LondonFhirService.Clients.AuditAndMetrics.Services.Foundations.Audits;
using LondonFhirService.Clients.AuditAndMetrics.Services.Foundations.Metrics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace LondonFhirService.Clients.AuditAndMetrics.Clients
{
    /// <summary>
    /// Follows the SecurityClient shape: the client owns its own ServiceCollection, registers
    /// everything it needs, builds a provider and resolves its sub-clients from it. Consumers
    /// hand in the two things the library cannot supply itself - somewhere to persist to, and its
    /// configuration - and get a wired client back.
    /// </summary>
    public class AuditAndMetricsClient : IAuditAndMetricsClient
    {
        public const string ConfigurationSectionName = "AuditAndMetricsConfigurations";

        public AuditAndMetricsClient(
            IAuditAndMetricsStorageBroker storageBroker,
            IAuditUserBroker auditUserBroker,
            IConfiguration configuration,
            ILoggerFactory loggerFactory = null,
            IAuditAndMetricsDispatcher dispatcher = null)
            : this(
                storageBroker,
                auditUserBroker,
                BindConfigurations(configuration),
                loggerFactory,
                dispatcher)
        { }

        public AuditAndMetricsClient(
            IAuditAndMetricsStorageBroker storageBroker,
            IAuditUserBroker auditUserBroker,
            AuditAndMetricsConfigurations configurations,
            ILoggerFactory loggerFactory = null,
            IAuditAndMetricsDispatcher dispatcher = null)
        {
            IServiceProvider serviceProvider = RegisterServices(
                storageBroker,
                auditUserBroker,
                configurations,
                loggerFactory,
                dispatcher);

            InitializeClients(serviceProvider);
        }

        public IAuditClient AuditClient { get; private set; }
        public IMetricClient MetricClient { get; private set; }

        private void InitializeClients(IServiceProvider serviceProvider)
        {
            AuditClient = serviceProvider.GetRequiredService<IAuditClient>();
            MetricClient = serviceProvider.GetRequiredService<IMetricClient>();
        }

        private static IServiceProvider RegisterServices(
            IAuditAndMetricsStorageBroker storageBroker,
            IAuditUserBroker auditUserBroker,
            AuditAndMetricsConfigurations configurations,
            ILoggerFactory loggerFactory,
            IAuditAndMetricsDispatcher dispatcher)
        {
            // The storage broker is supplied rather than constructed: it is the seam that keeps
            // this library free of any reference to the application that hosts it.
            IServiceCollection serviceCollection = new ServiceCollection()
                .AddSingleton(storageBroker)
                .AddSingleton(auditUserBroker)
                .AddSingleton(configurations)

                // The consuming application's factory, not a fresh one. AddLogging() on its own
                // registers an ILoggerFactory with no providers, so every line this library
                // writes would be discarded - including the only channel a failed fire-and-forget
                // write has to report itself. NullLoggerFactory is the deliberate no-op for a
                // consumer that genuinely wants silence.
                .AddSingleton(loggerFactory ?? NullLoggerFactory.Instance)
                .AddSingleton(typeof(ILogger<>), typeof(Logger<>))
                .AddTransient<IDateTimeBroker, DateTimeBroker>()
                .AddTransient<IIdentifierBroker, IdentifierBroker>()
                .AddTransient<ILoggingBroker, LoggingBroker>()
                .AddTransient<IMetricBroker, MetricBroker>()
                .AddTransient<IAuditService, AuditService>()
                .AddTransient<IMetricService, MetricService>()
                .AddTransient<IAuditClient, AuditClient>()
                .AddTransient<IMetricClient, MetricClient>();

            // A host with a lifecycle can queue deferred writes and drain them under control.
            // Without one the fallback is a thread pool item per write, which is unbounded.
            if (dispatcher is null)
            {
                serviceCollection.AddSingleton<IAuditAndMetricsDispatcher, ThreadPoolDispatcher>();
            }
            else
            {
                serviceCollection.AddSingleton(dispatcher);
            }

            return serviceCollection.BuildServiceProvider();
        }

        internal static AuditAndMetricsConfigurations BindConfigurations(IConfiguration configuration)
        {
            AuditAndMetricsConfigurations configurations =
                configuration
                    .GetSection(ConfigurationSectionName)
                    .Get<AuditAndMetricsConfigurations>();

            return configurations ?? new AuditAndMetricsConfigurations();
        }
    }
}
