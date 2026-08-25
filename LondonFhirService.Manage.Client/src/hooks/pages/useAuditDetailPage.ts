import { useCallback, useMemo } from "react";
import { useQuery } from "@tanstack/react-query";
import { useNavigate } from "react-router-dom";
import { AuditViewService } from "../../services/views/audits/auditViewService";
import type { AuditDetailView } from "../../models/views/audits/AuditDetailView";

export type AuditDetailPageState = {
    audit: AuditDetailView | null;
    loading: boolean;
    error: Error | null;
    handleBackToAudits: () => void;
};

export function useAuditDetailPage(auditId: string): AuditDetailPageState {
    const auditViewService = useMemo(() => new AuditViewService(), []);
    const navigate = useNavigate();
    const hasAuditId = auditId.trim().length > 0;

    const { data, isLoading, error } = useQuery<AuditDetailView>({
        queryKey: ["AuditDetailView", auditId],
        queryFn: async ({ signal }) =>
            await auditViewService.retrieveAuditDetailViewAsync(auditId, signal),
        enabled: hasAuditId
    });

    const handleBackToAudits = useCallback(() => navigate("/admin/audits"), [navigate]);

    return {
        audit: data ?? null,
        loading: isLoading,
        error: error,
        handleBackToAudits: handleBackToAudits
    };
}
