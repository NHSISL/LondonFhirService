// What the metrics master page is narrowed to. Every field is optional; an empty string means
// "no bound on this". Dates are the raw YYYY-MM-DD strings a date input produces - the broker
// widens them to whole local days before sending them.
export type MetricFilter = {
    correlationId: string;
    fromDate: string;
    toDate: string;
};
