// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using LondonFhirService.Core.Clients.Audits;
using LondonFhirService.Core.Models.Foundations.Audits;

namespace LondonFhirService.Core.Brokers.Audits
{
    public class AuditBroker : IAuditBroker
    {
        private readonly IAuditClient auditClient;

        public AuditBroker(IAuditClient auditClient) =>
            this.auditClient = auditClient;

        public async ValueTask BulkLogAsync(List<Audit> audits) =>
            await auditClient.BulkLogAuditsAsync(audits);

        public async ValueTask<Audit> LogAsync(
            string auditType,
            string title,
            string message,
            string fileName,
            string correlationId,
            string logLevel = "Information")
        {
            return await auditClient.LogAuditAsync(auditType, title, message, fileName, correlationId, logLevel);
        }

        public async ValueTask<Audit> LogInformationAsync(
            string auditType,
            string title,
            string message,
            string fileName,
            string correlationId)
        {
            return await auditClient.LogAuditAsync(auditType, title, message, fileName, correlationId, "Information");
        }

        public async ValueTask<Audit> LogWarningAsync(
            string auditType,
            string title,
            string message,
            string fileName,
            string correlationId)
        {
            return await auditClient.LogAuditAsync(auditType, title, message, fileName, correlationId, "Warning");
        }

        public async ValueTask<Audit> LogErrorAsync(
            string auditType,
            string title,
            string message,
            string fileName,
            string correlationId)
        {
            return await auditClient.LogAuditAsync(auditType, title, message, fileName, correlationId, "Error");
        }

        public async ValueTask<Audit> LogCriticalAsync(
            string auditType,
            string title,
            string message,
            string fileName,
            string correlationId)
        {
            return await auditClient.LogAuditAsync(auditType, title, message, fileName, correlationId, "Critical");
        }
    }
}
