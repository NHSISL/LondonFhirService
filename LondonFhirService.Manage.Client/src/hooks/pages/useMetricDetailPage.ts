import { useCallback, useMemo } from "react";
import { useQuery } from "@tanstack/react-query";
import { useNavigate } from "react-router-dom";
import { MetricViewService } from "../../services/views/metrics/metricViewService";
import { buildComparisonsUrl } from "../../helpers/correlationIds";
import type { MetricCorrelationView } from "../../models/views/metrics/MetricCorrelationView";

export type MetricDetailPageState = {
    correlation: MetricCorrelationView | null;
    loading: boolean;
    error: Error | null;

    // The same request on the comparisons screen. Built here rather than in the page, because the
    // two screens spell the correlation id differently and choosing between those spellings is a
    // transformation - see helpers/correlationIds.
    comparisonsUrl: string;
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

        // Blank rather than a route with no id, which would land on the unfiltered comparisons
        // table and read as a result.
        comparisonsUrl: hasCorrelationId ? buildComparisonsUrl(correlationId) : "",
        handleBackToMetrics: handleBackToMetrics
    };
}
