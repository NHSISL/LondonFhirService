import type { FhirRecordDifferenceQuery } from "../../models/foundations/fhirRecordDifferences/FhirRecordDifferenceQuery";

// OData query options on this endpoint are bound against the CLR type, so property names are
// PascalCase here even though the payload comes back camelCased. Kept apart from the broker so
// the query string can be exercised without standing up the authenticated transport.
//
// $select is deliberately not used to drop DiffJson from the list. This is an attribute routed
// controller rather than an OData route, so a projection comes back as a SelectExpandWrapper that
// the host's System.Text.Json pipeline cannot serialise into a readable object. The rows carry
// their DiffJson instead, which is what lets the list break each row down by difference kind
// without a second round trip - see comparisonPageSize for the bound that keeps that affordable.
export function buildFhirRecordDifferenceQueryUrl(
    relativeFhirRecordDifferencesUrl: string,
    fhirRecordDifferenceQuery: FhirRecordDifferenceQuery)
    : string {
    const queryOptions = [
        "$orderby=ComparedAt desc",
        `$skip=${fhirRecordDifferenceQuery.skip}`,
        `$top=${fhirRecordDifferenceQuery.take}`
    ];

    const filter = buildFhirRecordDifferenceFilter(fhirRecordDifferenceQuery);

    if (filter.length > 0) {
        queryOptions.push(`$filter=${encodeURIComponent(filter)}`);
    }

    return `${relativeFhirRecordDifferencesUrl}?${queryOptions.join("&")}`;
}

function buildFhirRecordDifferenceFilter(
    fhirRecordDifferenceQuery: FhirRecordDifferenceQuery)
    : string {
    const clauses: string[] = [];
    const trimmedSearchTerm = fhirRecordDifferenceQuery.searchTerm.trim();

    if (trimmedSearchTerm.length > 0) {
        const literal = toStringLiteral(trimmedSearchTerm);

        // Comment is nullable, so it is guarded rather than passed straight into contains().
        clauses.push(
            "(" +
            [
                `contains(CorrelationId,${literal})`,
                `(Comment ne null and contains(Comment,${literal}))`
            ].join(" or ") +
            ")");
    }

    if (fhirRecordDifferenceQuery.unresolvedOnly) {
        clauses.push("IsResolved eq false");
    }

    return clauses.join(" and ");
}

// A single quote is escaped by doubling it in an OData string literal. Without this, a search term
// containing one would break the query.
function toStringLiteral(value: string): string {
    return `'${value.split("'").join("''")}'`;
}
