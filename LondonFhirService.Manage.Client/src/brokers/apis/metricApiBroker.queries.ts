import type { MetricFilter } from "../../models/foundations/metrics/MetricFilter";
import type { MetricQuery } from "../../models/foundations/metrics/MetricQuery";
import moment from "moment";

// OData query options on this endpoint are bound against the CLR type, so property names are
// PascalCase here even though the payload comes back camelCased. Kept apart from the broker so
// the query strings can be exercised without standing up the authenticated transport.
//
// The syntax below is not guesswork: MetricsControllerTests.ODataFilters runs each of these
// through the same query pipeline [EnableQuery] uses. In particular an enum member can be named
// as a plain string - Type eq 'Request' - with no qualifying namespace.
export function buildRequestMetricQueryUrl(
    relativeMetricsUrl: string,
    metricQuery: MetricQuery,
    metricFilter?: MetricFilter)
    : string {
    // Request is the root span of a correlation; every other kind is a child of one. The master
    // list is a list of requests, so the children are filtered out here rather than fetched and
    // discarded.
    return buildTypedMetricQueryUrl(
        relativeMetricsUrl,
        "Request",
        metricQuery,
        metricFilter);
}

// The master list never fetches these - it filters to root spans - so the averages need their own
// query rather than a figure that could be read off the rows already on screen.
export function buildProviderRequestsMetricQueryUrl(
    relativeMetricsUrl: string,
    metricQuery: MetricQuery,
    metricFilter?: MetricFilter)
    : string {
    return buildTypedMetricQueryUrl(
        relativeMetricsUrl,
        "ProviderRequests",
        metricQuery,
        metricFilter);
}

function buildTypedMetricQueryUrl(
    relativeMetricsUrl: string,
    typeName: string,
    metricQuery: MetricQuery,
    metricFilter?: MetricFilter)
    : string {
    const clauses = [`Type eq '${typeName}'`, ...buildFilterClauses(metricFilter)];

    const queryOptions = [
        `$filter=${encodeURIComponent(clauses.join(" and "))}`,
        "$orderby=Started desc",
        `$skip=${metricQuery.skip}`,
        `$top=${metricQuery.take}`
    ];

    return `${relativeMetricsUrl}?${queryOptions.join("&")}`;
}

// The date bounds are widened to whole local days: an operator picking 25 August means the whole
// of the 25th, not the instant it began. The upper bound is inclusive for the same reason.
function buildFilterClauses(metricFilter?: MetricFilter): string[] {
    if (metricFilter === undefined) {
        return [];
    }

    const clauses: string[] = [];
    const correlationId = metricFilter.correlationId.trim();

    if (correlationId.length > 0) {
        clauses.push(`CorrelationId eq ${correlationId}`);
    }

    const fromDate = moment(metricFilter.fromDate, "YYYY-MM-DD", true);

    if (fromDate.isValid()) {
        clauses.push(`CreatedDate ge ${fromDate.startOf("day").toISOString()}`);
    }

    const toDate = moment(metricFilter.toDate, "YYYY-MM-DD", true);

    if (toDate.isValid()) {
        clauses.push(`CreatedDate le ${toDate.endOf("day").toISOString()}`);
    }

    return clauses;
}

export function buildCorrelationMetricQueryUrl(
    relativeMetricsUrl: string,
    correlationId: string,
    metricQuery: MetricQuery)
    : string {
    // A guid is an unquoted literal in OData. It is still encoded, because the caller controls
    // this value and a malformed one must not be able to add query options of its own.
    const filter = `CorrelationId eq ${encodeURIComponent(correlationId)}`;

    const queryOptions = [
        `$filter=${filter}`,

        // Ascending, so the table reads in the order the work happened rather than newest first.
        "$orderby=Started asc",
        `$skip=${metricQuery.skip}`,
        `$top=${metricQuery.take}`
    ];

    return `${relativeMetricsUrl}?${queryOptions.join("&")}`;
}
