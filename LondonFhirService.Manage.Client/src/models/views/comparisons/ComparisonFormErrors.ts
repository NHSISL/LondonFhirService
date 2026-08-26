// The resolution form has no server side field rules to mirror - Comment is unbounded and
// IsResolved is a flag - so the only thing that can fail is an acceptable count that does not fit
// the comparison it belongs to. That is checked against the row's own diff count, which the
// repository's generic validation rules cannot express, so this bag is filled in the page hook
// rather than by useValidation.
export type ComparisonFormErrors = {
    hasErrors: boolean;
    acceptableDiffCount: string;
};
