// The choices the Status filter offers. Only the two outcomes a person filters by: the other
// MetricStatus values are spans that timed out, were cancelled or were skipped, which a root request
// span does not end as.
export const metricStatusFilterOptions: { value: string; label: string }[] = [
    { value: "", label: "All" },
    { value: "Succeeded", label: "Succeeded" },
    { value: "Failed", label: "Failed" }
];
