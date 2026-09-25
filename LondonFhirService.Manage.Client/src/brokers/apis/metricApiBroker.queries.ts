import type { MetricFilter } from "../../models/foundations/metrics/MetricFilter";
import type { MetricQuery } from "../../models/foundations/metrics/MetricQuery";
import moment from "moment";
import { toStringLiteral } from "./odataLiterals";

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

// The averages across the whole table, worked out by the database in one call: $apply groups the
// Request and ProviderRequests spans by type and averages each group. No filter and no paging -
// this is the all-time figure, not a sample - and MetricsControllerTests.ODataFilters runs the
// same expression through the pipeline [EnableQuery] uses.
export function buildMetricAveragesQueryUrl(relativeMetricsUrl: string): string {
    const apply =
        "filter(Type eq 'Request' or Type eq 'ProviderRequests')"
        + "/groupby((Type),aggregate($count as SpanCount,DurationMs with average as AverageDurationMs))";

    return `${relativeMetricsUrl}?$apply=${encodeURIComponent(apply)}`;
}

// The master list shows each request's proxy overhead, which needs the ProviderRequests span of
// every request on the page. One in list rather than a call per row: a request has exactly one
// ProviderRequests span, so the page size of the list bounds the size of this answer too.
export function buildProviderRequestsByCorrelationIdsQueryUrl(
    relativeMetricsUrl: string,
    correlationIds: string[])
    : string {
    // Guids are unquoted literals in OData. Encoded as a whole, like every other filter here, so
    // a malformed value cannot add query options of its own.
    const filter = `Type eq 'ProviderRequests' and CorrelationId in (${correlationIds.join(",")})`;

    const queryOptions = [
        `$filter=${encodeURIComponent(filter)}`,
        `$top=${correlationIds.length}`
    ];

    return `${relativeMetricsUrl}?${queryOptions.join("&")}`;
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

    const userId = metricFilter.userId.trim();

    if (userId.length > 0) {
        clauses.push(`UserId eq ${toStringLiteral(userId)}`);
    }

    const fromDate = toStartOfDay(metricFilter.fromDate);

    if (fromDate !== undefined) {
        clauses.push(`CreatedDate ge ${fromDate}`);
    }

    const toDate = toEndOfDay(metricFilter.toDate);

    if (toDate !== undefined) {
        clauses.push(`CreatedDate le ${toDate}`);
    }

    return clauses;
}

function toStartOfDay(value: string): string | undefined {
    const date = moment(value, "YYYY-MM-DD", true);

    return date.isValid() ? date.startOf("day").toISOString() : undefined;
}

function toEndOfDay(value: string): string | undefined {
    const date = moment(value, "YYYY-MM-DD", true);

    return date.isValid() ? date.endOf("day").toISOString() : undefined;
}

// The CSV export takes the same filter as the list, as plain query parameters rather than OData:
// it is a single unpaged file, not a queryable collection. The dates are widened to whole local
// days exactly as the list's are, so the file holds the same rows the list would.
export function buildMetricExportUrl(
    relativeMetricsUrl: string,
    metricFilter: MetricFilter)
    : string {
    const parameters = new URLSearchParams();
    const correlationId = metricFilter.correlationId.trim();
    const userId = metricFilter.userId.trim();
    const fromDate = toStartOfDay(metricFilter.fromDate);
    const toDate = toEndOfDay(metricFilter.toDate);

    if (correlationId.length > 0) {
        parameters.append("correlationId", correlationId);
    }

    if (userId.length > 0) {
        parameters.append("userId", userId);
    }

    if (fromDate !== undefined) {
        parameters.append("fromDate", fromDate);
    }

    if (toDate !== undefined) {
        parameters.append("toDate", toDate);
    }

    const queryString = parameters.toString();

    return queryString.length > 0
        ? `${relativeMetricsUrl}/exports?${queryString}`
        : `${relativeMetricsUrl}/exports`;
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
