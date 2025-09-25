// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using LondonDataServices.IDecide.Manage.Tests.Integration.Brokers;

namespace LondonFhirService.Manage.Tests.Integration.Brokers
{
    [CollectionDefinition(nameof(ApiTestCollection))]
    public class ApiTestCollection : ICollectionFixture<ApiBroker>
    { }
}
