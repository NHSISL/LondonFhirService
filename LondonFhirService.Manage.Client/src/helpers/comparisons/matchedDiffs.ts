import type { DiffItemView } from "../../models/views/comparisons/DiffItemView";

// A resource has no stable id across the two providers, so its own differences are found by
// rebuilding the key the matcher on the server paired it on and matching against that instead.
//
// A "manual-review-required" entry is excluded even when its identifier matches: it means the
// engine could not pair that resource with a counterpart at all - most often because a provider
// reused the same identifier (e.g. the same DDS id) across more than one resource of that type,
// so several genuinely different, unrelated resources all carry the same identifierText. Folding
// one of those into this item's badge would let its checkbox accept a difference that belongs to
// a resource this card never shows - the only honest place for it is the full differences list,
// where it is ticked by its own index rather than by a key that turned out not to be unique.
export function getMatchedDiffs(diffs: DiffItemView[], matchKey: string | null): DiffItemView[] {
    return matchKey === null
        ? []
        : diffs.filter(diff =>
            diff.identifierText === matchKey && diff.type !== "manual-review-required");
}

// The manual-review-required entries getMatchedDiffs leaves out, for a caller that wants to show
// them anyway - not as a difference on this resource, but as a note that this resource's own key
// is one the engine could not resolve to a single match. Every resource whose own key produced one
// of these gets it, which for a key duplicated on both sides means every one of those resources -
// an honest "this may or may not be the one" rather than a guess landing the tick on just one of
// them.
export function getAmbiguousDiffs(diffs: DiffItemView[], matchKey: string | null): DiffItemView[] {
    return matchKey === null
        ? []
        : diffs.filter(diff =>
            diff.identifierText === matchKey && diff.type === "manual-review-required");
}
