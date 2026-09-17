import { expect, it } from "vitest";
import {
    buildComparisonsUrl,
    buildMetricsUrl,
    toCompactCorrelationId,
    toDashedCorrelationId
} from "./correlationIds";

// The two spellings of one real correlation, taken from a record and its metrics.
const compact = "d8924d9709dab2e07cf313bef9fdf820";
const dashed = "d8924d97-09da-b2e0-7cf3-13bef9fdf820";

it("should dash a correlation id a record handed over undashed", () => {
    expect(toDashedCorrelationId(compact)).toBe(dashed);
});

it("should leave an already dashed correlation id alone", () => {
    expect(toDashedCorrelationId(dashed)).toBe(dashed);
});

it("should undash a correlation id the metrics screen handed over dashed", () => {
    expect(toCompactCorrelationId(dashed)).toBe(compact);
});

it("should leave an already compact correlation id alone", () => {
    expect(toCompactCorrelationId(compact)).toBe(compact);
});

it("should trim before deciding", () => {
    expect(toDashedCorrelationId(`  ${compact}  `)).toBe(dashed);
    expect(toCompactCorrelationId(`  ${dashed}  `)).toBe(compact);
});

// A formatter, not a validator. Something that is not a correlation id goes through untouched so
// the route reports that it found nothing, rather than this reshaping it into a value that looks
// valid, resolves to someone else's request, or silently drops characters.
it("should not reshape a value that is not a correlation id", () => {
    expect(toDashedCorrelationId("not-an-id")).toBe("not-an-id");
    expect(toDashedCorrelationId("")).toBe("");
    expect(toDashedCorrelationId("d8924d9709dab2e07cf313bef9fdf82")).toBe(
        "d8924d9709dab2e07cf313bef9fdf82");

    expect(toCompactCorrelationId("not-an-id")).toBe("not-an-id");
    expect(toCompactCorrelationId("")).toBe("");
});

// The whole point of the pair. An OData guid literal is defined with its dashes, so the metrics
// filter does not merely miss on the compact form - it fails to parse.
it("should send the metrics route the dashed form whichever it was given", () => {
    expect(buildMetricsUrl(compact)).toBe(`/admin/metrics/${dashed}`);
    expect(buildMetricsUrl(dashed)).toBe(`/admin/metrics/${dashed}`);
});

// And the comparisons search matches on a string column holding the compact form, where a dashed
// needle never appears.
it("should send the comparisons search the compact form whichever it was given", () => {
    expect(buildComparisonsUrl(dashed)).toBe(`/admin/comparisons?correlationId=${compact}`);
    expect(buildComparisonsUrl(compact)).toBe(`/admin/comparisons?correlationId=${compact}`);
});

// The value reaches these from a response header and from an api payload, so it is encoded rather
// than trusted to be url safe.
it("should encode a value it did not recognise", () => {
    expect(buildMetricsUrl("a b&c=d")).toBe("/admin/metrics/a%20b%26c%3Dd");
    expect(buildComparisonsUrl("a b&c=d")).toBe("/admin/comparisons?correlationId=a%20b%26c%3Dd");
});
