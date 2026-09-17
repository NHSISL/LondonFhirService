import type { FhirRecordDifferenceQuery } from "../../models/foundations/fhirRecordDifferences/FhirRecordDifferenceQuery";

// OData query options on this endpoint are bound against the CLR type, so property names are
// PascalCase here even though the payload comes back camelCased. Kept apart from the broker so
// the query string can be exercised without standing up the authenticated transport.
//
// $select is deliberately not used to drop DiffJson from the list, but not because it could not
// be: a projection serialises perfectly readably, which is what the expand below relies on. The
// rows carry their DiffJson because the list breaks each row down by difference kind, and dropping
// the column would buy a second round trip per row - see comparisonPageSize for the bound that
// keeps carrying it affordable.
export function buildFhirRecordDifferenceQueryUrl(
    relativeFhirRecordDifferencesUrl: string,
    fhirRecordDifferenceQuery: FhirRecordDifferenceQuery)
    : string {
    const queryOptions = [
        // The source that produced the compared side, without its FhirRecord. A record carries the
        // whole bundle in JsonPayload, so expanding it whole would pull a patient record per row to
        // render a short label; the nested $select leaves the payload behind and brings two fields.
        //
        // Asking for an expand makes the host answer through OData's projection wrapper rather
        // than its own serialiser. Measured, that wrapper keeps the same camelCase - the expanded
        // object arrives as "secondary": { "sourceName": ... } - so the reader on the other side
        // needs no translation, only the nested object it was not previously asking for.
        "$expand=Secondary($select=SourceName,IsPrimarySource)",
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
