import type { Audit } from "../../models/foundations/audits/Audit";
import type { AuditQuery } from "../../models/foundations/audits/AuditQuery";

export interface IAuditApiBroker {
    getAuditsAsync(auditQuery: AuditQuery, abortSignal?: AbortSignal): Promise<Audit[]>;
    getAuditByIdAsync(auditId: string, abortSignal?: AbortSignal): Promise<Audit>;
}
