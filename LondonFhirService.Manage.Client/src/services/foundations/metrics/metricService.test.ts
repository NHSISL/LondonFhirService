import { expect, it } from "vitest";
import { MetricService } from "./metricService";
import type { IMetricApiBroker } from "../../../brokers/apis/iMetricApiBroker";
import type { MetricFilter } from "../../../models/foundations/metrics/MetricFilter";

// The two spellings of one correlation id. The API filters on a Guid column, where an OData guid
// literal is only defined with the dashes in, so whichever one the operator pasted, the broker must
// be handed the dashed one.
const compactCorrelationId = "0f1c4d6b9a2e4f318c771b2a3c4d5e6f";
const dashedCorrelationId = "0f1c4d6b-9a2e-4f31-8c77-1b2a3c4d5e6f";
const metricQuery = { skip: 0, take: 50 };

const createFilter = (correlationId: string): MetricFilter => ({
    correlationId: correlationId,
    userId: "",
    status: "",
    fromDate: "",
    toDate: ""
});

const createMetricApiBroker = (overrides: Partial<IMetricApiBroker> = {}): IMetricApiBroker => ({
    getRequestMetricsAsync: async () => [],
    getMetricAveragesAsync: async () => ({
        requestCount: 0,
        averageRequestMs: null,
        providerRequestsCount: 0,
        averageProviderRequestsMs: null
    }),
    getProviderRequestsMetricsByCorrelationIdsAsync: async () => [],
    getMetricExportAsync: async () => new Blob(),
    getMetricsByCorrelationIdAsync: async () => [],
    ...overrides
});

it("should search on a compact correlation id by handing the broker its dashed form", async () => {
    let capturedFilter: MetricFilter | null = null;

    const metricService = new MetricService(createMetricApiBroker({
        getRequestMetricsAsync: async (_metricQuery, metricFilter) => {
            capturedFilter = metricFilter;

            return [];
        }
    }));

    await metricService.retrieveRequestMetricsAsync(metricQuery, createFilter(compactCorrelationId));

    expect(capturedFilter).toEqual(createFilter(dashedCorrelationId));
});

it("should search on a dashed correlation id as it is", async () => {
    let capturedFilter: MetricFilter | null = null;

    const metricService = new MetricService(createMetricApiBroker({
        getRequestMetricsAsync: async (_metricQuery, metricFilter) => {
            capturedFilter = metricFilter;

            return [];
        }
    }));

    await metricService.retrieveRequestMetricsAsync(metricQuery, createFilter(dashedCorrelationId));

    expect(capturedFilter).toEqual(createFilter(dashedCorrelationId));
});

it("should search with no correlation id when none is given", async () => {
    let capturedFilter: MetricFilter | null = null;

    const metricService = new MetricService(createMetricApiBroker({
        getRequestMetricsAsync: async (_metricQuery, metricFilter) => {
            capturedFilter = metricFilter;

            return [];
        }
    }));

    await metricService.retrieveRequestMetricsAsync(metricQuery, createFilter(""));

    expect(capturedFilter).toEqual(createFilter(""));
});

it("should export on a compact correlation id by handing the broker its dashed form", async () => {
    let capturedFilter: MetricFilter | null = null;

    const metricService = new MetricService(createMetricApiBroker({
        getMetricExportAsync: async metricFilter => {
            capturedFilter = metricFilter;

            return new Blob();
        }
    }));

    await metricService.retrieveMetricExportAsync(createFilter(compactCorrelationId));

    expect(capturedFilter).toEqual(createFilter(dashedCorrelationId));
});

it("should load a correlation from its compact id by handing the broker its dashed form", async () => {
    let capturedCorrelationId: string | null = null;

    const metricService = new MetricService(createMetricApiBroker({
        getMetricsByCorrelationIdAsync: async correlationId => {
            capturedCorrelationId = correlationId;

            return [];
        }
    }));

    await metricService.retrieveMetricsByCorrelationIdAsync(compactCorrelationId, metricQuery);

    expect(capturedCorrelationId).toBe(dashedCorrelationId);
});

it("should load a correlation from its dashed id as it is", async () => {
    let capturedCorrelationId: string | null = null;

    const metricService = new MetricService(createMetricApiBroker({
        getMetricsByCorrelationIdAsync: async correlationId => {
            capturedCorrelationId = correlationId;

            return [];
        }
    }));

    await metricService.retrieveMetricsByCorrelationIdAsync(dashedCorrelationId, metricQuery);

    expect(capturedCorrelationId).toBe(dashedCorrelationId);
});

it("should look up provider requests for compact correlation ids by their dashed form", async () => {
    let capturedCorrelationIds: string[] | null = null;

    const metricService = new MetricService(createMetricApiBroker({
        getProviderRequestsMetricsByCorrelationIdsAsync: async correlationIds => {
            capturedCorrelationIds = correlationIds;

            return [];
        }
    }));

    await metricService.retrieveProviderRequestsMetricsByCorrelationIdsAsync(
        [compactCorrelationId, dashedCorrelationId]);

    expect(capturedCorrelationIds).toEqual([dashedCorrelationId, dashedCorrelationId]);
});
