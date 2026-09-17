// What the comparisons page asks the records endpoint for: the records still in the compare
// queue, optionally narrowed to one correlation. There is no paging here on purpose - the queue
// is meant to be short, and a take that fills means the backlog itself is the thing to look at.
export type FhirRecordQuery = {
    take: number;
    searchTerm: string;
};
