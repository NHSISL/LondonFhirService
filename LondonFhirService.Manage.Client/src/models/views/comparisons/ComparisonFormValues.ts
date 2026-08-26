// What an operator can change about a comparison. Everything else on the record is written by the
// comparison coordination service and is read only here.
export type ComparisonFormValues = {
    comment: string;
    isResolved: boolean;
    acceptableDiffCount: string;
};
