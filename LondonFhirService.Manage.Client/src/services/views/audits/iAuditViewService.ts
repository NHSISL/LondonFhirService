import type { AuditDetailView } from "../../../models/views/audits/AuditDetailView";
import type { AuditPageView } from "../../../models/views/audits/AuditPageView";

export interface IAuditViewService {
    retrieveAuditPageViewAsync(
        pageNumber: number,
        searchTerm: string,
        abortSignal?: AbortSignal): Promise<AuditPageView>;

    retrieveAuditDetailViewAsync(
        auditId: string,
        abortSignal?: AbortSignal): Promise<AuditDetailView>;
}
