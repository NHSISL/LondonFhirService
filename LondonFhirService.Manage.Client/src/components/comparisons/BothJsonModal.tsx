import { useRef } from "react";
import { Button, Col, Modal, Row } from "react-bootstrap";
import type { BothJsonModalProps } from "../../models/components/comparisons/BothJsonModalProps";

// The raw bundles, side by side. The parsed cards deliberately show only what the comparison
// looks at, so this is where an operator goes when the difference is in something the cards do
// not render.
export function BothJsonModal({
    show,
    onHide,
    primarySource,
    secondarySource,
    syncScrollEnabled,
    onToggleSyncScroll
}: BothJsonModalProps) {
    const primaryJson = useRef<HTMLPreElement>(null);
    const secondaryJson = useRef<HTMLPreElement>(null);

    const handleScroll = (scrolledPanel: "primary" | "secondary") => {
        if (syncScrollEnabled === false) {
            return;
        }

        const primaryElement = primaryJson.current;
        const secondaryElement = secondaryJson.current;

        if (primaryElement === null || secondaryElement === null) {
            return;
        }

        if (scrolledPanel === "primary") {
            secondaryElement.scrollTop = primaryElement.scrollTop;
        } else {
            primaryElement.scrollTop = secondaryElement.scrollTop;
        }
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
                <Row>
                    <Col xs={6}>
                        <div className="mb-2 fw-bold">
                            {primarySource?.sourceName ?? "Primary source"}
                        </div>

                        <pre
                            ref={primaryJson}
                            onScroll={() => handleScroll("primary")}
                            className="bg-light p-3 small"
                            style={{
                                maxHeight: "70vh",
                                overflow: "auto",
                                whiteSpace: "pre-wrap",
                                wordBreak: "break-word"
                            }}>
                            {primarySource?.formattedJsonPayload ?? "Not available"}
                        </pre>
                    </Col>

                    <Col xs={6}>
                        <div className="mb-2 fw-bold">
                            {secondarySource?.sourceName ?? "Secondary source"}
                        </div>

                        <pre
                            ref={secondaryJson}
                            onScroll={() => handleScroll("secondary")}
                            className="bg-light p-3 small"
                            style={{
                                maxHeight: "70vh",
                                overflow: "auto",
                                whiteSpace: "pre-wrap",
                                wordBreak: "break-word"
                            }}>
                            {secondarySource?.formattedJsonPayload ?? "Not available"}
                        </pre>
                    </Col>
                </Row>
            </Modal.Body>

            <Modal.Footer>
                <Button variant="secondary" onClick={onHide}>Close</Button>
            </Modal.Footer>
        </Modal>
    );
}
