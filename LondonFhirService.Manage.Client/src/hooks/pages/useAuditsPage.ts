import { useCallback, useEffect, useMemo, useState } from "react";
import { useInfiniteQuery } from "@tanstack/react-query";
import { AuditViewService } from "../../services/views/audits/auditViewService";
import type { AuditListItemView } from "../../models/views/audits/AuditListItemView";
import type { AuditPageView } from "../../models/views/audits/AuditPageView";

const searchDebounceMilliseconds = 400;

export type AuditsPageState = {
    audits: AuditListItemView[];
    searchTerm: string;
    loading: boolean;
    loadingMore: boolean;
    searching: boolean;
    hasNextPage: boolean;
    error: Error | null;
    handleSearchTermChange: (searchTerm: string) => void;
    handleSearchClear: () => void;
    handleLoadMore: () => void;
};

export function useAuditsPage(): AuditsPageState {
    const auditViewService = useMemo(() => new AuditViewService(), []);
    const [searchTerm, setSearchTerm] = useState<string>("");
    const [appliedSearchTerm, setAppliedSearchTerm] = useState<string>("");

    // The audit table is a log, so every keystroke would otherwise be a query against it. Settle
    // first, then ask the API once.
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
    } = useInfiniteQuery<AuditPageView>({
        queryKey: ["AuditPageViews", appliedSearchTerm],
        initialPageParam: 0,
        queryFn: async ({ pageParam, signal }) =>
            await auditViewService.retrieveAuditPageViewAsync(
                pageParam as number,
                appliedSearchTerm,
                signal),
        getNextPageParam: (lastPage, allPages) =>
            lastPage.hasMore ? allPages.length : undefined
    });

    const audits = useMemo(
        () => (data?.pages ?? []).flatMap(page => page.audits),
        [data]);

    const handleSearchTermChange = useCallback(
        (nextSearchTerm: string) => setSearchTerm(nextSearchTerm),
        []);

    const handleSearchClear = useCallback(() => setSearchTerm(""), []);

    const handleLoadMore = useCallback(() => {
        if (hasNextPage && isFetchingNextPage === false) {
            fetchNextPage();
        }
    }, [hasNextPage, isFetchingNextPage, fetchNextPage]);

    return {
        audits: audits,
        searchTerm: searchTerm,
        loading: isLoading,
        loadingMore: isFetchingNextPage,
        searching: searchTerm !== appliedSearchTerm,
        hasNextPage: hasNextPage === true,
        error: error,
        handleSearchTermChange: handleSearchTermChange,
        handleSearchClear: handleSearchClear,
        handleLoadMore: handleLoadMore
    };
}
