// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading.Tasks;
using ISL.Security.Client.Models.Foundations.Users;
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
        private readonly ISecurityBroker securityBroker;

        public AuditUserBroker(
            ISecurityAuditBroker securityAuditBroker,
            ISecurityBroker securityBroker)
        {
            this.securityAuditBroker = securityAuditBroker;
            this.securityBroker = securityBroker;
        }

        public async ValueTask<string> GetCurrentUserIdAsync() =>
            await this.securityAuditBroker.GetUserIdAsync() ?? string.Empty;

        /// <summary>
        /// Falls back to the given and family names because DisplayName is not guaranteed to be
        /// populated - a directory entry can carry the parts without the whole. An empty string
        /// rather than a null when nothing is resolvable, so callers stamping this onto a row do
        /// not have to guard it.
        /// </summary>
        public async ValueTask<string> GetCurrentUserDisplayNameAsync()
        {
            User currentUser = await this.securityBroker.GetCurrentUserAsync();

            if (currentUser is null)
            {
                return string.Empty;
            }

            if (string.IsNullOrWhiteSpace(currentUser.DisplayName) is false)
            {
                return currentUser.DisplayName;
            }

            return $"{currentUser.GivenName} {currentUser.Surname}".Trim();
        }
    }
}
