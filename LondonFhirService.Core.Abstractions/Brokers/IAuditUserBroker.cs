// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading.Tasks;

namespace LondonFhirService.Core.Abstractions.Brokers
{
    /// <summary>
    /// Who the current caller is, supplied by the hosting application. The audit library has no
    /// notion of users or of how this application authenticates, so identity arrives through a
    /// port in the same way persistence does.
    ///
    /// Read synchronously, before an audit write is dispatched to the background. The hosting
    /// implementation is free to capture the identity when it is constructed, so it must be
    /// resolved within the request it belongs to.
    /// </summary>
    public interface IAuditUserBroker
    {
        ValueTask<string> GetCurrentUserIdAsync();

        /// <summary>
        /// The caller in a form a person reading a dashboard can recognise, or an empty string
        /// outside a user request. Kept separate from the id because a metric row carries both:
        /// the id to join on, and this to read.
        /// </summary>
        ValueTask<string> GetCurrentUserDisplayNameAsync();
    }
}
