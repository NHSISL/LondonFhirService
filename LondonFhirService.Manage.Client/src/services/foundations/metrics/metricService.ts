import { MetricApiBroker } from "../../../brokers/apis/metricApiBroker";
import { tryCatchMetricServiceAsync } from "./metricService.exceptions";
import {
    validateCorrelationId,
    validateMetricFilter,
    validateMetricQuery
} from "./metricService.validations";
import type { IMetricApiBroker } from "../../../brokers/apis/iMetricApiBroker";
import type { IMetricService } from "./iMetricService";
import type { Metric } from "../../../models/foundations/metrics/Metric";
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
                metricFilter,
                abortSignal);
        });
    }

    public async retrieveProviderRequestsMetricsAsync(
        metricQuery: MetricQuery,
        metricFilter: MetricFilter,
        abortSignal?: AbortSignal)
        : Promise<Metric[]> {
        return await tryCatchMetricServiceAsync(async () => {
            validateMetricQuery(metricQuery);
            validateMetricFilter(metricFilter);

            return await this.metricApiBroker.getProviderRequestsMetricsAsync(
                metricQuery,
                metricFilter,
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
                correlationId,
                metricQuery,
                abortSignal);
        });
    }
}
