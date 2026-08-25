import { expect, it } from "vitest";
import { MetricViewService, metricPageSize } from "./metricViewService";
import type { IMetricService } from "../../foundations/metrics/iMetricService";
import type { Metric } from "../../../models/foundations/metrics/Metric";
import type { MetricQuery } from "../../../models/foundations/metrics/MetricQuery";

const correlationId = "0f1c4d6b-9a2e-4f31-8c77-1b2a3c4d5e6f";

const createMetric = (overrides: Partial<Metric>): Metric => ({
    id: "11111111-1111-1111-1111-111111111111",
    userId: "528f3bb2-27b4-40d8-a694-5e78bfd3480e",
    parentId: null,
    correlationId: correlationId,
    method: "STU3-Patient-GetStructuredRecord",
    type: 0,
    name: "Coordination",
    target: null,
    started: "2026-08-25T16:14:00+00:00",
    completed: "2026-08-25T16:14:01+00:00",
    durationMs: 1000,
    status: 0,
    errorCode: null,
    payloadBytes: null,
    consumer: "a-consumer",
    description: null,
    createdDate: "2026-08-25T16:14:01+00:00",
    ...overrides
});

const noFilter = { correlationId: "", fromDate: "", toDate: "" };

const createMetricService = (overrides: Partial<IMetricService> = {}): IMetricService => ({
    retrieveRequestMetricsAsync: async () => [],
    retrieveProviderRequestsMetricsAsync: async () => [],
    retrieveMetricsByCorrelationIdAsync: async () => [],
    ...overrides
});

it("should page the request list and map a row for display", async () => {
    let captured: MetricQuery | null = null;

    const metricViewService = new MetricViewService(createMetricService({
        retrieveRequestMetricsAsync: async metricQuery => {
            captured = metricQuery;

            return [createMetric({ status: 1, durationMs: 1500 })];
        }
    }));

    const metricPageView = await metricViewService.retrieveMetricPageViewAsync(2, noFilter);

    const metricQuery = captured as unknown as MetricQuery;
    expect(metricQuery.skip).toBe(2 * metricPageSize);
    expect(metricQuery.take).toBe(metricPageSize);

    expect(metricPageView.metrics[0].statusText).toBe("Failed");
    expect(metricPageView.metrics[0].statusClassName).toBe("badge bg-danger");
    expect(metricPageView.metrics[0].durationText).toBe("1.50 s");
    expect(metricPageView.metrics[0].detailUrl).toBe(`/admin/metrics/${correlationId}`);
});

it("should report more pages only when the page came back full", async () => {
    const fullPage = Array.from({ length: metricPageSize }, () => createMetric({}));

    const withFullPage = new MetricViewService(createMetricService({
        retrieveRequestMetricsAsync: async () => fullPage
    }));

    const withShortPage = new MetricViewService(createMetricService({
        retrieveRequestMetricsAsync: async () => [createMetric({})]
    }));

    expect((await withFullPage.retrieveMetricPageViewAsync(0, noFilter)).hasMore).toBe(true);
    expect((await withShortPage.retrieveMetricPageViewAsync(0, noFilter)).hasMore).toBe(false);
});

it("should keep paging a correlation until the api stops filling a page", async () => {
    const firstPage = Array.from({ length: metricPageSize }, (_unused, index) =>
        createMetric({ id: `first-${index}` }));

    const secondPage = [createMetric({ id: "second-0" })];
    const requestedSkips: number[] = [];

    const metricViewService = new MetricViewService(createMetricService({
        retrieveMetricsByCorrelationIdAsync: async (_correlationId, metricQuery) => {
            requestedSkips.push(metricQuery.skip);

            return metricQuery.skip === 0 ? firstPage : secondPage;
        }
    }));

    const correlationView =
        await metricViewService.retrieveMetricCorrelationViewAsync(correlationId);

    expect(requestedSkips).toEqual([0, metricPageSize]);
    expect(correlationView.spanCount).toBe(metricPageSize + 1);
});

