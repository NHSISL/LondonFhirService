import type { MetricDurationBars } from "./MetricDurationBars";

// One averages tile on the metrics master page. There are two: every request in the table, and
// the requests the search has loaded. sampleText says which rows a tile covers, so the two are
// never mistaken for each other.
export type MetricAveragesView = {
    titleText: string;
    averageRequestText: string;
    averageProviderRequestsText: string;
    averageProxyOverheadText: string;
    sampleText: string;
    bars: MetricDurationBars;
};
