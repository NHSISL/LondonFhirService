import { MetricValidationException } from "../../../models/foundations/metrics/exceptions/MetricValidationException";
import type { MetricFilter } from "../../../models/foundations/metrics/MetricFilter";
import type { MetricQuery } from "../../../models/foundations/metrics/MetricQuery";

// A correlation id goes into an OData filter as a bare guid literal, so a malformed one would be
// rejected by the API rather than simply matching nothing.
const correlationIdPattern =
    /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

const datePattern = /^\d{4}-\d{2}-\d{2}$/;

export function isSearchableCorrelationId(value: string): boolean {
    return correlationIdPattern.test(value.trim());
}

export function validateCorrelationId(correlationId: string): void {
    if (correlationId === null || correlationId === undefined) {
        throw new MetricValidationException("correlationId", "A correlation id is required.");
    }

    if (correlationId.trim().length === 0) {
        throw new MetricValidationException("correlationId", "A correlation id cannot be blank.");
    }
}

export function validateMetricQuery(metricQuery: MetricQuery): void {
    if (metricQuery === null || metricQuery === undefined) {
        throw new MetricValidationException("metricQuery", "A metric query is required.");
    }

    if (Number.isInteger(metricQuery.skip) === false || metricQuery.skip < 0) {
        throw new MetricValidationException("skip", "Skip must be a whole number of zero or more.");
    }

    if (Number.isInteger(metricQuery.take) === false || metricQuery.take < 1) {
        throw new MetricValidationException("take", "Take must be a whole number of one or more.");
    }
}

export function validateMetricFilter(metricFilter: MetricFilter): void {
    if (metricFilter === null || metricFilter === undefined) {
        throw new MetricValidationException("metricFilter", "A metric filter is required.");
    }

    const correlationId = metricFilter.correlationId.trim();

    if (correlationId.length > 0 && isSearchableCorrelationId(correlationId) === false) {
        throw new MetricValidationException(
            "correlationId",
            "A correlation id must be a valid identifier.");
    }

    validateOptionalDate(metricFilter.fromDate, "fromDate", "A from date");
    validateOptionalDate(metricFilter.toDate, "toDate", "A to date");

    if (datePattern.test(metricFilter.fromDate)
        && datePattern.test(metricFilter.toDate)
        && metricFilter.fromDate > metricFilter.toDate) {
        throw new MetricValidationException(
            "toDate",
            "The to date must be the same as or later than the from date.");
    }
}

function validateOptionalDate(value: string, fieldName: string, description: string): void {
    if (value.length === 0) {
        return;
    }

    if (datePattern.test(value) === false) {
        throw new MetricValidationException(fieldName, `${description} must be a valid date.`);
    }
}