it("should map the span ordinals to names and indent by parent", async () => {
    const spans = [
        createMetric({ id: "root", parentId: null, type: 0 }),
        createMetric({ id: "orchestration", parentId: "root", type: 1 }),
        createMetric({ id: "provider", parentId: "orchestration", type: 7, target: "https://p" }),
        createMetric({ id: "call", parentId: "provider", type: 8, payloadBytes: 2048 })
    ];

    const metricViewService = new MetricViewService(createMetricService({
        retrieveMetricsByCorrelationIdAsync: async () => spans
    }));

    const correlationView =
        await metricViewService.retrieveMetricCorrelationViewAsync(correlationId);

    expect(correlationView.spans.map(span => span.typeText))
        .toEqual(["Request", "Orchestration", "Provider", "ProviderCall"]);

    expect(correlationView.spans.map(span => span.depth)).toEqual([0, 1, 2, 3]);
    expect(correlationView.spans[3].payloadText).toBe("2.0 KB");
    expect(correlationView.methodText).toBe("STU3-Patient-GetStructuredRecord");
});

it("should render a sub millisecond span without collapsing it to zero", async () => {
    const metricViewService = new MetricViewService(createMetricService({
        retrieveRequestMetricsAsync: async () => [createMetric({ durationMs: 0.42 })]
    }));

    const metricPageView = await metricViewService.retrieveMetricPageViewAsync(0, noFilter);

    expect(metricPageView.metrics[0].durationText).toBe("0.42 ms");
});

it("should survive a cycle in the parent chain", async () => {
    const spans = [
        createMetric({ id: "a", parentId: "b" }),
        createMetric({ id: "b", parentId: "a" })
    ];

    const metricViewService = new MetricViewService(createMetricService({
        retrieveMetricsByCorrelationIdAsync: async () => spans
    }));

    const correlationView =
        await metricViewService.retrieveMetricCorrelationViewAsync(correlationId);

    expect(correlationView.spanCount).toBe(2);
});

it("should wrap a foundation service failure in a view service exception", async () => {
    const metricViewService = new MetricViewService(createMetricService({
        retrieveRequestMetricsAsync: async () => { throw new Error("dependency down"); }
    }));

    await expect(metricViewService.retrieveMetricPageViewAsync(0, noFilter))
        .rejects.toThrowError(
            "We could not load the request metrics, please try again or contact support.");
});

it("should lay the timeline out against the whole correlation window", async () => {
    // A request of 100ms, with a child starting 20ms in and running 30ms.
    const spans = [
        createMetric({
            id: "root",
            parentId: null,
            started: "2026-08-25T19:47:21.000+00:00",
            durationMs: 100
        }),
        createMetric({
            id: "child",
            parentId: "root",
            started: "2026-08-25T19:47:21.020+00:00",
            durationMs: 30
        })
    ];

    const metricViewService = new MetricViewService(createMetricService({
        retrieveMetricsByCorrelationIdAsync: async () => spans
    }));

    const correlationView =
        await metricViewService.retrieveMetricCorrelationViewAsync(correlationId);

    const [root, child] = correlationView.spans;

    expect(root.offsetPercent).toBe(0);
    expect(root.widthPercent).toBe(100);
    expect(child.offsetPercent).toBeCloseTo(20, 5);
    expect(child.widthPercent).toBeCloseTo(30, 5);
    expect(child.offsetText).toBe("+20 ms");
    expect(correlationView.windowText).toBe("100 ms");
});

it("should widen the window for a deferred span that outlives the request", async () => {
    // Persist is dispatched to a background queue, so it starts as its parent finishes and can
    // end after the request span does. Scaling to the request alone would push it off the track.
    const spans = [
        createMetric({
            id: "root",
            parentId: null,
            started: "2026-08-25T19:47:21.000+00:00",
            durationMs: 100
        }),
        createMetric({
            id: "persist",
            parentId: "root",
            type: 9,
            started: "2026-08-25T19:47:21.100+00:00",
            durationMs: 100
        })
    ];

    const metricViewService = new MetricViewService(createMetricService({
        retrieveMetricsByCorrelationIdAsync: async () => spans
    }));

    const correlationView =
        await metricViewService.retrieveMetricCorrelationViewAsync(correlationId);

    const [root, persist] = correlationView.spans;

    expect(correlationView.windowText).toBe("200 ms");
    expect(root.widthPercent).toBeCloseTo(50, 5);
    expect(persist.offsetPercent).toBeCloseTo(50, 5);
    expect(persist.widthPercent).toBeCloseTo(50, 5);

    // Nothing may spill past the right hand edge of the track.
    correlationView.spans.forEach(span =>
        expect(span.offsetPercent + span.widthPercent).toBeLessThanOrEqual(100.0001));
});

