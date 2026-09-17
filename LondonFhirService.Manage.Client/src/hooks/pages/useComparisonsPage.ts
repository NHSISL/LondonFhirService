import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { useSearchParams } from "react-router-dom";
import { useInfiniteQuery, useQuery } from "@tanstack/react-query";
import { ComparisonViewService } from "../../services/views/comparisons/comparisonViewService";
import type { ComparisonListItemView } from "../../models/views/comparisons/ComparisonListItemView";
import type { ComparisonPageView } from "../../models/views/comparisons/ComparisonPageView";
import type { PendingComparisonView } from "../../models/views/comparisons/PendingComparisonView";

const searchDebounceMilliseconds = 400;

// Half the compare worker's own interval, so a comparison shows up within a tick of being written
// rather than a tick and a half. Only ever runs while waitingOnTheQueue holds - see below.
const pendingPollMilliseconds = 5000;

// How long after arriving the page keeps asking even though it has been told there is nothing
// queued. The Api answers a structured record request before the records are written - the insert
// goes onto a dispatch queue - so an operator following the correlation id straight here can beat
// their own rows to the page. Without this the first empty answer would be taken as final and the
// poll would never start.
const arrivalGraceMilliseconds = 30000;

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
        refetchInterval: () => waitingOnTheQueueRef.current ? pendingPollMilliseconds : false
    });

    const pendingComparisons = useMemo(() => pendingData ?? [], [pendingData]);
    const mountedAt = useRef<number>(Date.now());

    /**
     * Whether this page is still waiting on something, and so the one condition both queries poll
     * on. Written once rather than twice, because a page that refreshed one of its two lists and
     * not the other would be worse than one that refreshed neither.
     *
     * Three reasons to keep asking, and all three have to be here:
     *
     * A record the queue could still claim. Not merely an unprocessed one - see
     * isAwaitingTheQueue, which is what stops a permanently unclaimable row pinning this on
     * forever.
     *
     * A pending check that failed. Stopping on an error would mean one transient 502 silently
     * ends live refresh on both lists while the panel claims the comparisons below are up to date.
     *
     * The arrival window. An empty first answer is not evidence that nothing is coming, because
     * the records are written after the request that produced them was answered.
     */
    const waitingOnTheQueue =
        pendingComparisons.some(pendingComparison => pendingComparison.isAwaitingTheQueue)
        || pendingError !== null
        || Date.now() - mountedAt.current < arrivalGraceMilliseconds;

    // The pending query's own interval callback is declared above this line, so it reads the
    // decision through a ref rather than closing over a value that does not exist yet. Written on
    // every render, so the callback always sees the current answer.
    const waitingOnTheQueueRef = useRef<boolean>(true);
    waitingOnTheQueueRef.current = waitingOnTheQueue;

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

        // Refetched on the same beat as the queue, so a comparison appears here the moment the
        // worker writes it. Every loaded page refetches together, which is affordable precisely
        // because this stops once there is nothing left to wait for.
        refetchInterval: waitingOnTheQueue ? pendingPollMilliseconds : false
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
        watching: pendingFetching,
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
