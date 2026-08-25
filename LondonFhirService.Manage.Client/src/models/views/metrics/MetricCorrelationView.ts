import type { MetricDurationBars } from "./MetricDurationBars";
import type { MetricSpanView } from "./MetricSpanView";

// Everything recorded under one correlation id: the root request span summarised, and every span
// beneath it in the order it started.
export type MetricCorrelationView = {
    correlationId: string;
    methodText: string;
    startedText: string;
    durationText: string;
    statusText: string;
    statusClassName: string;

    // How much of the request was spent getting data from providers, and what was left over.
    providerRequestsText: string;
    proxyOverheadText: string;
    consumerText: string;
    userIdText: string;
    spanCount: number;

    // The span of wall clock the timeline covers: from the earliest start to the latest finish.
    // Wider than the request's own duration whenever a deferred Persist outlives it.
    windowText: string;
    bars: MetricDurationBars;
    spans: MetricSpanView[];
};
