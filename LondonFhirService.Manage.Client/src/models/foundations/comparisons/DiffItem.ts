// One entry inside a stored DiffJson. Mirrors
// LondonFhirService.Core.Models.Processings.ListEntryComparisons.DiffItem, which carries explicit
// [JsonPropertyName] attributes, so these names are the wire names rather than a camel cased
// guess at them.
export type DiffItem = {
    type: DiffItemType;
    path: string;
    oldValue: string | null;
    newValue: string | null;
    resourceType: string | null;
    identifier: string | null;
    reason: string | null;
};

// The comparison engine writes Type as free text, so an unrecognised kind has to survive the trip
// rather than be dropped or coerced into one of the known ones.
export type DiffItemType =
    | "modified"
    | "added"
    | "removed"
    | "manual-review-required"
    | "entry-count-mismatch"
    | string;
