import { useCallback, useEffect, useMemo, useState } from "react";
import { useInfiniteQuery, useQuery } from "@tanstack/react-query";
import { MetricViewService } from "../../services/views/metrics/metricViewService";
import type { MetricAveragesView } from "../../models/views/metrics/MetricAveragesView";
import type { MetricFilter } from "../../models/foundations/metrics/MetricFilter";
import type { MetricListItemView } from "../../models/views/metrics/MetricListItemView";
import type { MetricPageView } from "../../models/views/metrics/MetricPageView";

export type MetricsPageState = {
    metrics: MetricListItemView[];
    averages: MetricAveragesView | null;
    filter: MetricFilter;
    correlationIdIsIncomplete: boolean;
    searching: boolean;
    handleFilterChange: (fieldName: keyof MetricFilter, value: string) => void;
    handleFilterClear: () => void;
    loading: boolean;
    loadingMore: boolean;
    hasNextPage: boolean;
    error: Error | null;
    handleLoadMore: () => void;
};

const searchDebounceMilliseconds = 400;

export function useMetricsPage(): MetricsPageState {
    const metricViewService = useMemo(() => new MetricViewService(), []);

    const [filter, setFilter] =
        useState<MetricFilter>(() => metricViewService.createMetricFilter());

    // A part typed correlation id is not a filter yet - sending it would be rejected by the API -
    // so it is held back until it is complete, and the page says so rather than showing no rows.
    const correlationIdIsIncomplete =
        filter.correlationId.trim().length > 0
        && metricViewService.isSearchableCorrelationId(filter.correlationId) === false;

    const appliedFilterSource = useMemo<MetricFilter>(() => ({
        correlationId: correlationIdIsIncomplete ? "" : filter.correlationId.trim(),
        fromDate: filter.fromDate,
        toDate: filter.toDate
    }), [filter, correlationIdIsIncomplete]);

    const [appliedFilter, setAppliedFilter] = useState<MetricFilter>(appliedFilterSource);

    // Settle before querying: the metrics table is a log, and every keystroke would otherwise be
    // two queries against it.
    useEffect(() => {
        const isSame = appliedFilter.correlationId === appliedFilterSource.correlationId
            && appliedFilter.fromDate === appliedFilterSource.fromDate
            && appliedFilter.toDate === appliedFilterSource.toDate;

        if (isSame) {
            return;
        }

        const timeoutId = window.setTimeout(
            () => setAppliedFilter(appliedFilterSource),
            searchDebounceMilliseconds);

        return () => window.clearTimeout(timeoutId);
    }, [appliedFilterSource, appliedFilter]);

    const {
        data,
        isLoading,
        isFetchingNextPage,
        hasNextPage,
        fetchNextPage,
        error
    } = useInfiniteQuery<MetricPageView>({
        queryKey: ["MetricPageViews", appliedFilter],
        initialPageParam: 0,
        queryFn: async ({ pageParam, signal }) =>
            await metricViewService.retrieveMetricPageViewAsync(
                pageParam as number,
                appliedFilter,
                signal),
        getNextPageParam: (lastPage, allPages) =>
            lastPage.hasMore ? allPages.length : undefined
    });

    // Deliberately a fixed sample rather than an average of whatever is on screen: scrolling the
    // list would otherwise quietly change the headline figures.
    const { data: averages } = useQuery<MetricAveragesView>({
        queryKey: ["MetricAveragesView", appliedFilter],
        queryFn: async ({ signal }) =>
            await metricViewService.retrieveMetricAveragesViewAsync(appliedFilter, signal)
    });

    const metrics = useMemo(
        () => (data?.pages ?? []).flatMap(page => page.metrics),
        [data]);

    const handleFilterChange = useCallback(
        (fieldName: keyof MetricFilter, value: string) =>
            setFilter(currentFilter => ({ ...currentFilter, [fieldName]: value })),
        []);

    const handleFilterClear = useCallback(
        () => setFilter(metricViewService.createMetricFilter()),
        [metricViewService]);

    const handleLoadMore = useCallback(() => {
        if (hasNextPage && isFetchingNextPage === false) {
            fetchNextPage();
        }
    }, [hasNextPage, isFetchingNextPage, fetchNextPage]);

    return {
        metrics: metrics,
        averages: averages ?? null,
        filter: filter,
        correlationIdIsIncomplete: correlationIdIsIncomplete,
        searching: appliedFilter !== appliedFilterSource
            && (appliedFilter.correlationId !== appliedFilterSource.correlationId
                || appliedFilter.fromDate !== appliedFilterSource.fromDate
                || appliedFilter.toDate !== appliedFilterSource.toDate),
        handleFilterChange: handleFilterChange,
        handleFilterClear: handleFilterClear,
        loading: isLoading,
        loadingMore: isFetchingNextPage,
        hasNextPage: hasNextPage === true,
        error: error,
        handleLoadMore: handleLoadMore
    };
}
