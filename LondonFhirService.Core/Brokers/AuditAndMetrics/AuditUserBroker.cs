// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading.Tasks;
using LondonFhirService.Core.Abstractions.Brokers;
using LondonFhirService.Core.Brokers.Securities;

namespace LondonFhirService.Core.Brokers.AuditAndMetrics
{
    /// <summary>
    /// Satisfies the audit library's identity port from this application's security broker.
    ///
    /// SecurityAuditBroker captures the ClaimsPrincipal in its constructor rather than reading it
    /// per call, so this must be resolved per request. Registered as a singleton it would be
    /// built at startup with no HttpContext, and every audit row in the system would be stamped
    /// anonymous with nothing failing to signal it.
    /// </summary>
    public class AuditUserBroker : IAuditUserBroker
    {
        private readonly ISecurityAuditBroker securityAuditBroker;

        public AuditUserBroker(ISecurityAuditBroker securityAuditBroker) =>
            this.securityAuditBroker = securityAuditBroker;

        public async ValueTask<string> GetCurrentUserIdAsync() =>
            await this.securityAuditBroker.GetUserIdAsync() ?? string.Empty;
    }
}
