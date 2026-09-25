import { expect, it } from "vitest";
import {
    buildCorrelationMetricQueryUrl,
    buildMetricExportUrl,
    buildProviderRequestsByCorrelationIdsQueryUrl,
    buildMetricAveragesQueryUrl,
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

it("should ask the database to average every request and provider requests span by type", () => {
    const url = buildMetricAveragesQueryUrl("/api/metrics");

    expect(decode(url)).toBe(
        "/api/metrics?$apply=filter(Type eq 'Request' or Type eq 'ProviderRequests')"
        + "/groupby((Type),aggregate($count as SpanCount,DurationMs with average as AverageDurationMs))");

    // Unpaged and unfiltered: the all-requests tile is the whole table, not a sample of it.
    expect(url).not.toContain("$top");
    expect(url).not.toContain("CreatedDate");
});

it("should add no filter clauses when nothing is searched for", () => {
    const url = buildRequestMetricQueryUrl(
        "/api/metrics",
        { skip: 0, take: 50 },
        { correlationId: "", userId: "", fromDate: "", toDate: "" });

    expect(decode(url)).toContain("$filter=Type eq 'Request'&");
});

it("should search for one correlation id alongside the type", () => {
    const url = buildRequestMetricQueryUrl(
        "/api/metrics",
        { skip: 0, take: 50 },
        {
            correlationId: "  0f1c4d6b-9a2e-4f31-8c77-1b2a3c4d5e6f  ",
            userId: "",
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
        { correlationId: "", userId: "", fromDate: "2026-08-24", toDate: "2026-08-25" });

    const filter = decode(url);

    // Picking the 25th means the whole of the 25th, not the instant it began.
    expect(filter).toContain("CreatedDate ge ");
    expect(filter).toContain("CreatedDate le ");
    expect(filter).toMatch(/CreatedDate ge 2026-08-2[34]T\d{2}:00:00\.000Z/);
    expect(filter).toMatch(/CreatedDate le 2026-08-2[56]T\d{2}:\d{2}:59\.999Z/);
});

it("should ignore a part typed date rather than sending a broken literal", () => {
    const url = buildRequestMetricQueryUrl(
        "/api/metrics",
        { skip: 0, take: 50 },
        { correlationId: "", userId: "", fromDate: "2026-08", toDate: "not-a-date" });

    expect(decode(url)).toContain("$filter=Type eq 'Request'&");
});

it("should ask for the provider request spans of several correlations in one call", () => {
    const url = buildProviderRequestsByCorrelationIdsQueryUrl(
        "/api/metrics",
        ["0f1c4d6b-9a2e-4f31-8c77-1b2a3c4d5e6f", "7b9fc741-1bc7-3d31-61c8-09bf7e820df4"]);

    expect(decode(url)).toBe(
        "/api/metrics?$filter=Type eq 'ProviderRequests' and CorrelationId in "
        + "(0f1c4d6b-9a2e-4f31-8c77-1b2a3c4d5e6f,7b9fc741-1bc7-3d31-61c8-09bf7e820df4)"
        + "&$top=2");
});

it("should encode the correlation ids so they cannot smuggle in query options", () => {
    const url = buildProviderRequestsByCorrelationIdsQueryUrl(
        "/api/metrics",
        ["abc)&$top=9999"]);

    expect(url.split("$top=").length).toBe(2);
});

it("should search for one user id as an escaped string literal", () => {
    const url = buildRequestMetricQueryUrl(
        "/api/metrics",
        { skip: 0, take: 50 },
        { correlationId: "", userId: "  o'brien  ", fromDate: "", toDate: "" });

    expect(decode(url)).toContain("$filter=Type eq 'Request' and UserId eq 'o''brien'");
});

it("should ask for the export with no parameters when nothing is searched for", () => {
    const url = buildMetricExportUrl(
        "/api/metrics",
        { correlationId: "", userId: "", fromDate: "", toDate: "" });

    expect(url).toBe("/api/metrics/exports");
});

it("should ask for the export with the list's filter as query parameters", () => {
    const url = buildMetricExportUrl(
        "/api/metrics",
        {
            correlationId: " 0f1c4d6b-9a2e-4f31-8c77-1b2a3c4d5e6f ",
            userId: " 2e9209fb-25fe-4ed8-ba3d-a830d5fffb60 ",
            fromDate: "2026-08-24",
            toDate: "2026-08-25"
        });

    const parameters = new URL(url, "https://portal.example").searchParams;

    expect(url.startsWith("/api/metrics/exports?")).toBe(true);
    expect(parameters.get("correlationId")).toBe("0f1c4d6b-9a2e-4f31-8c77-1b2a3c4d5e6f");
    expect(parameters.get("userId")).toBe("2e9209fb-25fe-4ed8-ba3d-a830d5fffb60");

    // Widened to whole local days, exactly as the list's OData filter is.
    expect(parameters.get("fromDate")).toMatch(/^2026-08-2[34]T\d{2}:00:00\.000Z$/);
    expect(parameters.get("toDate")).toMatch(/^2026-08-2[56]T\d{2}:\d{2}:59\.999Z$/);
});
