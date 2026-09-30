// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using LondonFhirService.Core.Brokers.Securities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NHSOneLondon.AuditAndMetrics.Abstractions.Brokers;

namespace LondonFhirService.Manage.Tests.Unit.Startup
{
    /// <summary>
    /// These brokers read the caller's ClaimsPrincipal in their constructors, not per call, so the
    /// lifetime they are registered with decides whose identity every access rule and audit stamp
    /// is evaluated against. A singleton would freeze the first principal the process ever saw and
    /// nothing would throw - behavioural tests routinely exclude the audit fields, so only a check
    /// on the registration itself catches it.
    /// </summary>
    public class IdentityBrokerRegistrationTests
    {
        [Theory]
        [InlineData(typeof(IAuditUserBroker))]
        [InlineData(typeof(ISecurityAuditBroker))]
        [InlineData(typeof(ISecurityBroker))]
        public void ShouldNotRegisterAnIdentityCapturingBrokerAsASingleton(Type brokerType)
        {
            // given
            IServiceCollection services = CreateServicesWithBrokers();

            // when
            List<ServiceDescriptor> registrations = services
                .Where(descriptor => descriptor.ServiceType == brokerType)
                .ToList();

            // then
            registrations.Should().NotBeEmpty();

            registrations.Should().OnlyContain(descriptor =>
                descriptor.Lifetime != ServiceLifetime.Singleton);
        }

        /// <summary>
        /// The audit library's scoped client holds the port, so the port has to be answered per
        /// request as well - and by the security audit broker, which calls the security client
        /// directly rather than through another broker.
        /// </summary>
        [Fact]
        public void ShouldAnswerTheAuditUserPortWithASecurityAuditBrokerPerRequest()
        {
            // given
            IServiceCollection services = CreateServicesWithBrokers();
            services.AddHttpContextAccessor();
            using ServiceProvider serviceProvider = services.BuildServiceProvider();
            using IServiceScope firstScope = serviceProvider.CreateScope();
            using IServiceScope secondScope = serviceProvider.CreateScope();

            // when
            IAuditUserBroker firstBroker =
                firstScope.ServiceProvider.GetRequiredService<IAuditUserBroker>();

            IAuditUserBroker sameScopeBroker =
                firstScope.ServiceProvider.GetRequiredService<IAuditUserBroker>();

            IAuditUserBroker secondBroker =
                secondScope.ServiceProvider.GetRequiredService<IAuditUserBroker>();

            // then
            services.Single(descriptor => descriptor.ServiceType == typeof(IAuditUserBroker))
                .Lifetime.Should().Be(ServiceLifetime.Scoped);

            firstBroker.Should().BeOfType<SecurityAuditBroker>();
            sameScopeBroker.Should().BeSameAs(firstBroker);
            secondBroker.Should().NotBeSameAs(firstBroker);
        }

        private static IServiceCollection CreateServicesWithBrokers()
        {
            IConfiguration configuration = new ConfigurationBuilder().Build();
            var services = new ServiceCollection();
            Program.AddBrokers(services, configuration);

            return services;
        }
    }
}
