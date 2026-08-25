import ApiBroker from "../apiBroker";
import { MetricApiBrokerException } from "../../models/foundations/metrics/exceptions/MetricApiBrokerException";
import {
    buildCorrelationMetricQueryUrl,
    buildProviderRequestsMetricQueryUrl,
    buildRequestMetricQueryUrl
} from "./metricApiBroker.queries";

import type { IMetricApiBroker } from "./iMetricApiBroker";
import type { Metric } from "../../models/foundations/metrics/Metric";
import type { MetricFilter } from "../../models/foundations/metrics/MetricFilter";
import type { MetricQuery } from "../../models/foundations/metrics/MetricQuery";

export class MetricApiBroker implements IMetricApiBroker {
    private readonly relativeMetricsUrl = "/api/metrics";
    private readonly apiBroker: ApiBroker;

    constructor(apiBroker: ApiBroker = new ApiBroker()) {
        this.apiBroker = apiBroker;
    }

    public async getRequestMetricsAsync(
        metricQuery: MetricQuery,
        metricFilter: MetricFilter,
        abortSignal?: AbortSignal)
        : Promise<Metric[]> {
        try {
            const response = await this.apiBroker.GetAsync(
                buildRequestMetricQueryUrl(this.relativeMetricsUrl, metricQuery, metricFilter),
                abortSignal);

            return this.toMetrics(response.data);
        } catch (exception) {
            throw new MetricApiBrokerException(
                "Failed to retrieve request metrics from the API.",
                exception);
        }
    }

    public async getProviderRequestsMetricsAsync(
        metricQuery: MetricQuery,
        metricFilter: MetricFilter,
        abortSignal?: AbortSignal)
        : Promise<Metric[]> {
        try {
            const response = await this.apiBroker.GetAsync(
                buildProviderRequestsMetricQueryUrl(
                    this.relativeMetricsUrl,
                    metricQuery,
                    metricFilter),
                abortSignal);

            return this.toMetrics(response.data);
        } catch (exception) {
            throw new MetricApiBrokerException(
                "Failed to retrieve provider request metrics from the API.",
                exception);
        }
    }

    public async getMetricsByCorrelationIdAsync(
        correlationId: string,
        metricQuery: MetricQuery,
        abortSignal?: AbortSignal)
        : Promise<Metric[]> {
        try {
            const response = await this.apiBroker.GetAsync(
                buildCorrelationMetricQueryUrl(
                    this.relativeMetricsUrl,
                    correlationId,
                    metricQuery),
                abortSignal);

            return this.toMetrics(response.data);
        } catch (exception) {
            throw new MetricApiBrokerException(
                `Failed to retrieve metrics for correlation '${correlationId}' from the API.`,
                exception);
        }
    }

    private toMetrics(rawMetrics: unknown): Metric[] {
        if (Array.isArray(rawMetrics) === false) {
            throw new Error("The metrics endpoint did not return a collection.");
        }

        return (rawMetrics as unknown[]).map(rawMetric => this.toMetric(rawMetric));
    }

    // Format conversion only - the API is an untyped boundary, so every field is read
    // defensively rather than asserted into shape.
    private toMetric(rawMetric: unknown): Metric {
        if (typeof rawMetric !== "object" || rawMetric === null) {
            throw new Error("The metrics endpoint returned an unreadable metric.");
        }

        const source = rawMetric as Record<string, unknown>;

        return {
            id: this.readString(source.id),
            userId: this.readNullableString(source.userId),
            parentId: this.readNullableString(source.parentId),
            correlationId: this.readString(source.correlationId),
            method: this.readString(source.method),
            type: this.readNumber(source.type),
            name: this.readString(source.name),
            target: this.readNullableString(source.target),
            started: this.readString(source.started),
            completed: this.readString(source.completed),
            durationMs: this.readNumber(source.durationMs),
            status: this.readNumber(source.status),
            errorCode: this.readNullableString(source.errorCode),
            payloadBytes: this.readNullableNumber(source.payloadBytes),
            consumer: this.readNullableString(source.consumer),
            description: this.readNullableString(source.description),
            createdDate: this.readString(source.createdDate)
        };
    }

    private readString(rawValue: unknown): string {
        return typeof rawValue === "string" ? rawValue : "";
    }

    private readNullableString(rawValue: unknown): string | null {
        return typeof rawValue === "string" && rawValue.length > 0 ? rawValue : null;
    }

    private readNumber(rawValue: unknown): number {
        return typeof rawValue === "number" && Number.isNaN(rawValue) === false ? rawValue : 0;
    }

    private readNullableNumber(rawValue: unknown): number | null {
        return typeof rawValue === "number" && Number.isNaN(rawValue) === false ? rawValue : null;
    }
}