it("should keep a sub millisecond span visible on the timeline", async () => {
    const spans = [
        createMetric({
            id: "root",
            parentId: null,
            started: "2026-08-25T19:47:21.000+00:00",
            durationMs: 1000
        }),
        createMetric({
            id: "consolidation",
            parentId: "root",
            type: 10,
            started: "2026-08-25T19:47:21.500+00:00",
            durationMs: 0.01
        })
    ];

    const metricViewService = new MetricViewService(createMetricService({
        retrieveMetricsByCorrelationIdAsync: async () => spans
    }));

    const correlationView =
        await metricViewService.retrieveMetricCorrelationViewAsync(correlationId);

    const consolidation = correlationView.spans[1];

    // 0.01ms of 1000ms is a thousandth of a percent - a bar of that width draws nothing.
    expect(consolidation.widthPercent).toBeGreaterThan(0.3);
    expect(consolidation.durationText).toBe("0.01 ms");
});

it("should give a single instantaneous span the whole track rather than dividing by zero", async () => {
    const spans = [createMetric({ id: "only", parentId: null, durationMs: 0 })];

    const metricViewService = new MetricViewService(createMetricService({
        retrieveMetricsByCorrelationIdAsync: async () => spans
    }));

    const correlationView =
        await metricViewService.retrieveMetricCorrelationViewAsync(correlationId);

    expect(correlationView.spans[0].offsetPercent).toBe(0);
    expect(correlationView.spans[0].widthPercent).toBe(100);
    expect(Number.isNaN(correlationView.spans[0].widthPercent)).toBe(false);
});

it("should name provider spans on the timeline and leave the other kinds alone", async () => {
    // A fan out draws one Provider row per provider. Without the name they are identical rows.
    const spans = [
        createMetric({ id: "root", parentId: null, type: 0, name: "GetStructuredRecordSerialised" }),
        createMetric({ id: "fanout", parentId: "root", type: 6, name: "Parallel provider execution" }),
        createMetric({ id: "dds", parentId: "fanout", type: 7, name: "DDS" }),
        createMetric({ id: "dds1", parentId: "fanout", type: 7, name: "DDS1" }),
        createMetric({ id: "persist", parentId: "dds", type: 9, name: "DDS" })
    ];

    const metricViewService = new MetricViewService(createMetricService({
        retrieveMetricsByCorrelationIdAsync: async () => spans
    }));

    const correlationView =
        await metricViewService.retrieveMetricCorrelationViewAsync(correlationId);

    expect(correlationView.spans.map(span => span.labelText)).toEqual([
        "Request",
        "ProviderFanOut",
        "Provider - DDS",
        "Provider - DDS1",
        "Persist"
    ]);

    // The name is still carried separately, which is what the bar tooltip shows.
    expect(correlationView.spans[4].nameText).toBe("DDS");
});

it("should not leave a dangling separator when a provider span has no name", async () => {
    const spans = [createMetric({ id: "provider", parentId: null, type: 7, name: "" })];

    const metricViewService = new MetricViewService(createMetricService({
        retrieveMetricsByCorrelationIdAsync: async () => spans
    }));

    const correlationView =
        await metricViewService.retrieveMetricCorrelationViewAsync(correlationId);

    expect(correlationView.spans[0].labelText).toBe("Provider");
});

it("should report provider requests and the proxy overhead left over", async () => {
    const spans = [
        createMetric({ id: "root", parentId: null, type: 0, durationMs: 183 }),
        createMetric({ id: "access", parentId: "root", type: 2, durationMs: 0 }),
        createMetric({ id: "providerRequests", parentId: "root", type: 3, durationMs: 141 }),
        createMetric({ id: "consolidation", parentId: "root", type: 10, durationMs: 3 })
    ];

    const metricViewService = new MetricViewService(createMetricService({
        retrieveMetricsByCorrelationIdAsync: async () => spans
    }));

    const correlationView =
        await metricViewService.retrieveMetricCorrelationViewAsync(correlationId);

    expect(correlationView.durationText).toBe("183 ms");
    expect(correlationView.providerRequestsText).toBe("141 ms");
    expect(correlationView.proxyOverheadText).toBe("42 ms");
});

