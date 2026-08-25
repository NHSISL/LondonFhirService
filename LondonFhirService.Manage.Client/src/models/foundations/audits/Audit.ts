// Foundation model - mirrors LondonFhirService.Core.Models.Foundations.Audits.Audit as the
// /api/audits endpoint serialises it. CorrelationId and FileName have no server side default, so
// they can come back null.
export type Audit = {
    id: string;
    correlationId: string | null;
    auditType: string;
    title: string;
    message: string;
    fileName: string | null;
    logLevel: string;
    createdBy: string;
    createdDate: string;
    updatedBy: string;
    updatedDate: string;
};
