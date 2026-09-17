// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using FluentAssertions;
using LondonFhirService.Clients.AuditAndMetrics.Clients;
using LondonFhirService.Clients.AuditAndMetrics.Models.Configurations;
using LondonFhirService.Core.Abstractions.Brokers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace LondonFhirService.Clients.AuditAndMetrics.Tests.Unit.Clients
{
    /// <summary>
    /// The client builds its own container, so a broker it forgets to register is not a compile
    /// error - it is an exception on the first request, in production. Every test elsewhere in
    /// this project constructs its subject directly with mocks, which never touches that wiring.
    /// These tests construct the real client so the container has to resolve for real.
    /// </summary>
    public class AuditAndMetricsClientTests
    {
        private readonly Mock<IAuditAndMetricStorageBroker> storageBrokerMock;
        private readonly Mock<IAuditUserBroker> auditUserBrokerMock;

        public AuditAndMetricsClientTests()
        {
            this.storageBrokerMock = new Mock<IAuditAndMetricStorageBroker>();
            this.auditUserBrokerMock = new Mock<IAuditUserBroker>();
        }

        [Fact]
        public void ShouldResolveBothClientsFromItsOwnContainer()
        {
            // given
            var configurations = new AuditAndMetricsConfigurations();

            // when
            var auditAndMetricsClient = new AuditAndMetricsClient(
                this.storageBrokerMock.Object,
                this.auditUserBrokerMock.Object,
                configurations);

            // then
            // Resolving both exercises every registration behind them - the services, and every
            // broker those services depend on.
            auditAndMetricsClient.AuditClient.Should().NotBeNull();
            auditAndMetricsClient.MetricClient.Should().NotBeNull();
        }

        [Fact]
        public void ShouldLogThroughTheHostsLoggerFactory()
        {
            // given
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger>();

            loggerFactoryMock.Setup(factory => factory.CreateLogger(It.IsAny<string>()))
                .Returns(loggerMock.Object);

            // when
            var auditAndMetricsClient = new AuditAndMetricsClient(
                this.storageBrokerMock.Object,
                this.auditUserBrokerMock.Object,
                new AuditAndMetricsConfigurations(),
                loggerFactoryMock.Object);

            _ = auditAndMetricsClient.AuditClient;

            // then
            // Without this the library builds its own factory with no providers, and every line
            // it writes is discarded - including the only report a failed background write makes.
            loggerFactoryMock.Verify(factory =>
                factory.CreateLogger(It.IsAny<string>()),
                    Times.AtLeastOnce);
        }

        [Fact]
        public void ShouldStillConstructWhenTheHostSuppliesNoLoggerFactory()
        {
            // given, when
            var auditAndMetricsClient = new AuditAndMetricsClient(
                this.storageBrokerMock.Object,
                this.auditUserBrokerMock.Object,
                new AuditAndMetricsConfigurations(),
                loggerFactory: null);

            // then
            // Silence is a legitimate choice for a consumer, but it has to be chosen rather than
            // arrived at by accident.
            auditAndMetricsClient.AuditClient.Should().NotBeNull();
            auditAndMetricsClient.MetricClient.Should().NotBeNull();
        }

        [Fact]
        public void ShouldBindConfigurationFromTheNamedSection()
        {
            // given
            int expectedAuditRetentionPeriodInDays = 42;
            int expectedMetricsRetentionPeriodInDays = 77;
            int expectedPurgeBatchSize = 1234;
            string expectedActivitySourceName = "Some.Other.Source";

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    [$"{AuditAndMetricsClient.ConfigurationSectionName}:IsAuditEnabled"] = "false",

                    [$"{AuditAndMetricsClient.ConfigurationSectionName}:IsAuditPurgingAllowed"] =
                        "true",

                    [$"{AuditAndMetricsClient.ConfigurationSectionName}:AuditRetentionPeriodInDays"] =
                        expectedAuditRetentionPeriodInDays.ToString(),

                    [$"{AuditAndMetricsClient.ConfigurationSectionName}:IsMetricsEnabled"] = "false",

                    [$"{AuditAndMetricsClient.ConfigurationSectionName}:IsMetricsPurgingAllowed"] =
                        "true",

                    [$"{AuditAndMetricsClient.ConfigurationSectionName}:MetricsRetentionPeriodInDays"] =
                        expectedMetricsRetentionPeriodInDays.ToString(),

                    [$"{AuditAndMetricsClient.ConfigurationSectionName}:PurgeBatchSize"] =
                        expectedPurgeBatchSize.ToString(),

                    [$"{AuditAndMetricsClient.ConfigurationSectionName}:ActivitySourceName"] =
                        expectedActivitySourceName
                })
                .Build();

            // when
            AuditAndMetricsConfigurations actualConfigurations =
                AuditAndMetricsClient.BindConfigurations(configuration);

            // then
            // Pinned to the section name the application's appsettings has to use. A rename on
            // either side silently falls back to defaults rather than failing, so the two names
            // have to be held together by a test.
            actualConfigurations.IsAuditEnabled.Should().BeFalse();
            actualConfigurations.IsAuditPurgingAllowed.Should().BeTrue();

            actualConfigurations.AuditRetentionPeriodInDays.Should()
                .Be(expectedAuditRetentionPeriodInDays);

            actualConfigurations.IsMetricsEnabled.Should().BeFalse();
            actualConfigurations.IsMetricsPurgingAllowed.Should().BeTrue();

            actualConfigurations.MetricsRetentionPeriodInDays.Should()
                .Be(expectedMetricsRetentionPeriodInDays);

            actualConfigurations.PurgeBatchSize.Should().Be(expectedPurgeBatchSize);
            actualConfigurations.ActivitySourceName.Should().Be(expectedActivitySourceName);
        }

        [Fact]
        public void ShouldFallBackToDefaultsWhenTheSectionIsAbsent()
        {
            // given
            IConfiguration configuration = new ConfigurationBuilder().Build();

            // when
            AuditAndMetricsConfigurations actualConfigurations =
                AuditAndMetricsClient.BindConfigurations(configuration);

            // then
            // A host that has not configured the library still starts rather than failing at
            // construction, with recording on and both retention sweeps live at 30 days. The
            // defaults purge, so an unconfigured host ages its tables out rather than growing
            // them forever - the section is what an environment sets to keep longer, or to
            // stop deleting altogether.
            actualConfigurations.Should().NotBeNull();
            actualConfigurations.IsAuditEnabled.Should().BeTrue();
            actualConfigurations.IsAuditPurgingAllowed.Should().BeTrue();
            actualConfigurations.AuditRetentionPeriodInDays.Should().Be(30);
            actualConfigurations.IsMetricsEnabled.Should().BeTrue();
            actualConfigurations.IsMetricsPurgingAllowed.Should().BeTrue();
            actualConfigurations.MetricsRetentionPeriodInDays.Should().Be(30);
            actualConfigurations.PurgeBatchSize.Should().Be(5000);
        }
    }
}
