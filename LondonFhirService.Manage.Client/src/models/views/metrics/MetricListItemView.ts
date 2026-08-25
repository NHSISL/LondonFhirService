// One root request span, as the master list renders it.
export type MetricListItemView = {
    id: string;
    correlationId: string;
    startedText: string;
    methodText: string;
    nameText: string;
    statusText: string;
    statusClassName: string;
    durationText: string;
    consumerText: string;
    userIdText: string;
    detailUrl: string;
};
