import { Badge, Card, Form } from "react-bootstrap";
import { Link } from "react-router-dom";
import { buildComparisonsUrl, buildMetricsUrl } from "../../helpers/correlationIds";
import type { StructuredRecordPayloadProps } from "../../models/components/patients/StructuredRecordPayloadProps";

export function StructuredRecordPayload({ structuredRecord }: StructuredRecordPayloadProps) {
    return (
        <Card>
            <Card.Header className="d-flex align-items-center justify-content-between flex-wrap gap-2">
                <span className="h6 mb-0 d-flex align-items-center flex-wrap gap-2">
                    Response

                    {/*
                        The two places this call left a trace, named rather than hidden behind the
                        id itself. The id used to be the link, which said nothing about where it
                        went and offered only one of the two destinations; it now reads as what it
                        is - a value to copy into a ticket or a log search - and the journeys are
                        spelled out beside it.

                        Rendered only when there is one. An older build of the host sends no
                        header, and both links would lead somewhere that cannot exist.
                    */}
                    {structuredRecord.correlationId.length > 0 && (
                        <span className="d-flex align-items-center flex-wrap gap-2 small fw-normal">
                            <Link
                                to={buildMetricsUrl(structuredRecord.correlationId)}
                                title="Every span recorded while this call ran">
                                View metrics
                            </Link>

                            <span className="text-muted" aria-hidden="true">|</span>

                            <Link
                                to={buildComparisonsUrl(structuredRecord.correlationId)}
                                title="See the comparisons this call produced">
                                View comparisons
                            </Link>

                            <span className="text-muted" aria-hidden="true">|</span>

                            <span className="text-muted">
                                CorrelationId:{" "}
                                <code className="text-body">
                                    {structuredRecord.correlationId}
                                </code>
                            </span>
                        </span>
                    )}
                </span>
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
