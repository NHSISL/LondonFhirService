import moment from "moment";
import { MetricService } from "../../foundations/metrics/metricService";
import { MetricViewServiceException } from "../../../models/views/metrics/exceptions/MetricViewServiceException";
import { isSearchableCorrelationId } from "../../foundations/metrics/metricService.validations";
import type { IMetricService } from "../../foundations/metrics/iMetricService";
import type { IMetricViewService } from "./iMetricViewService";
import type { Metric } from "../../../models/foundations/metrics/Metric";
import type { MetricFilter } from "../../../models/foundations/metrics/MetricFilter";
import type { MetricAveragesView } from "../../../models/views/metrics/MetricAveragesView";
import type { MetricCorrelationView } from "../../../models/views/metrics/MetricCorrelationView";
import type { MetricDurationBars } from "../../../models/views/metrics/MetricDurationBars";
import type { MetricListItemView } from "../../../models/views/metrics/MetricListItemView";
import type { MetricPageView } from "../../../models/views/metrics/MetricPageView";
import type { MetricSpanView } from "../../../models/views/metrics/MetricSpanView";

const notSetText = "—";
const dateDisplayFormat = "DD MMM YYYY HH:mm:ss";

export const metricPageSize = 50;

// A correlation's span tree is small - a few dozen at most - but it is fetched a page at a time
// because the endpoint caps a response at its configured page size. This bounds the walk so a
// pathological correlation cannot spin here forever.
const maximumCorrelationPages = 20;

// A bar this narrow is still a visible sliver. Sub-millisecond spans are real - Consolidation
// routinely lands at hundredths of a millisecond - and would otherwise render as nothing at all.
const minimumBarWidthPercent = 0.4;

// Ordinals, because the Manage host registers no JsonStringEnumConverter and so serialises both
// enums as numbers. Indexed by value, matching MetricType and MetricStatus declaration order.
const metricTypeNames = [
    "Request",
    "Orchestration",
    "AccessCheck",
    "ProviderRequests",
    "ProviderDiscovery",
    "Foundation",
    "ProviderFanOut",
    "Provider",
    "ProviderCall",
    "Persist",
    "Consolidation"
];

const metricStatusNames = ["Succeeded", "Failed", "TimedOut", "Cancelled", "Skipped"];

// Resolved from the name rather than written as a bare 3, so the intent survives a reader who
// does not have the enum to hand.
const providerRequestsType = metricTypeNames.indexOf("ProviderRequests");

export class MetricViewService implements IMetricViewService {
    private readonly metricService: IMetricService;

    constructor(metricService: IMetricService = new MetricService()) {
        this.metricService = metricService;
    }

    public async retrieveMetricPageViewAsync(
        pageNumber: number,
        metricFilter: MetricFilter,
        abortSignal?: AbortSignal)
        : Promise<MetricPageView> {
        try {
            const metrics = await this.metricService.retrieveRequestMetricsAsync(
                {
                    skip: pageNumber * metricPageSize,
                    take: metricPageSize
                },
                metricFilter,
                abortSignal);

            return {
                metrics: metrics.map(metric => this.toMetricListItemView(metric)),

                // The endpoint reports no total, so a full page is taken as a signal that there
                // may be another one. A short page is the end.
                hasMore: metrics.length === metricPageSize
            };
        } catch (exception) {
            throw new MetricViewServiceException(
                "We could not load the request metrics, please try again or contact support.",
                exception);
        }
    }

    public async retrieveMetricAveragesViewAsync(
        metricFilter: MetricFilter,
        abortSignal?: AbortSignal)
        : Promise<MetricAveragesView> {
        try {
            const query = { skip: 0, take: metricPageSize };

            // Issued together rather than in sequence: neither depends on the other, and this is
            // work the page waits on before it can show a headline figure.
            const [requestMetrics, providerRequestsMetrics] = await Promise.all([
                this.metricService.retrieveRequestMetricsAsync(query, metricFilter, abortSignal),

                this.metricService.retrieveProviderRequestsMetricsAsync(
                    query,
                    metricFilter,
                    abortSignal)
            ]);

            const averageRequestMs = this.average(requestMetrics);
            const averageProviderRequestsMs = this.average(providerRequestsMetrics);

            const averageOverheadMs =
                this.measureProxyOverhead(averageRequestMs, averageProviderRequestsMs);

            return {
                averageRequestText: this.formatOptionalDuration(averageRequestMs),

                averageProviderRequestsText:
                    this.formatOptionalDuration(averageProviderRequestsMs),

                averageProxyOverheadText: this.formatOptionalDuration(averageOverheadMs),
                sampleText: this.buildSampleText(requestMetrics.length),

                bars: this.buildDurationBars(
                    averageRequestMs,
                    averageProviderRequestsMs,
                    "Avg request time",
                    "Avg provider requests",
                    "Avg proxy overhead")
            };
        } catch (exception) {
            throw new MetricViewServiceException(
                "We could not load the metric averages, please try again or contact support.",
                exception);
        }
    }

