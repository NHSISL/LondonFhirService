// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using LondonFhirService.Core.Services.Foundations.Audits;
using LondonFhirService.Core.Models.Foundations.Audits;

namespace LondonFhirService.Core.Brokers.Audits
{
    public class AuditBroker : IAuditBroker
    {
        // Calls the service directly rather than going through IAuditClient. The client now
        // ships in LondonFhirService.Clients.AuditAndMetrics, which references Core, so Core
        // cannot reference it back. Nothing was catching the client exceptions this hop used to
        // produce, so what escapes here is now the service exception underneath them.
        private readonly IAuditService auditService;

        public AuditBroker(IAuditService auditService) =>
            this.auditService = auditService;

        public async ValueTask BulkLogAsync(List<Audit> audits) =>
            await auditService.BulkAddAuditsAsync(audits);

        public async ValueTask<Audit> LogAsync(
            string auditType,
            string title,
            string message,
            string fileName,
            string correlationId,
            string logLevel = "Information")
        {
            return await auditService.AddAuditAsync(auditType, title, message, fileName, correlationId, logLevel);
        }

        public async ValueTask<Audit> LogInformationAsync(
            string auditType,
            string title,
            string message,
            string fileName,
            string correlationId)
        {
            return await auditService.AddAuditAsync(auditType, title, message, fileName, correlationId, "Information");
        }

        public async ValueTask<Audit> LogWarningAsync(
            string auditType,
            string title,
            string message,
            string fileName,
            string correlationId)
        {
            return await auditService.AddAuditAsync(auditType, title, message, fileName, correlationId, "Warning");
        }

        public async ValueTask<Audit> LogErrorAsync(
            string auditType,
            string title,
            string message,
            string fileName,
            string correlationId)
        {
            return await auditService.AddAuditAsync(auditType, title, message, fileName, correlationId, "Error");
        }

        public async ValueTask<Audit> LogCriticalAsync(
            string auditType,
            string title,
            string message,
            string fileName,
            string correlationId)
        {
            return await auditService.AddAuditAsync(auditType, title, message, fileName, correlationId, "Critical");
        }
    }
}
