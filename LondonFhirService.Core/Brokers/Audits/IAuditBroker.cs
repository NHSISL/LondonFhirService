// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.Audits;

namespace LondonFhirService.Core.Brokers.Audits
{
    public interface IAuditBroker
    {
        ValueTask BulkLogAsync(List<Audit> audits);

        ValueTask<Audit> LogAsync(
            string auditType,
            string title,
            string message,
            string fileName,
            string correlationId,
            string logLevel = "Information");

        ValueTask<Audit> LogInformationAsync(
            string auditType,
            string title,
            string message,
            string fileName,
            string correlationId);

        ValueTask<Audit> LogWarningAsync(
            string auditType,
            string title,
            string message,
            string fileName,
            string correlationId);

        ValueTask<Audit> LogErrorAsync(
            string auditType,
            string title,
            string message,
            string fileName,
            string correlationId);

        ValueTask<Audit> LogCriticalAsync(
            string auditType,
            string title,
            string message,
            string fileName,
            string correlationId);
    }
}