    public createMetricFilter(): MetricFilter {
        return { correlationId: "", fromDate: "", toDate: "" };
    }

    // A half typed correlation id is not an empty one: querying with it would be rejected by the
    // API, so the page holds off until the value is a complete identifier.
    public isSearchableCorrelationId(correlationId: string): boolean {
        return isSearchableCorrelationId(correlationId);
    }

    // Taken as a share of the request duration and then completed to 100, so the two parts always
    // fill the bar exactly rather than leaving a rounding gap at the end.
    private buildDurationBars(
        requestMs: number | undefined,
        providerRequestsMs: number | undefined,
        requestLabel: string,
        providerRequestsLabel: string,
        proxyOverheadLabel: string)
        : MetricDurationBars {
        const overheadMs = this.measureProxyOverhead(requestMs, providerRequestsMs);
        const requestText = this.formatOptionalDuration(requestMs);

        const providerRequestsPercent =
            requestMs !== undefined && providerRequestsMs !== undefined && requestMs > 0
                ? Math.min((providerRequestsMs / requestMs) * 100, 100)
                : 0;

        return {
            hasBars: requestMs !== undefined && requestMs > 0,
            providerRequestsPercent: providerRequestsPercent,
            proxyOverheadPercent: 100 - providerRequestsPercent,
            requestTooltip: `${requestLabel} ${requestText}`,

            providerRequestsTooltip:
                `${providerRequestsLabel} ${this.formatOptionalDuration(providerRequestsMs)}`
                + ` of ${requestText}`,

            proxyOverheadTooltip:
                `${proxyOverheadLabel} ${this.formatOptionalDuration(overheadMs)}`
                + ` of ${requestText}`
        };
    }

    private average(metrics: Metric[]): number | undefined {
        if (metrics.length === 0) {
            return undefined;
        }

        return metrics.reduce((total, metric) => total + metric.durationMs, 0) / metrics.length;
    }

    private buildSampleText(requestCount: number): string {
        if (requestCount === 0) {
            return "No requests recorded yet";
        }

        return requestCount === 1
            ? "Across the latest request"
            : `Across the latest ${requestCount} requests`;
    }

    public async retrieveMetricCorrelationViewAsync(
        correlationId: string,
        abortSignal?: AbortSignal)
        : Promise<MetricCorrelationView> {
        try {
            const metrics = await this.retrieveEverySpanAsync(correlationId, abortSignal);

            return this.toMetricCorrelationView(correlationId, metrics);
        } catch (exception) {
            throw new MetricViewServiceException(
                "We could not load this request's metrics, please try again or contact support.",
                exception);
        }
    }

    // The detail page promises every span of the correlation, so this pages until the API stops
    // filling a page rather than showing whatever the first response happened to hold.
    private async retrieveEverySpanAsync(
        correlationId: string,
        abortSignal?: AbortSignal)
        : Promise<Metric[]> {
        const metrics: Metric[] = [];

        for (let pageNumber = 0; pageNumber < maximumCorrelationPages; pageNumber++) {
            const page = await this.metricService.retrieveMetricsByCorrelationIdAsync(
                correlationId,
                {
                    skip: pageNumber * metricPageSize,
                    take: metricPageSize
                },
                abortSignal);

            metrics.push(...page);

            if (page.length < metricPageSize) {
                break;
            }
        }

        return metrics;
    }

    private toMetricListItemView(metric: Metric): MetricListItemView {
        return {
            id: metric.id,
            correlationId: metric.correlationId,
            startedText: this.formatDate(metric.started),
            methodText: metric.method || notSetText,
            nameText: metric.name || notSetText,
            statusText: this.mapStatusToDisplayText(metric.status),
            statusClassName: this.mapStatusToClassName(metric.status),
            durationText: this.formatDuration(metric.durationMs),
            consumerText: metric.consumer ?? notSetText,
            userIdText: metric.userId ?? notSetText,
            detailUrl: this.buildDetailUrl(metric.correlationId)
        };
    }

