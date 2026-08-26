import type { ComparisonDetailView } from "../../../models/views/comparisons/ComparisonDetailView";
import type { ComparisonFormValues } from "../../../models/views/comparisons/ComparisonFormValues";
import type { ComparisonPageView } from "../../../models/views/comparisons/ComparisonPageView";

export interface IComparisonViewService {
    retrieveComparisonPageViewAsync(
        pageNumber: number,
        searchTerm: string,
        unresolvedOnly: boolean,
        abortSignal?: AbortSignal): Promise<ComparisonPageView>;

    retrieveComparisonDetailViewAsync(
        fhirRecordDifferenceId: string,
        abortSignal?: AbortSignal): Promise<ComparisonDetailView>;

    createComparisonFormValues(): ComparisonFormValues;

    updateComparisonAsync(
        fhirRecordDifferenceId: string,
        comparisonFormValues: ComparisonFormValues): Promise<ComparisonDetailView>;
}
