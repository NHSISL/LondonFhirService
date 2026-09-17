import { Badge, Card, Form } from "react-bootstrap";
import type { StructuredRecordPayloadProps } from "../../models/components/patients/StructuredRecordPayloadProps";

export function StructuredRecordPayload({ structuredRecord }: StructuredRecordPayloadProps) {
    return (
        <Card>
            <Card.Header className="d-flex align-items-center justify-content-between">
                <span className="h6 mb-0">Response</span>
                <span>
                    <Badge bg={structuredRecord.isJson ? "success" : "warning"} className="me-2">
                        {structuredRecord.isJson ? "Formatted JSON" : "Not JSON"}
                    </Badge>
                    <small className="text-muted">
                        {structuredRecord.lineCount.toLocaleString()} lines,{" "}
                        {structuredRecord.characterCountText}
                    </small>
                </span>
            </Card.Header>
            <Card.Body>
                {/*
                    Read only and monospaced. The payload is what the provider answered, so it is
                    shown rather than edited - and a bundle only reads as a bundle when its
                    indentation lines up, which a proportional font destroys.
                */}
                <Form.Control
                    as="textarea"
                    id="structuredRecordPayload"
                    name="structuredRecordPayload"
                    aria-label="Structured record response"
                    readOnly
                    rows={25}
                    spellCheck={false}
                    wrap="off"
                    className="font-monospace small"
                    value={structuredRecord.payloadText} />
            </Card.Body>
        </Card>
    );
}