    private toMetricCorrelationView(
        correlationId: string,
        metrics: Metric[])
        : MetricCorrelationView {
        const depths = this.mapDepths(metrics);
        const rootMetric = metrics.find(metric => metric.parentId === null) ?? metrics[0];
        const window = this.measureWindow(metrics);

        const providerRequestsMs = metrics
            .find(metric => metric.type === providerRequestsType)?.durationMs;

        return {
            correlationId: correlationId,
            methodText: rootMetric?.method ?? notSetText,
            startedText: this.formatDate(rootMetric?.started ?? ""),
            durationText: this.formatDuration(rootMetric?.durationMs ?? 0),
            statusText: this.mapStatusToDisplayText(rootMetric?.status ?? 0),
            statusClassName: this.mapStatusToClassName(rootMetric?.status ?? 0),
            providerRequestsText: this.formatOptionalDuration(providerRequestsMs),
            proxyOverheadText: this.formatOptionalDuration(
                this.measureProxyOverhead(rootMetric?.durationMs, providerRequestsMs)),
            consumerText: rootMetric?.consumer ?? notSetText,
            userIdText: rootMetric?.userId ?? notSetText,
            spanCount: metrics.length,
            windowText: this.formatDuration(window.totalMs),

            bars: this.buildDurationBars(
                rootMetric?.durationMs,
                providerRequestsMs,
                "Request duration",
                "Provider requests",
                "Proxy overhead"),

            spans: metrics.map(metric => this.toMetricSpanView(metric, depths, window))
        };
    }

    // What the request cost outside of fetching from providers: the access check, consolidation
    // and the proxy's own work. Null whenever either figure is missing - a request that failed
    // before provider discovery has no ProviderRequests span, and a subtraction against a missing
    // operand would read as "no overhead" rather than "not known".
    private measureProxyOverhead(
        requestMs: number | undefined,
        providerRequestsMs: number | undefined)
        : number | undefined {
        if (requestMs === undefined || providerRequestsMs === undefined) {
            return undefined;
        }

        // Clamped: the two spans are timed by separate stopwatches, so rounding can leave the
        // child a hair longer than its parent, and a negative overhead is nonsense to show.
        return Math.max(requestMs - providerRequestsMs, 0);
    }

    private formatOptionalDuration(durationMs: number | undefined): string {
        return durationMs === undefined ? notSetText : this.formatDuration(durationMs);
    }

    // The timeline is drawn against the whole correlation, not against the request span. A
    // deferred Persist starts as its provider finishes and can outlive the request entirely, so
    // taking the request's duration as the width would push those bars off the end.
    private measureWindow(metrics: Metric[]): { startMs: number; totalMs: number } {
        const starts = metrics.map(metric => this.toEpochMs(metric.started));

        const ends = metrics.map(metric =>
            this.toEpochMs(metric.started) + metric.durationMs);

        const startMs = starts.length > 0 ? Math.min(...starts) : 0;
        const endMs = ends.length > 0 ? Math.max(...ends) : 0;

        return { startMs: startMs, totalMs: Math.max(0, endMs - startMs) };
    }

    private toEpochMs(value: string): number {
        const parsedValue = moment(value);

        return parsedValue.isValid() ? parsedValue.valueOf() : 0;
    }

