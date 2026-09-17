export type ComparisonListItemView = {
    id: string;
    correlationId: string;
    sourceNameText: string;
    isPrimarySource: boolean;
    diffCountText: string;
    diffCountClassName: string;
    acceptableDiffCountText: string;
    acceptableDiffCountClassName: string;
    breakdownText: string;
    comparedAtText: string;
    resolutionText: string;
    resolutionClassName: string;
    commentText: string;
    detailUrl: string;

    // The same correlation on the metrics screen - how long each provider took, beside
    // whether they agreed.
    metricsUrl: string;
};
