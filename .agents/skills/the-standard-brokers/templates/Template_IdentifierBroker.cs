// ---
// skill: the-standard-brokers
// type: template
// source-section: "1. Brokers"
// ---

// IIdentifierBroker.cs — interface
using System;
using System.Threading.Tasks;

namespace {Namespace}.Brokers.Identifiers
{
    public interface IIdentifierBroker
    {
        ValueTask<Guid> GetIdentifierAsync();
    }
}

// IdentifierBroker.cs — implementation
using System;
using System.Threading.Tasks;

namespace {Namespace}.Brokers.Identifiers
{
    public class IdentifierBroker : IIdentifierBroker
    {
        public async ValueTask<Guid> GetIdentifierAsync() =>
            Guid.NewGuid();
    }
}
