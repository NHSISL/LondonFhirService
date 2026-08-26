import { useState } from "react";
import { Button, Card, Modal } from "react-bootstrap";
import type { CSSProperties } from "react";

type PatientHeaderProps = {
    name: string;
    nhsNumber: string | null;
    sourceName: string;
    roleText: string;
    roleClassName: string;
    formattedJsonPayload: string;
    nameStyle: CSSProperties;
    nhsNumberStyle: CSSProperties;
};

export function PatientHeader({
    name,
    nhsNumber,
    sourceName,
    roleText,
    roleClassName,
    formattedJsonPayload,
    nameStyle,
    nhsNumberStyle
}: PatientHeaderProps) {
    const [showBundleJson, setShowBundleJson] = useState<boolean>(false);

    return (
        <>
            <Card.Header className="d-flex justify-content-between align-items-center gap-2">
                <div>
                    <div className="h5 mb-1" style={nameStyle}>{name}</div>

                    <div className="text-muted small" style={nhsNumberStyle}>
                        NHS: {nhsNumber ?? "N/A"}
                    </div>
                </div>

                <div className="d-flex align-items-center gap-2">
                    <span className={roleClassName}>{roleText}</span>

                    <Button
                        variant="outline-secondary"
                        size="sm"
                        onClick={() => setShowBundleJson(true)}>
                        Show all JSON
                    </Button>
                </div>
            </Card.Header>

            <Modal
                show={showBundleJson}
                onHide={() => setShowBundleJson(false)}
                size="lg"
                centered>
                <Modal.Header closeButton>
                    <Modal.Title as="h2" className="h5">
                        Full FHIR bundle — {sourceName}
                    </Modal.Title>
                </Modal.Header>

                <Modal.Body>
                    <pre
                        className="bg-light p-3 small"
                        style={{
                            maxHeight: "70vh",
                            overflow: "auto",
                            whiteSpace: "pre-wrap",
                            wordBreak: "break-word"
                        }}>
                        {formattedJsonPayload}
                    </pre>
                </Modal.Body>

                <Modal.Footer>
                    <Button variant="secondary" onClick={() => setShowBundleJson(false)}>
                        Close
                    </Button>
                </Modal.Footer>
            </Modal>
        </>
    );
}
