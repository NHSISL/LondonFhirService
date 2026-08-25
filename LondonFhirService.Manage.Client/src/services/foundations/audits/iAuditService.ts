import type { Audit } from "../../../models/foundations/audits/Audit";
import type { AuditQuery } from "../../../models/foundations/audits/AuditQuery";

export interface IAuditService {
    retrieveAuditsAsync(auditQuery: AuditQuery, abortSignal?: AbortSignal): Promise<Audit[]>;
    retrieveAuditByIdAsync(auditId: string, abortSignal?: AbortSignal): Promise<Audit>;
}
