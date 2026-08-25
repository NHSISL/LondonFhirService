import type { MetricDurationBars } from "./MetricDurationBars";

// Headline averages for the metrics master page.
//
// The two samples are fetched separately - the master list holds only root Request spans, so a
// ProviderRequests figure cannot be read off the rows on screen - and each covers the most recent
// N spans of its kind. sampleText says what "recent" means so the numbers are not mistaken for an
// all-time average.
export type MetricAveragesView = {
    averageRequestText: string;
    averageProviderRequestsText: string;
    averageProxyOverheadText: string;
    sampleText: string;
    bars: MetricDurationBars;
};