    private toMetricSpanView(
        metric: Metric,
        depths: Map<string, number>,
        window: { startMs: number; totalMs: number })
        : MetricSpanView {
        const offsetMs = this.toEpochMs(metric.started) - window.startMs;

        // Everything collapses to one instant when a correlation holds a single zero length span.
        // Give it the full track rather than dividing by zero.
        const offsetPercent = window.totalMs > 0
            ? (offsetMs / window.totalMs) * 100
            : 0;

        const widthPercent = window.totalMs > 0
            ? Math.max((metric.durationMs / window.totalMs) * 100, minimumBarWidthPercent)
            : 100;

        return {
            id: metric.id,
            depth: depths.get(metric.id) ?? 0,
            typeText: this.mapTypeToDisplayText(metric.type),
            nameText: metric.name || notSetText,
            targetText: metric.target ?? notSetText,
            statusText: this.mapStatusToDisplayText(metric.status),
            statusClassName: this.mapStatusToClassName(metric.status),
            startedText: this.formatDate(metric.started),
            durationText: this.formatDuration(metric.durationMs),
            payloadText: this.formatBytes(metric.payloadBytes),
            errorCodeText: metric.errorCode ?? notSetText,
            descriptionText: metric.description ?? notSetText,

            // Clamped so a bar can never start past the track or spill over its right hand edge.
            offsetPercent: Math.min(Math.max(offsetPercent, 0), 100),
            widthPercent: Math.min(widthPercent, 100 - Math.min(Math.max(offsetPercent, 0), 100)),
            offsetText: `+${this.formatDuration(Math.max(offsetMs, 0))}`,
            barClassName: this.mapStatusToBarClassName(metric.status),
            labelText: this.buildSpanLabel(metric)
        };
    }

    // A fan out produces one Provider span per provider, all labelled "Provider". Naming them on
    // the timeline is the only way to tell which bar belongs to which provider without hovering.
    private buildSpanLabel(metric: Metric): string {
        const typeText = this.mapTypeToDisplayText(metric.type);

        if (typeText !== "Provider" || !metric.name) {
            return typeText;
        }

        return `${typeText} - ${metric.name}`;
    }

    private mapStatusToBarClassName(status: number): string {
        const barClassNames: Record<string, string> = {
            "Succeeded": "bg-success",
            "Failed": "bg-danger",
            "TimedOut": "bg-warning",
            "Cancelled": "bg-secondary",
            "Skipped": "bg-light border"
        };

        return barClassNames[this.mapStatusToDisplayText(status)] ?? "bg-secondary";
    }

    // Walks parentId to work out how far to indent each row, so the table shows the shape of the
    // request without a nested rendering. Capped so a cycle in the data cannot hang the page.
    private mapDepths(metrics: Metric[]): Map<string, number> {
        const parentsById = new Map<string, string | null>(
            metrics.map(metric => [metric.id, metric.parentId]));

        const depths = new Map<string, number>();

        for (const metric of metrics) {
            let depth = 0;
            let parentId = metric.parentId;

            while (parentId !== null && depth < metrics.length) {
                depth++;
                parentId = parentsById.get(parentId) ?? null;
            }

            depths.set(metric.id, depth);
        }

        return depths;
    }

    private mapTypeToDisplayText(type: number): string {
        return metricTypeNames[type] ?? `Unknown (${type})`;
    }

    private mapStatusToDisplayText(status: number): string {
        return metricStatusNames[status] ?? `Unknown (${status})`;
    }

    private mapStatusToClassName(status: number): string {
        const statusClassNames: Record<string, string> = {
            "Succeeded": "badge bg-success",
            "Failed": "badge bg-danger",
            "TimedOut": "badge bg-warning text-dark",
            "Cancelled": "badge bg-secondary",
            "Skipped": "badge bg-light text-dark border"
        };

        return statusClassNames[this.mapStatusToDisplayText(status)] ?? "badge bg-secondary";
    }

    // Sub-millisecond spans are real - that is why the server holds this as a double - so they
    // must not all render as "0 ms".
    private formatDuration(durationMs: number): string {
        if (durationMs >= 1000) {
            return `${(durationMs / 1000).toFixed(2)} s`;
        }

        if (durationMs >= 1) {
            return `${durationMs.toFixed(0)} ms`;
        }

        return `${durationMs.toFixed(2)} ms`;
    }

    private formatBytes(payloadBytes: number | null): string {
        if (payloadBytes === null) {
            return notSetText;
        }

        if (payloadBytes >= 1024 * 1024) {
            return `${(payloadBytes / (1024 * 1024)).toFixed(1)} MB`;
        }

        if (payloadBytes >= 1024) {
            return `${(payloadBytes / 1024).toFixed(1)} KB`;
        }

        return `${payloadBytes} B`;
    }

    private formatDate(value: string): string {
        if (!value || value.length === 0) {
            return notSetText;
        }

        const parsedValue = moment(value);

        return parsedValue.isValid() ? parsedValue.format(dateDisplayFormat) : notSetText;
    }

    private buildDetailUrl(correlationId: string): string {
        return `/admin/metrics/${encodeURIComponent(correlationId)}`;
    }
}
