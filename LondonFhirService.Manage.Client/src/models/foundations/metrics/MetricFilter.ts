// What the metrics master page is narrowed to. Every field is optional; an empty string means
// "no bound on this". Dates are the raw YYYY-MM-DD strings a date input produces - the broker
// widens them to whole local days before sending them.
export type MetricFilter = {
    correlationId: string;

    // The caller's user id (oid), as the User column shows it. Matched exactly.
    userId: string;

    // How the request ended, by MetricStatus name - "Succeeded" or "Failed" - or empty for any.
    status: string;
    fromDate: string;
    toDate: string;
};
