import { expect, it } from "vitest";
import { buildPendingFhirRecordQueryUrl } from "./fhirRecordApiBroker.queries";
import type { FhirRecordQuery } from "../../models/foundations/fhirRecords/FhirRecordQuery";

const createQuery = (overrides: Partial<FhirRecordQuery> = {}): FhirRecordQuery => ({
    take: 50,
    searchTerm: "",
    ...overrides
});

it("should ask for the unprocessed records newest first", () => {
    const url = buildPendingFhirRecordQueryUrl("/api/fhirrecords", createQuery());

    expect(url).toBe(
        "/api/fhirrecords"
        + "?$select=Id,CorrelationId,SourceName,IsPrimarySource,Status,InsertedDate"
        + "&$orderby=InsertedDate desc&$top=50"
        + "&$filter=IsProcessed%20eq%20false");
});

// Not a nicety. A FhirRecord carries the whole provider bundle, and fifty of them unprojected is
// a patient record apiece to render a list of "still waiting" lines.
it("should never ask for the payload", () => {
    const url = buildPendingFhirRecordQueryUrl("/api/fhirrecords", createQuery({ take: 50 }));

    expect(url).not.toContain("JsonPayload");
    expect(url).toContain("$select=");
});

it("should narrow to one correlation when the page is filtered", () => {
    const url = buildPendingFhirRecordQueryUrl(
        "/api/fhirrecords",
        createQuery({ searchTerm: "2168a66d8097c5a5d08c89eaac65960c" }));

    expect(decodeURIComponent(url)).toContain(
        "$filter=IsProcessed eq false and "
        + "contains(CorrelationId,'2168a66d8097c5a5d08c89eaac65960c')");
});

it("should trim a search term before asking", () => {
    const url = buildPendingFhirRecordQueryUrl(
        "/api/fhirrecords",
        createQuery({ searchTerm: "   abc   " }));

    expect(decodeURIComponent(url)).toContain("contains(CorrelationId,'abc')");
});

// A quote in the box would otherwise close the literal and make the whole filter unparseable.
it("should double a single quote in the search term", () => {
    const url = buildPendingFhirRecordQueryUrl(
        "/api/fhirrecords",
        createQuery({ searchTerm: "it's" }));

    expect(decodeURIComponent(url)).toContain("contains(CorrelationId,'it''s')");
});

it("should carry the take through", () => {
    const url = buildPendingFhirRecordQueryUrl("/api/fhirrecords", createQuery({ take: 5 }));

    expect(url).toContain("$top=5");
});
