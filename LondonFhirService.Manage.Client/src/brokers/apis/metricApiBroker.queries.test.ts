import { expect, it } from "vitest";
import {
    buildCorrelationMetricQueryUrl,
    buildProviderRequestsMetricQueryUrl,
    buildRequestMetricQueryUrl
} from "./metricApiBroker.queries";

const decode = (url: string): string => decodeURIComponent(url);

it("should ask only for root request spans, newest first", () => {
    const url = buildRequestMetricQueryUrl("/api/metrics", { skip: 100, take: 50 });

    expect(decode(url)).toBe(
        "/api/metrics?$filter=Type eq 'Request'&$orderby=Started desc&$skip=100&$top=50");
});

it("should ask for every span of one correlation, in the order the work started", () => {
    const url = buildCorrelationMetricQueryUrl(
        "/api/metrics",
        "0f1c4d6b-9a2e-4f31-8c77-1b2a3c4d5e6f",
        { skip: 0, take: 50 });

    expect(decode(url)).toBe(
        "/api/metrics?$filter=CorrelationId eq 0f1c4d6b-9a2e-4f31-8c77-1b2a3c4d5e6f"
        + "&$orderby=Started asc&$skip=0&$top=50");
});

it("should encode a correlation id so it cannot smuggle in query options", () => {
    const url = buildCorrelationMetricQueryUrl(
        "/api/metrics",
        "abc&$top=9999",
        { skip: 0, take: 50 });

    expect(url).toContain("CorrelationId eq abc%26%24top%3D9999");
    expect(url.split("$top=").length).toBe(2);
});

it("should ask only for provider request spans, newest first", () => {
    const url = buildProviderRequestsMetricQueryUrl("/api/metrics", { skip: 0, take: 50 });

    expect(decode(url)).toBe(
        "/api/metrics?$filter=Type eq 'ProviderRequests'&$orderby=Started desc&$skip=0&$top=50");
});

it("should add no filter clauses when nothing is searched for", () => {
    const url = buildRequestMetricQueryUrl(
        "/api/metrics",
        { skip: 0, take: 50 },
        { correlationId: "", fromDate: "", toDate: "" });

    expect(decode(url)).toContain("$filter=Type eq 'Request'&");
});

it("should search for one correlation id alongside the type", () => {
    const url = buildRequestMetricQueryUrl(
        "/api/metrics",
        { skip: 0, take: 50 },
        {
            correlationId: "  0f1c4d6b-9a2e-4f31-8c77-1b2a3c4d5e6f  ",
            fromDate: "",
            toDate: ""
        });

    expect(decode(url)).toContain(
        "$filter=Type eq 'Request' and CorrelationId eq 0f1c4d6b-9a2e-4f31-8c77-1b2a3c4d5e6f");
});

it("should widen a date range to whole days, with an inclusive upper bound", () => {
    const url = buildRequestMetricQueryUrl(
        "/api/metrics",
        { skip: 0, take: 50 },
        { correlationId: "", fromDate: "2026-08-24", toDate: "2026-08-25" });

    const filter = decode(url);

    // Picking the 25th means the whole of the 25th, not the instant it began.
    expect(filter).toContain("CreatedDate ge ");
    expect(filter).toContain("CreatedDate le ");
    expect(filter).toMatch(/CreatedDate ge 2026-08-2[34]T\d{2}:00:00\.000Z/);
    expect(filter).toMatch(/CreatedDate le 2026-08-2[56]T\d{2}:\d{2}:59\.999Z/);
});

it("should apply the same filter to the provider requests sample", () => {
    const url = buildProviderRequestsMetricQueryUrl(
        "/api/metrics",
        { skip: 0, take: 50 },
        { correlationId: "", fromDate: "2026-08-25", toDate: "" });

    const filter = decode(url);
    expect(filter).toContain("Type eq 'ProviderRequests'");
    expect(filter).toContain("CreatedDate ge ");
});

it("should ignore a part typed date rather than sending a broken literal", () => {
    const url = buildRequestMetricQueryUrl(
        "/api/metrics",
        { skip: 0, take: 50 },
        { correlationId: "", fromDate: "2026-08", toDate: "not-a-date" });

    expect(decode(url)).toContain("$filter=Type eq 'Request'&");
});
