import type { ParsedBundle } from "../../foundations/fhir/ParsedBundle";

// One side of a comparison. The bundle is parsed once here rather than on every render, because
// the side by side view re-renders on each expand and each scroll sync.
export type ComparisonSourceView = {
    sourceName: string;
    roleText: string;
    roleClassName: string;
    statusText: string;
    statusClassName: string;
    createdDateText: string;
    formattedJsonPayload: string;
    bundle: ParsedBundle;
};