it("should say the overhead is not known when there is no provider requests span", async () => {
    // A request that failed before provider discovery never records one, and subtracting nothing
    // would read as "no overhead" rather than "not known".
    const spans = [createMetric({ id: "root", parentId: null, type: 0, durationMs: 183 })];

    const metricViewService = new MetricViewService(createMetricService({
        retrieveMetricsByCorrelationIdAsync: async () => spans
    }));

    const correlationView =
        await metricViewService.retrieveMetricCorrelationViewAsync(correlationId);

    expect(correlationView.providerRequestsText).toBe("—");
    expect(correlationView.proxyOverheadText).toBe("—");
});

it("should never show a negative proxy overhead", async () => {
    // Separate stopwatches time the two spans, so rounding can leave the child a hair longer.
    const spans = [
        createMetric({ id: "root", parentId: null, type: 0, durationMs: 140.9 }),
        createMetric({ id: "providerRequests", parentId: "root", type: 3, durationMs: 141 })
    ];

    const metricViewService = new MetricViewService(createMetricService({
        retrieveMetricsByCorrelationIdAsync: async () => spans
    }));

    const correlationView =
        await metricViewService.retrieveMetricCorrelationViewAsync(correlationId);

    expect(correlationView.proxyOverheadText).toBe("0.00 ms");
});

it("should average both samples and derive the proxy overhead from them", async () => {
    const metricViewService = new MetricViewService(createMetricService({
        retrieveRequestMetricsAsync: async () => [
            createMetric({ id: "r1", durationMs: 183 }),
            createMetric({ id: "r2", durationMs: 230 }),
            createMetric({ id: "r3", durationMs: 100 })
        ],
        retrieveProviderRequestsMetricsAsync: async () => [
            createMetric({ id: "p1", type: 3, durationMs: 141 }),
            createMetric({ id: "p2", type: 3, durationMs: 200 }),
            createMetric({ id: "p3", type: 3, durationMs: 61 })
        ]
    }));

    const averagesView = await metricViewService.retrieveMetricAveragesViewAsync(noFilter);

    // (183 + 230 + 100) / 3 = 171, (141 + 200 + 61) / 3 = 134, leaving 37.
    expect(averagesView.averageRequestText).toBe("171 ms");
    expect(averagesView.averageProviderRequestsText).toBe("134 ms");
    expect(averagesView.averageProxyOverheadText).toBe("37 ms");
    expect(averagesView.sampleText).toBe("Across the latest 3 requests");
});

it("should ask for both samples with one page each", async () => {
    const requestedTakes: number[] = [];

    const metricViewService = new MetricViewService(createMetricService({
        retrieveRequestMetricsAsync: async metricQuery => {
            requestedTakes.push(metricQuery.take);

            return [createMetric({ durationMs: 10 })];
        },
        retrieveProviderRequestsMetricsAsync: async metricQuery => {
            requestedTakes.push(metricQuery.take);

            return [createMetric({ type: 3, durationMs: 4 })];
        }
    }));

    await metricViewService.retrieveMetricAveragesViewAsync(noFilter);

    expect(requestedTakes).toEqual([metricPageSize, metricPageSize]);
});

it("should say nothing is recorded yet rather than averaging an empty sample", async () => {
    const metricViewService = new MetricViewService(createMetricService());

    const averagesView = await metricViewService.retrieveMetricAveragesViewAsync(noFilter);

    expect(averagesView.averageRequestText).toBe("—");
    expect(averagesView.averageProviderRequestsText).toBe("—");
    expect(averagesView.averageProxyOverheadText).toBe("—");
    expect(averagesView.sampleText).toBe("No requests recorded yet");
});

