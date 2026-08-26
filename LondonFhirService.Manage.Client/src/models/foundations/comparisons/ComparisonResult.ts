import type { DiffItem } from "./DiffItem";

// The shape stored in FhirRecordDifference.DiffJson. Mirrors
// LondonFhirService.Core.Models.Orchestrations.Comparisons.ComparisonResult.
export type ComparisonResult = {
    correlationId: string;
    diffCount: number;
    diffs: DiffItem[];
};
