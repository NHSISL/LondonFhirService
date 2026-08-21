// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using LondonFhirService.Clients.AuditAndMetrics.Brokers.DateTimes;
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
            IConfiguration configuration)
            : this(storageBroker, auditUserBroker, BindConfigurations(configuration))
        { }

        public AuditAndMetricsClient(
            IAuditAndMetricsStorageBroker storageBroker,
            IAuditUserBroker auditUserBroker,
            AuditAndMetricsConfigurations configurations)
        {
            IServiceProvider serviceProvider =
                RegisterServices(storageBroker, auditUserBroker, configurations);

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
            AuditAndMetricsConfigurations configurations)
        {
            // The storage broker is supplied rather than constructed: it is the seam that keeps
            // this library free of any reference to the application that hosts it.
            IServiceCollection serviceCollection = new ServiceCollection()
                .AddSingleton(storageBroker)
                .AddSingleton(auditUserBroker)
                .AddSingleton(configurations)
                .AddLogging()
                .AddTransient<IDateTimeBroker, DateTimeBroker>()
                .AddTransient<IIdentifierBroker, IdentifierBroker>()
                .AddTransient<ILoggingBroker, LoggingBroker>()
                .AddTransient<IMetricBroker, MetricBroker>()
                .AddTransient<IAuditService, AuditService>()
                .AddTransient<IMetricService, MetricService>()
                .AddTransient<IAuditClient, AuditClient>()
                .AddTransient<IMetricClient, MetricClient>();

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
