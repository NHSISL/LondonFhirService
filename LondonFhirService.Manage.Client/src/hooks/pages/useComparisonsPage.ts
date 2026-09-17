import { useCallback, useEffect, useMemo, useState } from "react";
import { useSearchParams } from "react-router-dom";
import { useInfiniteQuery, useQuery } from "@tanstack/react-query";
import { ComparisonViewService } from "../../services/views/comparisons/comparisonViewService";
import type { ComparisonListItemView } from "../../models/views/comparisons/ComparisonListItemView";
import type { ComparisonPageView } from "../../models/views/comparisons/ComparisonPageView";
import type { PendingComparisonView } from "../../models/views/comparisons/PendingComparisonView";

const searchDebounceMilliseconds = 400;

// Half the compare worker's own interval, so a comparison shows up within a tick of being written
// rather than a tick and a half. Only ever runs while something is actually queued - see below.
const pendingPollMilliseconds = 5000;

export type ComparisonsPageState = {
    comparisons: ComparisonListItemView[];
    pendingComparisons: PendingComparisonView[];
    searchTerm: string;
    unresolvedOnly: boolean;
    loading: boolean;
    loadingMore: boolean;
    searching: boolean;
    watching: boolean;
    hasNextPage: boolean;
    error: Error | null;
    pendingError: Error | null;
    handleSearchTermChange: (searchTerm: string) => void;
    handleSearchClear: () => void;
    handleUnresolvedOnlyChange: (unresolvedOnly: boolean) => void;
    handleLoadMore: () => void;
};

export function useComparisonsPage(): ComparisonsPageState {
    const comparisonViewService = useMemo(() => new ComparisonViewService(), []);
    // Seeded from the query string so another page can send an operator here already filtered -
    // the structured record screen links its correlation id in, and that id is exactly what the
    // search box below filters on. Read once, as the initial value: after that the box belongs to
    // the operator, and rewriting it from the url on every render would fight their typing.
    //
    // correlationId is the name a caller should use, because that is what they have. searchTerm
    // is still honoured: this box searches comments too, so a link that means "show me these
    // words" has somewhere to say so.
    const [searchParameters] = useSearchParams();

    const initialSearchTerm =
        searchParameters.get("correlationId")
        ?? searchParameters.get("searchTerm")
        ?? "";

    const [searchTerm, setSearchTerm] = useState<string>(initialSearchTerm);
    const [appliedSearchTerm, setAppliedSearchTerm] = useState<string>(initialSearchTerm);
    const [unresolvedOnly, setUnresolvedOnly] = useState<boolean>(false);

    // Comparisons accumulate a row per compared correlation, so every keystroke would otherwise be
    // a query against the whole table. Settle first, then ask the API once.
    useEffect(() => {
        if (searchTerm === appliedSearchTerm) {
            return;
        }

        const timeoutId = window.setTimeout(
            () => setAppliedSearchTerm(searchTerm),
            searchDebounceMilliseconds);

        return () => window.clearTimeout(timeoutId);
    }, [searchTerm, appliedSearchTerm]);

    // What has landed but not been compared. Polled, because the thing an operator is waiting for
    // happens on a worker somewhere else and nothing pushes it here.
    //
    // The poll is conditional on there being something to wait for, which is what stops this being
    // a page that talks to the server forever. An empty queue means the screen is showing a
    // finished state, and a finished state does not change on its own.
    const {
        data: pendingData,
        error: pendingError,
        isFetching: pendingFetching
    } = useQuery<PendingComparisonView[]>({
        queryKey: ["PendingComparisonViews", appliedSearchTerm],
        queryFn: async ({ signal }) =>
            await comparisonViewService.retrievePendingComparisonViewsAsync(
                appliedSearchTerm,
                signal),
        refetchInterval: query =>
            (query.state.data?.length ?? 0) > 0 ? pendingPollMilliseconds : false
    });

    const pendingComparisons = useMemo(() => pendingData ?? [], [pendingData]);

    const {
        data,
        isLoading,
        isFetchingNextPage,
        hasNextPage,
        fetchNextPage,
        error
    } = useInfiniteQuery<ComparisonPageView>({
        queryKey: ["ComparisonPageViews", appliedSearchTerm, unresolvedOnly],
        initialPageParam: 0,
        queryFn: async ({ pageParam, signal }) =>
            await comparisonViewService.retrieveComparisonPageViewAsync(
                pageParam as number,
                appliedSearchTerm,
                unresolvedOnly,
                signal),
        getNextPageParam: (lastPage, allPages) =>
            lastPage.hasMore ? allPages.length : undefined,

        // Refetched on the same beat as the queue, and only while the queue has something in it,
        // so a comparison appears here the moment the worker writes it. Every loaded page refetches
        // together, which is affordable precisely because this stops as soon as the queue drains.
        refetchInterval: pendingComparisons.length > 0 ? pendingPollMilliseconds : false
    });

    const comparisons = useMemo(
        () => (data?.pages ?? []).flatMap(page => page.comparisons),
        [data]);

    const handleSearchTermChange = useCallback(
        (nextSearchTerm: string) => setSearchTerm(nextSearchTerm),
        []);

    const handleSearchClear = useCallback(() => setSearchTerm(""), []);

    const handleUnresolvedOnlyChange = useCallback(
        (nextUnresolvedOnly: boolean) => setUnresolvedOnly(nextUnresolvedOnly),
        []);

    const handleLoadMore = useCallback(() => {
        if (hasNextPage && isFetchingNextPage === false) {
            fetchNextPage();
        }
    }, [hasNextPage, isFetchingNextPage, fetchNextPage]);

    return {
        comparisons: comparisons,
        pendingComparisons: pendingComparisons,
        searchTerm: searchTerm,
        unresolvedOnly: unresolvedOnly,
        loading: isLoading,
        loadingMore: isFetchingNextPage,
        searching: searchTerm !== appliedSearchTerm,
        watching: pendingComparisons.length > 0 && pendingFetching,
        hasNextPage: hasNextPage === true,
        error: error,

        // Kept apart from error. The comparisons list is what this page is for, and it should not
        // be replaced by an error summary because the secondary "still waiting" panel could not
        // be filled.
        pendingError: pendingError,
        handleSearchTermChange: handleSearchTermChange,
        handleSearchClear: handleSearchClear,
        handleUnresolvedOnlyChange: handleUnresolvedOnlyChange,
        handleLoadMore: handleLoadMore
    };
}
