import { ListGroup, Offcanvas } from "react-bootstrap";
import type { DiffItemProps } from "../../models/components/comparisons/DiffSidebarProps";
import type { DiffSidebarProps } from "../../models/components/comparisons/DiffSidebarProps";

// Every difference the comparison recorded, in the order the engine wrote them. The values are
// the raw JSON fragments it compared, so what is shown here is what it actually saw.
export function DiffSidebar({ show, onHide, diffs, correlationId }: DiffSidebarProps) {
    return (
        <Offcanvas show={show} onHide={onHide} placement="end" style={{ width: "500px" }}>
            <Offcanvas.Header closeButton>
                <Offcanvas.Title as="h2" className="h5">
                    Differences
                    <span className="text-muted ms-2 fs-6">({diffs.length} total)</span>
                </Offcanvas.Title>
            </Offcanvas.Header>

            <Offcanvas.Body>
                <div className="text-muted small mb-3">
                    Correlation id: <code>{correlationId}</code>
                </div>

                {diffs.length === 0
                    ? <div className="text-muted">No differences were recorded.</div>
                    : (
                        <ListGroup variant="flush">
                            {diffs.map(diff => (
                                <ListGroup.Item key={diff.key} className="px-0 py-3 border-bottom">
                                    <div
                                        className={"d-flex justify-content-between "
                                            + "align-items-start gap-2 mb-2"}>
                                        <span className="fw-bold text-break">{diff.path}</span>
                                        <span className={diff.typeClassName}>{diff.typeText}</span>
                                    </div>

                                    {diff.identifierText !== null && (
                                        <div className="small text-muted mb-2">
                                            {diff.resourceTypeText ?? "Resource"}:{" "}
                                            <code>{diff.identifierText}</code>
                                        </div>
                                    )}

                                    <DiffValues diff={diff} />
                                </ListGroup.Item>
                            ))}
                        </ListGroup>
                    )}
            </Offcanvas.Body>
        </Offcanvas>
    );
}

function DiffValues({ diff }: DiffItemProps) {
    if (diff.type === "manual-review-required") {
        return (
            <div className="small text-muted">
                {diff.reasonText ?? "The comparison could not decide this automatically."}
            </div>
        );
    }

    if (diff.type === "entry-count-mismatch") {
        return (
            <div className="small">
                <div className="text-muted mb-1">List size mismatch:</div>

                <div>
                    <span className="text-danger">{diff.oldValueText}</span>
                    <span className="text-muted mx-2">→</span>
                    <span className="text-success">{diff.newValueText}</span>
                    <span className="text-muted ms-2">items</span>
                </div>
            </div>
        );
    }

    if (diff.type === "added") {
        return (
            <div className="small">
                <div className="text-muted mb-1">Only in the secondary source:</div>
                <div className="text-success text-break">{diff.newValueText}</div>
            </div>
        );
    }

    if (diff.type === "removed") {
        return (
            <div className="small">
                <div className="text-muted mb-1">Only in the primary source:</div>
                <div className="text-danger text-break">{diff.oldValueText}</div>
            </div>
        );
    }

    return (
        <div className="small">
            <div className="text-muted mb-1">Primary:</div>
            <div className="text-danger text-break">{diff.oldValueText}</div>
            <div className="text-muted my-1">→</div>
            <div className="text-muted mb-1">Secondary:</div>
            <div className="text-success text-break">{diff.newValueText}</div>
        </div>
    );
}
