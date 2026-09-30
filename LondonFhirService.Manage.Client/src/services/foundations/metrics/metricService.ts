import { MetricApiBroker } from "../../../brokers/apis/metricApiBroker";
import { toDashedCorrelationId } from "../../../helpers/correlationIds";
import { tryCatchMetricServiceAsync } from "./metricService.exceptions";
import {
    validateCorrelationId,
    validateCorrelationIds,
    validateMetricFilter,
    validateMetricQuery
} from "./metricService.validations";
import type { IMetricApiBroker } from "../../../brokers/apis/iMetricApiBroker";
import type { IMetricService } from "./iMetricService";
import type { Metric } from "../../../models/foundations/metrics/Metric";
import type { MetricAverages } from "../../../models/foundations/metrics/MetricAverages";
import type { MetricFilter } from "../../../models/foundations/metrics/MetricFilter";
import type { MetricQuery } from "../../../models/foundations/metrics/MetricQuery";

export class MetricService implements IMetricService {
    private readonly metricApiBroker: IMetricApiBroker;

    constructor(metricApiBroker: IMetricApiBroker = new MetricApiBroker()) {
        this.metricApiBroker = metricApiBroker;
    }

    public async retrieveRequestMetricsAsync(
        metricQuery: MetricQuery,
        metricFilter: MetricFilter,
        abortSignal?: AbortSignal)
        : Promise<Metric[]> {
        return await tryCatchMetricServiceAsync(async () => {
            validateMetricQuery(metricQuery);
            validateMetricFilter(metricFilter);

            return await this.metricApiBroker.getRequestMetricsAsync(
                metricQuery,
                this.toDashedMetricFilter(metricFilter),
                abortSignal);
        });
    }

    public async retrieveMetricAveragesAsync(abortSignal?: AbortSignal): Promise<MetricAverages> {
        return await tryCatchMetricServiceAsync(async () =>
            await this.metricApiBroker.getMetricAveragesAsync(abortSignal));
    }

    public async retrieveProviderRequestsMetricsByCorrelationIdsAsync(
        correlationIds: string[],
        abortSignal?: AbortSignal)
        : Promise<Metric[]> {
        return await tryCatchMetricServiceAsync(async () => {
            validateCorrelationIds(correlationIds);

            // An empty page has nothing to look up, and an empty in list is not valid OData.
            if (correlationIds.length === 0) {
                return [];
            }

            return await this.metricApiBroker.getProviderRequestsMetricsByCorrelationIdsAsync(
                correlationIds.map(correlationId => toDashedCorrelationId(correlationId)),
                abortSignal);
        });
    }

    public async retrieveMetricExportAsync(
        metricFilter: MetricFilter,
        abortSignal?: AbortSignal)
        : Promise<Blob> {
        return await tryCatchMetricServiceAsync(async () => {
            validateMetricFilter(metricFilter);

            return await this.metricApiBroker.getMetricExportAsync(
                this.toDashedMetricFilter(metricFilter),
                abortSignal);
        });
    }

    public async retrieveMetricsByCorrelationIdAsync(
        correlationId: string,
        metricQuery: MetricQuery,
        abortSignal?: AbortSignal)
        : Promise<Metric[]> {
        return await tryCatchMetricServiceAsync(async () => {
            validateCorrelationId(correlationId);
            validateMetricQuery(metricQuery);

            return await this.metricApiBroker.getMetricsByCorrelationIdAsync(
                toDashedCorrelationId(correlationId),
                metricQuery,
                abortSignal);
        });
    }

    // The validations accept either spelling of a correlation id, but the broker puts it into an
    // OData filter as a guid literal, which is only defined with the dashes in. A copy, so the
    // caller's filter - and the query key the page built from it - is left as the operator typed it.
    private toDashedMetricFilter(metricFilter: MetricFilter): MetricFilter {
        return {
            ...metricFilter,
            correlationId: toDashedCorrelationId(metricFilter.correlationId)
        };
    }
}
