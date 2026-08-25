import { useCallback, useMemo } from "react";
import { useQuery } from "@tanstack/react-query";
import { useNavigate } from "react-router-dom";
import { MetricViewService } from "../../services/views/metrics/metricViewService";
import type { MetricCorrelationView } from "../../models/views/metrics/MetricCorrelationView";

export type MetricDetailPageState = {
    correlation: MetricCorrelationView | null;
    loading: boolean;
    error: Error | null;
    handleBackToMetrics: () => void;
};

export function useMetricDetailPage(correlationId: string): MetricDetailPageState {
    const metricViewService = useMemo(() => new MetricViewService(), []);
    const navigate = useNavigate();
    const hasCorrelationId = correlationId.trim().length > 0;

    const { data, isLoading, error } = useQuery<MetricCorrelationView>({
        queryKey: ["MetricCorrelationView", correlationId],
        queryFn: async ({ signal }) =>
            await metricViewService.retrieveMetricCorrelationViewAsync(correlationId, signal),
        enabled: hasCorrelationId
    });

    const handleBackToMetrics = useCallback(() => navigate("/admin/metrics"), [navigate]);

    return {
        correlation: data ?? null,
        loading: isLoading,
        error: error,
        handleBackToMetrics: handleBackToMetrics
    };
}
