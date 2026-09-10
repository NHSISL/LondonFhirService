// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;

using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

using LondonFhirService.Core.Brokers.Storages.Sql;

namespace LondonFhirService.Core
{
    internal class ReIdentificationContextFactory : IDesignTimeDbContextFactory<StorageBroker>
    {
        public StorageBroker CreateDbContext(string[] args)
        {
            // CI (Linux) runs against a Dockerized SQL Server container and has no LocalDB, so
            // ConnectionStrings__LondonFhirServiceConnectionString - the standard .NET config
            // env var override - lets the build pipeline point this at the container without
            // touching the LocalDB default every other environment still relies on. An empty
            // (but present) value is treated as unset rather than passed through, since an empty
            // connection string would otherwise make StorageBroker construction fail outright.
            string environmentConnectionString =
                Environment.GetEnvironmentVariable("ConnectionStrings__LondonFhirServiceConnectionString");

            string connectionString = string.IsNullOrWhiteSpace(environmentConnectionString)
                ? "Server=(localdb)\\MSSQLLocalDB;Database=LondonFhirService;" +
                    "Trusted_Connection=True;MultipleActiveResultSets=true"
                : environmentConnectionString;

            var config = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(
                    key: "ConnectionStrings:LondonFhirServiceConnectionString",
                    value: connectionString),
            };

            var configurationBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(initialData: config);

            IConfiguration configuration = configurationBuilder.Build();
            return new StorageBroker(configuration);
        }
    }
}
