// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Clients.AuditClient.Exceptions;
using LondonFhirService.Core.Models.Foundations.Audits;
using LondonFhirService.Core.Models.Foundations.Audits.Exceptions;
using LondonFhirService.Core.Services.Foundations.Audits;
using Xeptions;

namespace LondonFhirService.Core.Clients.Audits
{
    public class AuditClient : IAuditClient
    {
        private readonly IAuditService auditService;

        public AuditClient(IAuditService auditService) =>
            this.auditService = auditService;

        public async ValueTask<Audit> LogAuditAsync(
            string auditType,
            string title,
            string message,
            string fileName,
            string correlationId,
            string logLevel = "Information")
        {
            try
            {
                return await auditService
                    .AddAuditAsync(auditType, title, message, fileName, correlationId, logLevel);
            }
            catch (AuditServiceValidationException auditValidationException)
            {
                throw new AuditClientValidationException(
                    message: "Audit client validation error occurred, fix errors and try again.",
                    innerException: auditValidationException.InnerException as Xeption);
            }
            catch (AuditServiceDependencyValidationException auditDependencyValidationException)
            {
                throw new AuditClientValidationException(
                    message: "Audit client validation error occurred, fix errors and try again.",
                    innerException: auditDependencyValidationException.InnerException as Xeption);
            }
            catch (AuditServiceDependencyException auditDependencyException)
            {
                throw new AuditClientDependencyException(
                    message: "Audit client dependency error occurred, please contact support.",
                    innerException: auditDependencyException.InnerException as Xeption);
            }
            catch (AuditServiceException auditServiceException)
            {
                throw new AuditClientServiceException(
                    message: "Audit client service error occurred, fix errors and try again.",
                    auditServiceException.InnerException as Xeption);
            }
        }

        public async ValueTask BulkLogAuditsAsync(List<Audit> audits)
        {
            try
            {
                await auditService.BulkAddAuditsAsync(audits);
            }
            catch (AuditServiceValidationException auditValidationException)
            {
                throw new AuditClientValidationException(
                    message: "Audit client validation error occurred, fix errors and try again.",
                    innerException: auditValidationException.InnerException as Xeption);
            }
            catch (AuditServiceDependencyValidationException auditDependencyValidationException)
            {
                throw new AuditClientValidationException(
                    message: "Audit client validation error occurred, fix errors and try again.",
                    innerException: auditDependencyValidationException.InnerException as Xeption);
            }
            catch (AuditServiceDependencyException auditDependencyException)
            {
                throw new AuditClientDependencyException(
                    message: "Audit client dependency error occurred, please contact support.",
                    innerException: auditDependencyException.InnerException as Xeption);
            }
            catch (AuditServiceException auditServiceException)
            {
                throw new AuditClientServiceException(
                    message: "Audit client service error occurred, fix errors and try again.",
                    auditServiceException.InnerException as Xeption);
            }
        }
    }
}
