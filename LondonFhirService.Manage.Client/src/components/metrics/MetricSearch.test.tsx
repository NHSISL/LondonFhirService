import { cleanup, fireEvent, render, screen } from "@testing-library/react";
import { useState } from "react";
import { afterEach, expect, it } from "vitest";
import { MetricSearch } from "./MetricSearch";
import type { MetricFilter } from "../../models/foundations/metrics/MetricFilter";
import type { MetricSearchProps } from "../../models/components/metrics/MetricSearchProps";

const noFilter: MetricFilter = { correlationId: "", userId: "", status: "", fromDate: "", toDate: "" };

const renderSearch = (overrides: Partial<MetricSearchProps> = {}) =>
    render(
        <MetricSearch
            filter={noFilter}
            correlationIdIsIncomplete={false}
            searching={false}
            loadedCount={60}
            exporting={false}
            onFilterChange={() => { }}
            onFilterClear={() => { }}
            onExport={() => { }}
            {...overrides} />);

afterEach(cleanup);

it("should export when the export button is pressed", () => {
    let exports = 0;
    renderSearch({ onExport: () => { exports++; } });

    fireEvent.click(screen.getByRole("button", { name: "Export to CSV" }));

    expect(exports).toBe(1);
    expect(screen.getByText("60 requests loaded")).toBeTruthy();
});

it("should hold the export back while one is running or the search is unsettled", () => {
    const { rerender } = renderSearch({ exporting: true });

    const exportingButton = screen.getByRole("button", { name: "Exporting..." }) as HTMLButtonElement;
    expect(exportingButton.disabled).toBe(true);

    rerender(
        <MetricSearch
            filter={{ ...noFilter, correlationId: "0f1c4d6b" }}
            correlationIdIsIncomplete={true}
            searching={false}
            loadedCount={60}
            exporting={false}
            onFilterChange={() => { }}
            onFilterClear={() => { }}
            onExport={() => { }} />);

    const exportButton = screen.getByRole("button", { name: "Export to CSV" }) as HTMLButtonElement;
    expect(exportButton.disabled).toBe(true);
});

it("should filter by user id and count it as a filter to clear", () => {
    const changes: [keyof MetricFilter, string][] = [];

    renderSearch({
        filter: { ...noFilter, userId: "2e9209fb" },
        onFilterChange: (fieldName, value) => { changes.push([fieldName, value]); }
    });

    fireEvent.change(screen.getByLabelText("User id"), {
        target: { value: "2e9209fb-25fe-4ed8-ba3d-a830d5fffb60" }
    });

    expect(changes).toEqual([["userId", "2e9209fb-25fe-4ed8-ba3d-a830d5fffb60"]]);

    const clearButton = screen.getByRole("button", { name: "Clear" }) as HTMLButtonElement;
    expect(clearButton.disabled).toBe(false);
});

it("should offer All, Succeeded and Failed and report the choice", () => {
    const changes: [keyof MetricFilter, string][] = [];

    // Holds the filter the way the page does. Rendered against a fixed filter, the controlled select
    // is reset by React after each change, and the test DOM reports that reset as a change of its own.
    function StatefulSearch() {
        const [filter, setFilter] = useState<MetricFilter>(noFilter);

        return (
            <MetricSearch
                filter={filter}
                correlationIdIsIncomplete={false}
                searching={false}
                loadedCount={60}
                exporting={false}
                onFilterChange={(fieldName, value) => {
                    changes.push([fieldName, value]);
                    setFilter(currentFilter => ({ ...currentFilter, [fieldName]: value }));
                }}
                onFilterClear={() => { }}
                onExport={() => { }} />);
    }

    render(<StatefulSearch />);

    const statusSelect = screen.getByLabelText("Status") as HTMLSelectElement;

    expect([...statusSelect.options].map(option => option.text)).toEqual(["All", "Succeeded", "Failed"]);
    expect(statusSelect.value).toBe("");

    fireEvent.change(statusSelect, { target: { value: "Failed" } });

    // How many change events the test DOM raises for one selection is its own business; what the
    // component reports is the point.
    expect(changes.length).toBeGreaterThan(0);
    expect(changes.every(([fieldName, value]) => fieldName === "status" && value === "Failed")).toBe(true);
    expect(statusSelect.value).toBe("Failed");
});

it("should count a chosen status as a filter to clear", () => {
    renderSearch({ filter: { ...noFilter, status: "Succeeded" } });

    const clearButton = screen.getByRole("button", { name: "Clear" }) as HTMLButtonElement;
    expect(clearButton.disabled).toBe(false);
});
