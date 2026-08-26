import { useCallback, useEffect, useMemo, useState } from "react";
import { useInfiniteQuery } from "@tanstack/react-query";
import { ComparisonViewService } from "../../services/views/comparisons/comparisonViewService";
import type { ComparisonListItemView } from "../../models/views/comparisons/ComparisonListItemView";
import type { ComparisonPageView } from "../../models/views/comparisons/ComparisonPageView";

const searchDebounceMilliseconds = 400;

export type ComparisonsPageState = {
    comparisons: ComparisonListItemView[];
    searchTerm: string;
    unresolvedOnly: boolean;
    loading: boolean;
    loadingMore: boolean;
    searching: boolean;
    hasNextPage: boolean;
    error: Error | null;
    handleSearchTermChange: (searchTerm: string) => void;
    handleSearchClear: () => void;
    handleUnresolvedOnlyChange: (unresolvedOnly: boolean) => void;
    handleLoadMore: () => void;
};

export function useComparisonsPage(): ComparisonsPageState {
    const comparisonViewService = useMemo(() => new ComparisonViewService(), []);
    const [searchTerm, setSearchTerm] = useState<string>("");
    const [appliedSearchTerm, setAppliedSearchTerm] = useState<string>("");
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
            lastPage.hasMore ? allPages.length : undefined
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
        searchTerm: searchTerm,
        unresolvedOnly: unresolvedOnly,
        loading: isLoading,
        loadingMore: isFetchingNextPage,
        searching: searchTerm !== appliedSearchTerm,
        hasNextPage: hasNextPage === true,
        error: error,
        handleSearchTermChange: handleSearchTermChange,
        handleSearchClear: handleSearchClear,
        handleUnresolvedOnlyChange: handleUnresolvedOnlyChange,
        handleLoadMore: handleLoadMore
    };
}
