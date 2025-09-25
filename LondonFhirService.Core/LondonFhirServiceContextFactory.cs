// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using LondonFhirService.Core.Brokers.Storages.Sql;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace LondonFhirService.Core
{
    internal class ReIdentificationContextFactory : IDesignTimeDbContextFactory<StorageBroker>
    {
        public StorageBroker CreateDbContext(string[] args)
        {
            List<KeyValuePair<string, string>> config = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(
                    key: "ConnectionStrings:LondonFhirServiceConnectionString",
                    value: "Server=(localdb)\\MSSQLLocalDB;Database=LondonFhirService;" +
                        "Trusted_Connection=True;MultipleActiveResultSets=true"),
            };

            var configurationBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(initialData: config);

            IConfiguration configuration = configurationBuilder.Build();
            return new StorageBroker(configuration);
        }
    }
}
