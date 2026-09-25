import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { act, cleanup, renderHook, waitFor } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { afterEach, beforeEach, expect, it, vi } from "vitest";
import { useComparisonsPage } from "./useComparisonsPage";
import type { ComparisonPageView } from "../../models/views/comparisons/ComparisonPageView";
import type { PendingComparisonView } from "../../models/views/comparisons/PendingComparisonView";
import type { ReactNode } from "react";

const retrievePendingComparisonViewsAsync = vi.fn();
const retrieveComparisonPageViewAsync = vi.fn();

vi.mock("../../services/views/comparisons/comparisonViewService", () => ({
    ComparisonViewService: class {
        retrievePendingComparisonViewsAsync(searchTerm: string, abortSignal?: AbortSignal) {
            return retrievePendingComparisonViewsAsync(searchTerm, abortSignal);
        }

        retrieveComparisonPageViewAsync(
            pageNumber: number,
            searchTerm: string,
            unresolvedOnly: boolean,
            abortSignal?: AbortSignal) {
            return retrieveComparisonPageViewAsync(
                pageNumber, searchTerm, unresolvedOnly, abortSignal);
        }
    }
}));

const aPendingComparison = (
    overrides: Partial<PendingComparisonView> = {})
    : PendingComparisonView => ({
    id: "cccccccc-0000-0000-0000-000000000003",
    correlationId: "abc-123",
    sourceNameText: "DDS2",
    isPrimarySource: false,
    statusText: "Pending",
    statusClassName: "badge bg-secondary",
    landedAtText: "04 May 2026 09:29:00",
    isAwaitingTheQueue: true,
    ...overrides
});

const anEmptyPage: ComparisonPageView = { comparisons: [], hasMore: false };

// Retries off and no cache between tests, so a failure is one call and one render rather than
// three seconds of backoff.
const wrapper = ({ children }: { children: ReactNode }) => {
    const queryClient = new QueryClient({
        defaultOptions: { queries: { retry: false, gcTime: 0 } }
    });

    return (
        <MemoryRouter>
            <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
        </MemoryRouter>
    );
};

afterEach(cleanup);

beforeEach(() => {
    retrievePendingComparisonViewsAsync.mockReset();
    retrieveComparisonPageViewAsync.mockReset();
    retrievePendingComparisonViewsAsync.mockResolvedValue([]);
    retrieveComparisonPageViewAsync.mockResolvedValue(anEmptyPage);
});

/**
 * The regression this file exists for. The pending query's refetchInterval is a function, and
 * react-query evaluates it inside setOptions during the very first render - so a ref declared
 * below the query was read in its temporal dead zone and the page died with "Cannot access
 * 'waitingOnTheQueueRef' before initialization" before painting anything.
 *
 * Nothing caught it: every other test in this suite exercises the services and the components,
 * and none of them mounted this hook.
 */
it("should render without touching anything before it is initialised", async () => {
    const rendered = renderHook(() => useComparisonsPage(), { wrapper });

    expect(rendered.result.current).toBeTruthy();
    await waitFor(() => expect(retrievePendingComparisonViewsAsync).toHaveBeenCalled());
});

it("should ask for both lists on arrival", async () => {
    renderHook(() => useComparisonsPage(), { wrapper });

    await waitFor(() => expect(retrievePendingComparisonViewsAsync).toHaveBeenCalled());
    await waitFor(() => expect(retrieveComparisonPageViewAsync).toHaveBeenCalled());
});

it("should surface what is queued", async () => {
    retrievePendingComparisonViewsAsync.mockResolvedValue([aPendingComparison()]);

    const rendered = renderHook(() => useComparisonsPage(), { wrapper });

    await waitFor(() => expect(rendered.result.current.pendingComparisons).toHaveLength(1));
    expect(rendered.result.current.pendingComparisons[0].correlationId).toBe("abc-123");
});

// The pending panel failing must not take the comparisons list down with it - that separation is
// the whole reason the two errors are reported apart.
it("should keep the main list usable when the pending check fails", async () => {
    retrievePendingComparisonViewsAsync.mockRejectedValue(new Error("pending is down"));

    const rendered = renderHook(() => useComparisonsPage(), { wrapper });

    await waitFor(() => expect(rendered.result.current.pendingError).not.toBeNull());
    expect(rendered.result.current.error).toBeNull();
    expect(rendered.result.current.pendingComparisons).toEqual([]);
});

it("should seed the search box from a correlation id in the query string", async () => {
    const seeded = ({ children }: { children: ReactNode }) => {
        const queryClient = new QueryClient({
            defaultOptions: { queries: { retry: false, gcTime: 0 } }
        });

        return (
            <MemoryRouter initialEntries={["/admin/comparisons?correlationId=abc-123"]}>
                <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
            </MemoryRouter>
        );
    };

    const rendered = renderHook(() => useComparisonsPage(), { wrapper: seeded });

    expect(rendered.result.current.searchTerm).toBe("abc-123");
    await waitFor(() =>
        expect(retrievePendingComparisonViewsAsync)
            .toHaveBeenCalledWith("abc-123", expect.anything()));
});

// Advances the faked clock and lets react-query and React settle what it set off.
const advance = async (milliseconds: number) =>
    await act(async () => { await vi.advanceTimersByTimeAsync(milliseconds); });

/**
 * The arrival window is state a timer closes, not a Date.now() comparison made while rendering.
 * These pin what that window is for: records land after the request that produced them was
 * answered, so an empty first answer keeps the pending list polling for a while.
 */
it("should keep polling the pending list through the arrival window", async () => {
    vi.useFakeTimers({ toFake: ["setTimeout", "clearTimeout", "setInterval", "clearInterval"] });

    try {
        renderHook(() => useComparisonsPage(), { wrapper });
        await advance(0);
        const firstCalls = retrievePendingComparisonViewsAsync.mock.calls.length;

        await advance(5000);
        await advance(5000);

        expect(retrievePendingComparisonViewsAsync.mock.calls.length).toBeGreaterThanOrEqual(firstCalls + 2);
    } finally {
        vi.useRealTimers();
    }
});

it("should stop polling once the arrival window closes and nothing is queued", async () => {
    vi.useFakeTimers({ toFake: ["setTimeout", "clearTimeout", "setInterval", "clearInterval"] });

    try {
        renderHook(() => useComparisonsPage(), { wrapper });
        await advance(0);

        // Past the 30 second window, plus one more poll for the closed window to be read.
        await advance(31000);
        await advance(5000);
        const callsOnceClosed = retrievePendingComparisonViewsAsync.mock.calls.length;

        await advance(20000);

        expect(retrievePendingComparisonViewsAsync.mock.calls.length).toBe(callsOnceClosed);
    } finally {
        vi.useRealTimers();
    }
});
