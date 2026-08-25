import { expect, it } from "vitest";
import { isSearchableCorrelationId, validateMetricFilter } from "./metricService.validations";

const filter = (overrides: Partial<{ correlationId: string; fromDate: string; toDate: string }>) => ({
    correlationId: "",
    fromDate: "",
    toDate: "",
    ...overrides
});

it("should accept an empty filter", () => {
    expect(() => validateMetricFilter(filter({}))).not.toThrow();
});

it("should reject a correlation id that is not an identifier", () => {
    // It goes into an OData filter as a bare literal, so a malformed one is a 400 rather than an
    // empty result set.
    expect(() => validateMetricFilter(filter({ correlationId: "not-a-guid" })))
        .toThrowError("correlationId: A correlation id must be a valid identifier.");
});

it("should accept a whole correlation id in either case", () => {
    expect(isSearchableCorrelationId("0f1c4d6b-9a2e-4f31-8c77-1b2a3c4d5e6f")).toBe(true);
    expect(isSearchableCorrelationId("0F1C4D6B-9A2E-4F31-8C77-1B2A3C4D5E6F")).toBe(true);
    expect(isSearchableCorrelationId("0f1c4d6b-9a2e-4f31-8c77")).toBe(false);
    expect(isSearchableCorrelationId("")).toBe(false);
});

it("should reject a malformed date", () => {
    expect(() => validateMetricFilter(filter({ fromDate: "25-08-2026" })))
        .toThrowError("fromDate: A from date must be a valid date.");
});

it("should reject a range that runs backwards", () => {
    expect(() => validateMetricFilter(
        filter({ fromDate: "2026-08-25", toDate: "2026-08-24" })))
        .toThrowError("toDate: The to date must be the same as or later than the from date.");
});

it("should accept a single day range", () => {
    expect(() => validateMetricFilter(
        filter({ fromDate: "2026-08-25", toDate: "2026-08-25" }))).not.toThrow();
});
