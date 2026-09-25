// Averages the API worked out across the whole Metrics table, one per span type. Null when there
// are no spans of that type to average, which is not the same answer as zero.
export type MetricAverages = {
    requestCount: number;
    averageRequestMs: number | null;
    providerRequestsCount: number;
    averageProviderRequestsMs: number | null;
};
