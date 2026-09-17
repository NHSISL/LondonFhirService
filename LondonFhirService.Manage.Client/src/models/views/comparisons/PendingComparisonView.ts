// One record that has landed but has not been compared yet - a row of the compare queue rather
// than a result. A correlation contributes one of these per provider that has answered, which is
// what makes a half arrived request legible: two rows here and a third still missing says the
// queue is waiting on a provider, not on the worker.
export type PendingComparisonView = {
    id: string;
    correlationId: string;
    sourceNameText: string;
    isPrimarySource: boolean;
    statusText: string;
    statusClassName: string;
    landedAtText: string;
};
