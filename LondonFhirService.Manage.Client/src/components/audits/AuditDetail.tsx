import { Card, Col, Row } from "react-bootstrap";
import type { AuditDetailProps } from "../../models/components/audits/AuditDetailProps";

export function AuditDetail({ audit }: AuditDetailProps) {
    return (
        <Card>
            <Card.Header className="d-flex flex-wrap align-items-center gap-2">
                <h2 className="h5 mb-0 me-auto">{audit.title}</h2>
                <span className={audit.logLevelClassName}>{audit.logLevelText}</span>
            </Card.Header>

            <Card.Body>
                <Row className="mb-3">
                    <Col md={6}>
                        <dl className="row mb-0">
                            <dt className="col-sm-5">Audit type</dt>
                            <dd className="col-sm-7">{audit.auditTypeText}</dd>

                            <dt className="col-sm-5">Correlation id</dt>
                            <dd className="col-sm-7 text-break">{audit.correlationIdText}</dd>

                            <dt className="col-sm-5">File name</dt>
                            <dd className="col-sm-7 text-break">{audit.fileNameText}</dd>
                        </dl>
                    </Col>

                    <Col md={6}>
                        <dl className="row mb-0">
                            <dt className="col-sm-5">Created</dt>
                            <dd className="col-sm-7">
                                {audit.createdDateText} by {audit.createdByText}
                            </dd>

                            <dt className="col-sm-5">Last updated</dt>
                            <dd className="col-sm-7">
                                {audit.updatedDateText} by {audit.updatedByText}
                            </dd>
                        </dl>
                    </Col>
                </Row>

                <h3 className="h6">Message</h3>
                <pre className="bg-light border rounded p-3 mb-0 text-break" style={{ whiteSpace: "pre-wrap" }}>
                    {audit.message}
                </pre>
            </Card.Body>

            <Card.Footer className="text-muted small text-break">
                Audit id: {audit.id}
            </Card.Footer>
        </Card>
    );
}
