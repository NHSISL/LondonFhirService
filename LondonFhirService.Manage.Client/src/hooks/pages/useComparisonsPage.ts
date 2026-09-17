import { useCallback, useEffect, useMemo, useState } from "react";
import { useSearchParams } from "react-router-dom";
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
