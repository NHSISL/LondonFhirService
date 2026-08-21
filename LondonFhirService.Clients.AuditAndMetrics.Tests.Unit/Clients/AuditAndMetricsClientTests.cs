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
        private readonly Mock<IAuditAndMetricsStorageBroker> storageBrokerMock;
        private readonly Mock<IAuditUserBroker> auditUserBrokerMock;

        public AuditAndMetricsClientTests()
        {
            this.storageBrokerMock = new Mock<IAuditAndMetricsStorageBroker>();
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
        public void ShouldBindConfigurationFromTheNamedSection()
        {
            // given
            int expectedRetentionPeriodInDays = 42;
            int expectedPurgeBatchSize = 1234;
            string expectedActivitySourceName = "Some.Other.Source";

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    [$"{AuditAndMetricsClient.ConfigurationSectionName}:IsEnabled"] = "false",
                    [$"{AuditAndMetricsClient.ConfigurationSectionName}:IsPurgingAllowed"] = "true",

                    [$"{AuditAndMetricsClient.ConfigurationSectionName}:RetentionPeriodInDays"] =
                        expectedRetentionPeriodInDays.ToString(),

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
            actualConfigurations.IsEnabled.Should().BeFalse();
            actualConfigurations.IsPurgingAllowed.Should().BeTrue();
            actualConfigurations.RetentionPeriodInDays.Should().Be(expectedRetentionPeriodInDays);
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
            // A host that has not configured the library still starts, recording enabled and
            // purging off, rather than failing at construction.
            actualConfigurations.Should().NotBeNull();
            actualConfigurations.IsEnabled.Should().BeTrue();
            actualConfigurations.IsPurgingAllowed.Should().BeFalse();
        }
    }
}
