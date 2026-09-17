import { cleanup, render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { afterEach, expect, it } from "vitest";
import { StructuredRecordPayload } from "./StructuredRecordPayload";
import type { StructuredRecordView } from "../../models/views/patients/StructuredRecordView";

// The header value as the API sends it: 32 hex digits, no dashes.
const correlationId = "d8924d9709dab2e07cf313bef9fdf820";
const dashedCorrelationId = "d8924d97-09da-b2e0-7cf3-13bef9fdf820";

const aStructuredRecord = (
    overrides: Partial<StructuredRecordView> = {})
    : StructuredRecordView => ({
    payloadText: "{\n  \"resourceType\": \"Bundle\"\n}",
    isJson: true,
    lineCount: 3,
    characterCountText: "30 characters",
    correlationId: correlationId,
    ...overrides
});

const renderPayload = (structuredRecord: StructuredRecordView) =>
    render(
        <MemoryRouter>
            <StructuredRecordPayload structuredRecord={structuredRecord} />
        </MemoryRouter>);

afterEach(cleanup);

// The metrics route filters on a Guid, and an OData guid literal carries its dashes - the compact
// form the header supplies does not merely miss, it fails to parse.
it("should link to the metrics for this call in the form that route accepts", () => {
    renderPayload(aStructuredRecord());

    expect(screen.getByRole("link", { name: /view metrics/i }).getAttribute("href"))
        .toBe(`/admin/metrics/${dashedCorrelationId}`);
});

// The comparisons search matches a string column holding the compact form.
it("should link to the comparisons for this call in the form that search matches", () => {
    renderPayload(aStructuredRecord());

    expect(screen.getByRole("link", { name: /view comparisons/i }).getAttribute("href"))
        .toBe(`/admin/comparisons?correlationId=${correlationId}`);
});

// It reads as a value to copy into a ticket or a log search now, with the two journeys named
// beside it rather than hidden behind it.
it("should show the correlation id as text rather than as a link", () => {
    renderPayload(aStructuredRecord());

    expect(screen.getByText(correlationId)).toBeTruthy();

    const links = screen.getAllByRole("link");
    expect(links).toHaveLength(2);
    expect(links.some(link => link.textContent?.includes(correlationId))).toBe(false);
});

// An older build of the host sends no such header, and both links would lead nowhere real.
it("should offer neither link when the host sent no correlation id", () => {
    renderPayload(aStructuredRecord({ correlationId: "" }));

    expect(screen.queryAllByRole("link")).toHaveLength(0);
    expect(screen.queryByText(/CorrelationId:/)).toBeNull();
});

it("should still show the payload and its summary", () => {
    renderPayload(aStructuredRecord());

    expect(screen.getByText("Formatted JSON")).toBeTruthy();
    expect(screen.getByText(/3 lines/)).toBeTruthy();

    expect(screen.getByLabelText("Structured record response").textContent)
        .toContain("resourceType");
});
