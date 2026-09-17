import type { FhirRecordQuery } from "../../models/foundations/fhirRecords/FhirRecordQuery";
import { toStringLiteral } from "./odataLiterals";

// OData query options on this endpoint are bound against the CLR type, so property names are
// PascalCase here even though the payload comes back camelCased. Kept apart from the
// broker so the query string can be exercised without standing up the authenticated transport.
export function buildPendingFhirRecordQueryUrl(
    relativeFhirRecordsUrl: string,
    fhirRecordQuery: FhirRecordQuery)
    : string {
    const queryOptions = [
        // $select is not an optimisation here, it is the only thing that makes this query
        // affordable. A FhirRecord carries the provider's entire FHIR bundle in JsonPayload, so
        // asking for whole rows would move a patient record per row to render a one line "still
        // waiting" entry. These six fields are everything the pending list shows.
        //
        // A projection is serialised by OData's SelectExpandWrapper rather than by the host's
        // own pipeline, which is a separate code path that need not name fields the same way. It
        // does - camelCase, same as an unprojected row - and an acceptance test pins that, because
        // a field arriving under another name is not an error here, it is a list of blank rows.
        "$select=Id,CorrelationId,SourceName,IsPrimarySource,Status,InsertedDate",
        "$orderby=InsertedDate desc",
        `$top=${fhirRecordQuery.take}`,
        `$filter=${encodeURIComponent(buildPendingFhirRecordFilter(fhirRecordQuery))}`
    ];

    return `${relativeFhirRecordsUrl}?${queryOptions.join("&")}`;
}

function buildPendingFhirRecordFilter(fhirRecordQuery: FhirRecordQuery): string {
    // IsProcessed rather than a status, because it is the flag the compare queue itself writes
    // when a record is done with. A record that failed is still unprocessed and still worth
    // showing - it is stuck rather than finished, and its status badge says so.
    const clauses = ["IsProcessed eq false"];
    const trimmedSearchTerm = fhirRecordQuery.searchTerm.trim();

    if (trimmedSearchTerm.length > 0) {
        // Only the correlation id. The comparisons list also searches comments, but a record that
        // has not been compared has no comment to search - matching on one of the two fields is
        // the most this side can honestly do.
        clauses.push(`contains(CorrelationId,${toStringLiteral(trimmedSearchTerm)})`);
    }

    return clauses.join(" and ");
}
