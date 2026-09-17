// What the page renders after a successful call. The payload is held as formatted text rather
// than as a parsed object: the point of the screen is to show what the provider actually
// answered, and a bundle that is not valid JSON must still be visible rather than disappearing
// into a parse error.
export type StructuredRecordView = {
    payloadText: string;
    isJson: boolean;
    lineCount: number;
    characterCountText: string;

    // The id the API filed the call under, or empty when it did not send one. The page shows it
    // so an operator can follow the same call into the comparisons it produced.
    correlationId: string;
};
