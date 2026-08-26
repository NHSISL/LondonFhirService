import { Form } from "react-bootstrap";
import { DiffHighlight } from "./DiffHighlight";
import type { DiffAcceptance } from "../../../models/components/comparisons/DiffAcceptance";
import type { DiffItemView } from "../../../models/views/comparisons/DiffItemView";

type OtherDiffListProps = {
    otherDiffs: DiffItemView[];
    acceptance: DiffAcceptance;
};

// The differences the card lays out no place for: a change inside the Patient that no field on it
// renders, or one against a resource type the card has no section for at all.
//
// It sits outside the collapsible Patient details panel, and outside any one section, because it
// is about the whole record - and because a difference that only appears behind a collapsed panel
// is not much better than one that does not appear. Without it the card shows fewer differences
// than the differences list counts, and the ones it drops cannot be ticked from the card.
export function OtherDiffList({ otherDiffs, acceptance }: OtherDiffListProps) {
    if (otherDiffs.length === 0) {
        return null;
    }

    return (
        <Form.Group className="mb-3">
            <Form.Label className="text-muted small mb-1">Other differences</Form.Label>

            <p className="text-muted small mb-2">
                Differences in parts of the record this card does not lay out. They are listed
                here so every difference can be seen and ticked.
            </p>

            {otherDiffs.map(otherDiff => (
                <DiffHighlight
                    key={otherDiff.key}
                    fieldDiffs={[otherDiff]}
                    acceptance={acceptance}>
                    <div className="small">
                        <div className="d-flex align-items-start gap-2 flex-wrap">
                            <code className="text-break">{otherDiff.path}</code>
                            <span className={otherDiff.typeClassName}>{otherDiff.typeText}</span>
                        </div>

                        <div className="text-break">
                            {(acceptance.side === "primary"
                                ? otherDiff.oldValueText
                                : otherDiff.newValueText)
                                ?? otherDiff.reasonText
                                ?? "N/A"}
                        </div>
                    </div>
                </DiffHighlight>
            ))}
        </Form.Group>
    );
}
