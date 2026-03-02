// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using LondonFhirService.Api.Tests.Integration.Brokers.Authentications;

namespace LondonFhirService.Api.Tests.Integration.Brokers
{
    [CollectionDefinition(nameof(AuthApiTestCollection))]
    public class AuthApiTestCollection : ICollectionFixture<AuthApiBroker>
    { }
}