it("should wrap an averages failure in a view service exception", async () => {
    const metricViewService = new MetricViewService(createMetricService({
        retrieveProviderRequestsMetricsAsync: async () => { throw new Error("dependency down"); }
    }));

    await expect(metricViewService.retrieveMetricAveragesViewAsync(noFilter))
        .rejects.toThrowError(
            "We could not load the metric averages, please try again or contact support.");
});

it("should split the averages bar into the two parts that fill it exactly", async () => {
    const metricViewService = new MetricViewService(createMetricService({
        retrieveRequestMetricsAsync: async () => [createMetric({ durationMs: 319 })],
        retrieveProviderRequestsMetricsAsync: async () =>
            [createMetric({ type: 3, durationMs: 302 })]
    }));

    const averagesView = await metricViewService.retrieveMetricAveragesViewAsync(noFilter);

    expect(averagesView.bars.hasBars).toBe(true);
    expect(averagesView.bars.providerRequestsPercent).toBeCloseTo((302 / 319) * 100, 5);

    // The two always complete each other, so no rounding gap shows at the end of the bar.
    expect(averagesView.bars.providerRequestsPercent + averagesView.bars.proxyOverheadPercent).toBe(100);

    expect(averagesView.bars.requestTooltip).toBe("Avg request time 319 ms");
    expect(averagesView.bars.providerRequestsTooltip)
        .toBe("Avg provider requests 302 ms of 319 ms");
    expect(averagesView.bars.proxyOverheadTooltip).toBe("Avg proxy overhead 17 ms of 319 ms");
});

it("should draw no bars when there is nothing to divide", async () => {
    const metricViewService = new MetricViewService(createMetricService());

    const averagesView = await metricViewService.retrieveMetricAveragesViewAsync(noFilter);

    expect(averagesView.bars.hasBars).toBe(false);
    expect(Number.isNaN(averagesView.bars.providerRequestsPercent)).toBe(false);
});

it("should fill the bar with provider time when a request was entirely provider bound", async () => {
    const metricViewService = new MetricViewService(createMetricService({
        retrieveRequestMetricsAsync: async () => [createMetric({ durationMs: 100 })],
        retrieveProviderRequestsMetricsAsync: async () =>
            [createMetric({ type: 3, durationMs: 120 })]
    }));

    const averagesView = await metricViewService.retrieveMetricAveragesViewAsync(noFilter);

    // Separate stopwatches can leave the child longer than its parent; the bar must not overflow.
    expect(averagesView.bars.providerRequestsPercent).toBe(100);
    expect(averagesView.bars.proxyOverheadPercent).toBe(0);
    expect(averagesView.averageProxyOverheadText).toBe("0.00 ms");
});

it("should give the detail card the same bars, labelled for one request", async () => {
    const spans = [
        createMetric({ id: "root", parentId: null, type: 0, durationMs: 164 }),
        createMetric({ id: "providerRequests", parentId: "root", type: 3, durationMs: 163.84 })
    ];

    const metricViewService = new MetricViewService(createMetricService({
        retrieveMetricsByCorrelationIdAsync: async () => spans
    }));

    const correlationView =
        await metricViewService.retrieveMetricCorrelationViewAsync(correlationId);

    expect(correlationView.bars.hasBars).toBe(true);
    expect(correlationView.bars.providerRequestsPercent).toBeCloseTo((163.84 / 164) * 100, 5);

    expect(correlationView.bars.providerRequestsPercent
        + correlationView.bars.proxyOverheadPercent).toBe(100);

    // Labelled for a single request, not for an average.
    expect(correlationView.bars.requestTooltip).toBe("Request duration 164 ms");
    expect(correlationView.bars.providerRequestsTooltip)
        .toBe("Provider requests 164 ms of 164 ms");
    expect(correlationView.bars.proxyOverheadTooltip)
        .toBe("Proxy overhead 0.16 ms of 164 ms");
});

it("should draw no bars on a detail card with nothing to divide", async () => {
    const spans = [createMetric({ id: "root", parentId: null, type: 0, durationMs: 0 })];

    const metricViewService = new MetricViewService(createMetricService({
        retrieveMetricsByCorrelationIdAsync: async () => spans
    }));

    const correlationView =
        await metricViewService.retrieveMetricCorrelationViewAsync(correlationId);

    expect(correlationView.bars.hasBars).toBe(false);
});
