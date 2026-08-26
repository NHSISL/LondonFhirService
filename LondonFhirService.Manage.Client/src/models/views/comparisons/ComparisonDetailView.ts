import type { ComparisonFormValues } from "./ComparisonFormValues";
import type { ComparisonSourceView } from "./ComparisonSourceView";
import type { DiffItemView } from "./DiffItemView";

export type ComparisonDetailView = {
    id: string;
    correlationId: string;
    diffCount: number;
    diffCountText: string;
    diffCountClassName: string;

    // The count of differences ticked as acceptable, derived from the stored result rather than
    // read off the record, so the page cannot show a total that disagrees with the ticks under it.
    acceptableDiffCount: number;
    acceptableDiffCountText: string;
    acceptableDiffCountClassName: string;

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
