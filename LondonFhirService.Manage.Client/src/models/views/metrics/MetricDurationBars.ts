// Geometry for the two stacked bars that show a request duration broken into the part spent on
// providers and the part that was not. Used for a single request on the detail page and for the
// averages on the master page, which is why the labels come through the tooltips rather than
// being fixed here.
export type MetricDurationBars = {
    hasBars: boolean;
    providerRequestsPercent: number;
    proxyOverheadPercent: number;
    requestTooltip: string;
    providerRequestsTooltip: string;
    proxyOverheadTooltip: string;
};
