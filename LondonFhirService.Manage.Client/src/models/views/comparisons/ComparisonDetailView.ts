import type { ComparisonFormValues } from "./ComparisonFormValues";
import type { ComparisonSourceView } from "./ComparisonSourceView";
import type { DiffItemView } from "./DiffItemView";

export type ComparisonDetailView = {
    id: string;
    correlationId: string;
    diffCount: number;
    diffCountText: string;
    diffCountClassName: string;
    acceptableDiffCountText: string;
    outstandingDiffCountText: string;
    breakdownText: string;
    comparedAtText: string;
    resolutionText: string;
    resolutionClassName: string;
    commentText: string;
    updatedByText: string;
    updatedDateText: string;
    diffs: DiffItemView[];

    // Either side can be missing - a record can be deleted while its comparison is still on the
    // shelf - so the page renders whichever sides it has rather than failing outright.
    primarySource: ComparisonSourceView | null;
    secondarySource: ComparisonSourceView | null;
    sourcesError: string | null;

    editValues: ComparisonFormValues;
};
