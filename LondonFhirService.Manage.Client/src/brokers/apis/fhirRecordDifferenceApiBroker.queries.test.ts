import { expect, it } from "vitest";
import { buildFhirRecordDifferenceQueryUrl } from "./fhirRecordDifferenceApiBroker.queries";
import type { FhirRecordDifferenceQuery } from "../../models/foundations/fhirRecordDifferences/FhirRecordDifferenceQuery";

const createQuery = (
    overrides: Partial<FhirRecordDifferenceQuery> = {})
    : FhirRecordDifferenceQuery => ({
    skip: 0,
    take: 25,
    searchTerm: "",
    unresolvedOnly: false,
    ...overrides
});

it("should page newest first without a filter when nothing is asked for", () => {
    const url = buildFhirRecordDifferenceQueryUrl("/api/fhirrecorddifferences", createQuery());

    expect(url).toBe(
        "/api/fhirrecorddifferences?$orderby=ComparedAt desc&$skip=0&$top=25");
});

it("should carry the paging window through", () => {
    const url = buildFhirRecordDifferenceQueryUrl(
        "/api/fhirrecorddifferences",
        createQuery({ skip: 50, take: 25 }));

    expect(url).toContain("$skip=50");
    expect(url).toContain("$top=25");
});

// CorrelationId and Comment are PascalCase because OData binds the options against the CLR type,
// even though the payload comes back camelCased.
it("should search correlation id and comment, guarding the nullable comment", () => {
    const url = buildFhirRecordDifferenceQueryUrl(
        "/api/fhirrecorddifferences",
        createQuery({ searchTerm: "abc-123" }));

    expect(decodeURIComponent(url)).toContain(
        "$filter=(contains(CorrelationId,'abc-123') or "
        + "(Comment ne null and contains(Comment,'abc-123')))");
});

it("should combine the search and the unresolved filter", () => {
    const url = buildFhirRecordDifferenceQueryUrl(
        "/api/fhirrecorddifferences",
        createQuery({ searchTerm: "abc", unresolvedOnly: true }));

    expect(decodeURIComponent(url)).toContain("and IsResolved eq false");
});

it("should filter on unresolved alone", () => {
    const url = buildFhirRecordDifferenceQueryUrl(
        "/api/fhirrecorddifferences",
        createQuery({ unresolvedOnly: true }));

    expect(decodeURIComponent(url)).toContain("$filter=IsResolved eq false");
});

// A single quote is escaped by doubling it in an OData string literal. Without this a search term
// containing one would break the query rather than return nothing.
it("should escape a single quote in the search term", () => {
    const url = buildFhirRecordDifferenceQueryUrl(
        "/api/fhirrecorddifferences",
        createQuery({ searchTerm: "o'brien" }));

    expect(decodeURIComponent(url)).toContain("contains(CorrelationId,'o''brien')");
});

it("should ignore a search term that is only whitespace", () => {
    const url = buildFhirRecordDifferenceQueryUrl(
        "/api/fhirrecorddifferences",
        createQuery({ searchTerm: "   " }));

    expect(url).not.toContain("$filter");
});
