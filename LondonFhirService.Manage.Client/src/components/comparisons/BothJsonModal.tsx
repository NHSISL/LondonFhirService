import { useMemo, useRef } from "react";
import { Button, Col, Modal, Row } from "react-bootstrap";
import { JsonDiffPane } from "./JsonDiffPane";
import { alignLines, countChangedLines } from "../../helpers/comparisons/lineDiff";
import type { BothJsonModalProps } from "../../models/components/comparisons/BothJsonModalProps";
import type { ComparisonSide } from "../../helpers/comparisons/diffHighlighting";

// The raw bundles, side by side. The parsed cards deliberately show only what the comparison
// looks at, so this is where an operator goes when the difference is in something the cards do
// not render.
//
// The two are lined up on their common lines and the rows that do not match are highlighted, so
// a difference can be found by eye rather than by scrolling both and comparing by hand.
export function BothJsonModal({
    show,
    onHide,
    primarySource,
    secondarySource,
    syncScrollEnabled,
    onToggleSyncScroll
}: BothJsonModalProps) {
    const primaryPane = useRef<HTMLDivElement>(null);
    const secondaryPane = useRef<HTMLDivElement>(null);

    const primaryText = primarySource?.formattedJsonPayload ?? "";
    const secondaryText = secondarySource?.formattedJsonPayload ?? "";
    const hasBothSources = primarySource !== null && secondarySource !== null;

    // Aligning two whole bundles is not free, so it is done once per pair rather than on every
    // render - and only while the modal is open, since it is closed most of the time.
    const alignedLines = useMemo(
        () => show && hasBothSources ? alignLines(primaryText, secondaryText) : null,
        [show, hasBothSources, primaryText, secondaryText]);

    const changedLineCount = alignedLines === null ? 0 : countChangedLines(alignedLines);

    // Sideways as well as down: the panes do not wrap, so a pane scrolled right has to take the
    // other with it or the two stop showing the same columns.
    const handleScroll = (scrolledPane: ComparisonSide) => {
        if (syncScrollEnabled === false) {
            return;
        }

        const primaryElement = primaryPane.current;
        const secondaryElement = secondaryPane.current;

        if (primaryElement === null || secondaryElement === null) {
            return;
        }

        const from = scrolledPane === "primary" ? primaryElement : secondaryElement;
        const to = scrolledPane === "primary" ? secondaryElement : primaryElement;

        to.scrollTop = from.scrollTop;
        to.scrollLeft = from.scrollLeft;
    };

    return (
        <Modal show={show} onHide={onHide} size="xl" centered>
            <Modal.Header closeButton>
                <div className="d-flex justify-content-between align-items-center w-100 me-4">
                    <Modal.Title as="h2" className="h5">
                        Full FHIR bundles — both sources
                    </Modal.Title>

                    <Button
                        variant={syncScrollEnabled ? "outline-primary" : "outline-secondary"}
                        size="sm"
                        onClick={onToggleSyncScroll}
                        aria-pressed={syncScrollEnabled}>
                        {syncScrollEnabled ? "Sync scrolling on" : "Sync scrolling off"}
                    </Button>
                </div>
            </Modal.Header>

            <Modal.Body>
                <p className="text-muted small" aria-live="polite">
                    {alignedLines === null
                        ? "These two payloads are too far apart to line up, so they are shown "
                        + "in full without highlighting."
                        : `The two are lined up on the lines they share. `
                        + `${changedLineCount === 1
                            ? "1 line differs"
                            : `${changedLineCount} lines differ`} `
                        + "and is highlighted on both sides."}
                </p>

                <Row>
                    <Col xs={6}>
                        <div className="mb-2 fw-bold">
                            {primarySource?.sourceName ?? "Primary source"}
                        </div>

                        {alignedLines === null
                            ? <PlainJsonPane text={primarySource?.formattedJsonPayload ?? null} />
                            : (
                                <JsonDiffPane
                                    alignedLines={alignedLines}
                                    side="primary"
                                    paneRef={primaryPane}
                                    onScroll={() => handleScroll("primary")} />
                            )}
                    </Col>

                    <Col xs={6}>
                        <div className="mb-2 fw-bold">
                            {secondarySource?.sourceName ?? "Secondary source"}
                        </div>

                        {alignedLines === null
                            ? <PlainJsonPane text={secondarySource?.formattedJsonPayload ?? null} />
                            : (
                                <JsonDiffPane
                                    alignedLines={alignedLines}
                                    side="secondary"
                                    paneRef={secondaryPane}
                                    onScroll={() => handleScroll("secondary")} />
                            )}
                    </Col>
                </Row>
            </Modal.Body>

            <Modal.Footer>
                <Button variant="secondary" onClick={onHide}>Close</Button>
            </Modal.Footer>
        </Modal>
    );
}

// The fallback: one payload on its own, unaligned. Used when a record is missing altogether, and
// when the two are too far apart to line up.
function PlainJsonPane({ text }: { text: string | null }) {
    return (
        <pre
            className="bg-light p-3 small"
            style={{
                maxHeight: "70vh",
                overflow: "auto",
                whiteSpace: "pre-wrap",
                wordBreak: "break-word"
            }}>
            {text ?? "Not available"}
        </pre>
    );
}
