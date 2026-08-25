import type { AuditListItemView } from "../../views/audits/AuditListItemView";

export type AuditListProps = {
    audits: AuditListItemView[];
    selectedAuditId?: string;
};
