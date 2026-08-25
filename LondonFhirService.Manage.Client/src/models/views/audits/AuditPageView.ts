import type { AuditListItemView } from "./AuditListItemView";

// One page of audit rows, plus whether the API had more to give. The audit table is a log rather
// than a small configuration table, so the list pages server side instead of loading everything.
export type AuditPageView = {
    audits: AuditListItemView[];
    hasMore: boolean;
};
