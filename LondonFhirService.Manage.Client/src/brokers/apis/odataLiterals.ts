/**
 * A single quote is escaped by doubling it in an OData string literal. Without this, a search term
 * containing one would break the query - so this is escaping of caller-controlled input, and four
 * private copies of it was four places to fix the next thing found to need escaping.
 */
export function toStringLiteral(value: string): string {
    return `'${value.split("'").join("''")}'`;
}
