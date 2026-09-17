import type { ComparisonDetailView } from "../../../models/views/comparisons/ComparisonDetailView";
import type { ComparisonFormValues } from "../../../models/views/comparisons/ComparisonFormValues";
import type { ComparisonPageView } from "../../../models/views/comparisons/ComparisonPageView";
import type { PendingComparisonView } from "../../../models/views/comparisons/PendingComparisonView";

export interface IComparisonViewService {
    retrieveComparisonPageViewAsync(
        pageNumber: number,
        searchTerm: string,
        unresolvedOnly: boolean,
        abortSignal?: AbortSignal): Promise<ComparisonPageView>;

    retrievePendingComparisonViewsAsync(
        searchTerm: string,
        abortSignal?: AbortSignal): Promise<PendingComparisonView[]>;

    retrieveComparisonDetailViewAsync(
        fhirRecordDifferenceId: string,
        abortSignal?: AbortSignal): Promise<ComparisonDetailView>;

    createComparisonFormValues(): ComparisonFormValues;

    setDiffAcceptanceAsync(
        fhirRecordDifferenceId: string,
        diffIndexes: number[],
        acceptable: boolean): Promise<void>;

    updateComparisonAsync(
        fhirRecordDifferenceId: string,
        comparisonFormValues: ComparisonFormValues): Promise<void>;
}
