import type { AuditQuery } from "../../models/foundations/audits/AuditQuery";

// OData query options on this endpoint are bound against the CLR type, so property names are
// PascalCase here even though the payload comes back camelCased. Kept apart from the broker so the
// query string can be exercised without standing up the authenticated transport.
export function buildAuditQueryUrl(relativeAuditsUrl: string, auditQuery: AuditQuery): string {
    const queryOptions = [
        "$orderby=CreatedDate desc",
        `$skip=${auditQuery.skip}`,
        `$top=${auditQuery.take}`
    ];

    const filter = buildAuditFilter(auditQuery.searchTerm);

    if (filter.length > 0) {
        queryOptions.push(`$filter=${encodeURIComponent(filter)}`);
    }

    return `${relativeAuditsUrl}?${queryOptions.join("&")}`;
}

function buildAuditFilter(searchTerm: string): string {
    const trimmedSearchTerm = searchTerm.trim();

    if (trimmedSearchTerm.length === 0) {
        return "";
    }

    const literal = toStringLiteral(trimmedSearchTerm);

    // CorrelationId is nullable, so it is guarded rather than passed straight into contains().
    return [
        `contains(Title,${literal})`,
        `contains(AuditType,${literal})`,
        `contains(Message,${literal})`,
        `contains(CreatedBy,${literal})`,
        `(CorrelationId ne null and contains(CorrelationId,${literal}))`
    ].join(" or ");
}

// A single quote is escaped by doubling it in an OData string literal. Without this, a search term
// containing one would break the query.
function toStringLiteral(value: string): string {
    return `'${value.split("'").join("''")}'`;
}
