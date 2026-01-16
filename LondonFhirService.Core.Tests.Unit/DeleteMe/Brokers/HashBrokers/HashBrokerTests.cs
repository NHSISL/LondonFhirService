// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using LondonFhirService.Core.Brokers.Hashing;
using Xunit.Abstractions;

namespace LondonFhirService.Core.Tests.Unit.DeleteMe.Brokers.HashBrokers
{
    public partial class HashBrokerTests
    {
        private readonly IHashBroker hashBroker;
        private readonly ITestOutputHelper output;

        public HashBrokerTests(ITestOutputHelper output)
        {
            this.hashBroker = new HashBroker();
            this.output = output;
        }
    }
}
