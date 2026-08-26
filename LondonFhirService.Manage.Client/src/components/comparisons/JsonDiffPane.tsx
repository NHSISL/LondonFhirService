import type { AlignedLine } from "../../helpers/comparisons/lineDiff";
import type { ComparisonSide } from "../../helpers/comparisons/diffHighlighting";
import type { RefObject, UIEventHandler } from "react";

type JsonDiffPaneProps = {
    alignedLines: AlignedLine[];
    side: ComparisonSide;
    paneRef: RefObject<HTMLDivElement>;
    onScroll: UIEventHandler<HTMLDivElement>;
};

// Bootstrap's warning-subtle. Inline because it is set per row rather than by a class on a
// component, and there are a lot of rows.
const changedBackground = "#fff3cd";

// One side of the raw payload, rendered from the same aligned rows as the other side so the two
// stay level: where one has a line the other does not, the other gets a blank in its place. That
// is what lets a reader put the two lines of a difference next to each other, and what makes
// synchronised scrolling land on the same content in both panes.
//
// Lines are not wrapped. A wrapped line is two rows tall on one side and one on the other, which
// would throw the alignment out; the pane scrolls sideways instead.
export function JsonDiffPane({ alignedLines, side, paneRef, onScroll }: JsonDiffPaneProps) {
    return (
        <div
            ref={paneRef}
            onScroll={onScroll}
            className="bg-light p-3 small font-monospace border rounded"
            style={{ maxHeight: "70vh", overflow: "auto", whiteSpace: "pre" }}>
            {alignedLines.map((alignedLine, index) => {
                const text = side === "primary"
                    ? alignedLine.primaryText
                    : alignedLine.secondaryText;

                return (
                    <div
                        key={index}
                        style={alignedLine.changed
                            ? { backgroundColor: changedBackground }
                            : undefined}>
                        {/* A zero width space keeps an empty row the same height as a full one,
                            which is what holds the two panes level. */}
                        {text ?? "​"}
                    </div>
                );
            })}
        </div>
    );
}
