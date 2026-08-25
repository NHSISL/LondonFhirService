// The paging window a list request asks the API for. The master list shows root spans only, so
// there is nothing else to vary.
export type MetricQuery = {
    skip: number;
    take: number;
};
