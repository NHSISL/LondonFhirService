// One correlation id, two spellings, because the two things that record it store it differently.
//
// A FhirRecord keeps it as a 32 character string and so it reaches the client - and the response
// header on the structured record screen - without dashes: d8924d9709dab2e07cf313bef9fdf820. A
// Metric keeps it as a uniqueidentifier, so it serialises dashed: d8924d97-09da-b2e0-7cf3-
// 13bef9fdf820. Same value both times.
//
// The spelling matters because the two screens look it up in ways that disagree about it. The
// metrics filter is `CorrelationId eq <literal>` against a Guid, and an OData guid literal is
// defined with the dashes in it, so the undashed form is not a value that means the same thing -
// it is a parse error. The comparisons filter is `contains(CorrelationId, '<text>')` against the
// string column, where a dashed needle simply never matches.
//
// So a link between the two screens has to convert, and the two functions below are the only
// places that know it.

const compactCorrelationIdPattern = /^[0-9a-fA-F]{32}$/;
const dashedCorrelationIdPattern =
    /^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$/;

/**
 * The dashed form the metrics route and its Guid filter expect. A value that is already dashed, or
 * that is not a correlation id at all, is handed back untouched: this is a formatter, and refusing
 * to guess at an unrecognised value leaves the route to say it found nothing rather than having
 * this mangle it into something that looks valid and is not.
 */
export function toDashedCorrelationId(correlationId: string): string {
    const trimmedCorrelationId = correlationId.trim();

    if (compactCorrelationIdPattern.test(trimmedCorrelationId) === false) {
        return trimmedCorrelationId;
    }

    return [
        trimmedCorrelationId.slice(0, 8),
        trimmedCorrelationId.slice(8, 12),
        trimmedCorrelationId.slice(12, 16),
        trimmedCorrelationId.slice(16, 20),
        trimmedCorrelationId.slice(20)
    ].join("-");
}

/**
 * The undashed form a FhirRecord stores, which is what the comparisons search matches on.
 */
export function toCompactCorrelationId(correlationId: string): string {
    const trimmedCorrelationId = correlationId.trim();

    return dashedCorrelationIdPattern.test(trimmedCorrelationId)
        ? trimmedCorrelationId.split("-").join("")
        : trimmedCorrelationId;
}

/**
 * Every span recorded against this correlation, whichever spelling the caller happens to hold.
 */
export function buildMetricsUrl(correlationId: string): string {
    return "/admin/metrics/"
        + encodeURIComponent(toDashedCorrelationId(correlationId));
}

/**
 * The comparisons list, filtered to this correlation. It lands on none, harmlessly, while the
 * compare queue is still working through it - the list says as much while it waits.
 */
export function buildComparisonsUrl(correlationId: string): string {
    return "/admin/comparisons?correlationId="
        + encodeURIComponent(toCompactCorrelationId(correlationId));
}
