import { expect, it } from "vitest";
import { buildAuditQueryUrl } from "./auditApiBroker.queries";

const buildUrl = (searchTerm: string): string =>
    buildAuditQueryUrl("/api/audits", { skip: 100, take: 50, searchTerm: searchTerm });

const filterOf = (url: string): string => decodeURIComponent(url.split("$filter=")[1]);

it("should page and order without a filter when the search term is blank", () => {
    expect(buildUrl("   ")).toBe("/api/audits?$orderby=CreatedDate desc&$skip=100&$top=50");
});

it("should search the searchable columns, guarding the nullable one", () => {
    expect(filterOf(buildUrl("patient"))).toBe(
        "contains(Title,'patient') or "
        + "contains(AuditType,'patient') or "
        + "contains(Message,'patient') or "
        + "contains(CreatedBy,'patient') or "
        + "(CorrelationId ne null and contains(CorrelationId,'patient'))");
});

it("should escape a single quote by doubling it", () => {
    const filter = filterOf(buildUrl("O'Brien"));

    expect(filter).toContain("contains(Title,'O''Brien')");
    expect(filter).not.toContain("contains(Title,'O'Brien')");
});

it("should trim the search term before building the filter", () => {
    expect(filterOf(buildUrl("  patient  "))).toContain("contains(Title,'patient')");
});
