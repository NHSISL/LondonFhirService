import { useId } from "react";
import { Form } from "react-bootstrap";
import { getDiffState, getHighlightStyle, getInlineHighlightStyle } from "../../../helpers/comparisons/diffHighlighting";
import type { DiffAcceptance } from "../../../models/components/comparisons/DiffAcceptance";
import type { DiffItemView } from "../../../models/views/comparisons/DiffItemView";
import type { ReactNode } from "react";

type DiffHighlightProps = {
    fieldDiffs: DiffItemView[];
    acceptance: DiffAcceptance;
    children: ReactNode;

    // Inline for a field rendered inside an already indented panel, where the boxed variant's
    // padding would push the row out of line with its neighbours.
    inline?: boolean;
};

// Outlines a field the comparison found a difference in, and - on the secondary card only - offers
// the tick that records the difference as acceptable.
//
// The tick sits on the secondary because that is the side under question: the primary provider's
// answer is taken as correct, and the secondary's is what an administrator is judging. Acceptance
// itself belongs to the difference rather than to a side, so ticking here turns the box green on
// both cards at once.
//
// One box can cover more than one difference - an address whose line and city both changed maps to
// the same field - so the tick writes to all of them together and reads as ticked only when every
// one of them is accepted.
export function DiffHighlight({ fieldDiffs, acceptance, children, inline }: DiffHighlightProps) {
    // One difference can be shown in more than one box - an address appears both in its summary
    // line and in the expanded components - so the id has to come from the element rather than
    // from the difference it is about.
    const checkboxId = useId();

    const state = getDiffState(fieldDiffs);

    if (state === "none") {
        return <>{children}</>;
    }

    const style = inline ? getInlineHighlightStyle(state) : getHighlightStyle(state);
    const accepted = state === "accepted";

    if (acceptance.side === "primary") {
        return <div style={style}>{children}</div>;
    }

    const diffIndexes = fieldDiffs.map(fieldDiff => fieldDiff.index);

    return (
        <div style={style} className="d-flex justify-content-between align-items-start gap-2">
            <div className="flex-grow-1">{children}</div>

            <Form.Check
                type="checkbox"
                id={checkboxId}
                className="flex-shrink-0"
                checked={accepted}
                disabled={acceptance.saving}
                title={accepted
                    ? "This difference is recorded as acceptable. Untick to reopen it."
                    : "Tick to record this difference as acceptable."}
                aria-label={fieldDiffs.length === 1
                    ? "Acceptable difference"
                    : `Acceptable difference (${fieldDiffs.length} differences)`}
                onChange={event =>
                    acceptance.onToggleAcceptance(diffIndexes, event.currentTarget.checked)} />
        </div>
    );
}
