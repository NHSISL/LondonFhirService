import type { MetricFilter } from "../../foundations/metrics/MetricFilter";

export type MetricSearchProps = {
    filter: MetricFilter;
    correlationIdIsIncomplete: boolean;
    searching: boolean;
    loadedCount: number;
    exporting: boolean;
    onFilterChange: (fieldName: keyof MetricFilter, value: string) => void;
    onFilterClear: () => void;
    onExport: () => void;
};
