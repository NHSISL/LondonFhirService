// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.IO;
using LondonFhirService.Core.Brokers.Hashing;
using LondonFhirService.Core.Models.Orchestrations.Accesses;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace LondonFhirService.Core.Tests.Unit.DeleteMe.Brokers.HashBrokers
{
    public partial class HashBrokerTests
    {
        private readonly IHashBroker hashBroker;
        private readonly ITestOutputHelper output;
        private readonly IConfiguration configuration;
        private readonly AccessConfigurations accessConfigurations;

        public HashBrokerTests(ITestOutputHelper output)
        {
            this.hashBroker = new HashBroker();
            this.output = output;

            var testProjectPath =
                Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));

            this.configuration = new ConfigurationBuilder()
                .SetBasePath(testProjectPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            accessConfigurations = configuration
                .GetSection("AccessConfigurations")
                .Get<AccessConfigurations>();
        }
    }
}
