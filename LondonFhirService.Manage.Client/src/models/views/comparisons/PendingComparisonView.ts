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

    // Whether the compare queue could still plausibly pick this up, rather than merely whether it
    // is unprocessed. A record the queue will never claim - a primary with no secondary, which
    // nothing completes - stays unprocessed forever, so "is anything unprocessed" is not a
    // question that ever stops being true and cannot be what the page polls on.
    isAwaitingTheQueue: boolean;
};
