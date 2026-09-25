// One root request span, as the master list renders it.
export type MetricListItemView = {
    id: string;
    correlationId: string;
    startedText: string;
    methodText: string;
    nameText: string;
    statusText: string;
    statusClassName: string;
    durationText: string;

    // The request duration less its provider requests - the same figure as the detail page. Not
    // set when the request never reached its providers, such as a failed access check.
    proxyOverheadText: string;

    // The raw figures behind durationText and proxyOverheadText, so the loaded requests tile can
    // average exactly the rows on screen. providerRequestsMs is null when the request never
    // reached its providers.
    durationMs: number;
    providerRequestsMs: number | null;
    consumerText: string;
    userIdText: string;
    detailUrl: string;
};
