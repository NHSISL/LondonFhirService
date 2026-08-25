import { Card, Col, Row } from "react-bootstrap";
import type { ProviderDetailProps } from "../../models/components/providers/ProviderDetailProps";

export function ProviderDetail({ provider }: ProviderDetailProps) {
    return (
        <Card>
            <Card.Header className="d-flex flex-wrap align-items-center gap-2">
                <h2 className="h5 mb-0 me-auto">{provider.friendlyName}</h2>
                <span className={provider.roleClassName}>{provider.roleText}</span>
                <span className={provider.statusClassName}>{provider.statusText}</span>
            </Card.Header>

            <Card.Body>
                <Row>
                    <Col md={6}>
                        <dl className="row mb-0">
                            <dt className="col-sm-5">Fully qualified name</dt>
                            <dd className="col-sm-7 text-break">{provider.fullyQualifiedName}</dd>

                            <dt className="col-sm-5">FHIR version</dt>
                            <dd className="col-sm-7">{provider.fhirVersionText}</dd>

                            <dt className="col-sm-5">Primary provider</dt>
                            <dd className="col-sm-7">{provider.isPrimaryText}</dd>

                            <dt className="col-sm-5">Comparison only</dt>
                            <dd className="col-sm-7">{provider.isForComparisonOnlyText}</dd>
                        </dl>
                    </Col>

                    <Col md={6}>
                        <dl className="row mb-0">
                            <dt className="col-sm-5">Active from</dt>
                            <dd className="col-sm-7">{provider.activeFromText}</dd>

                            <dt className="col-sm-5">Active to</dt>
                            <dd className="col-sm-7">{provider.activeToText}</dd>

                            <dt className="col-sm-5">Created</dt>
                            <dd className="col-sm-7">
                                {provider.createdDateText} by {provider.createdBy}
                            </dd>

                            <dt className="col-sm-5">Last updated</dt>
                            <dd className="col-sm-7">
                                {provider.updatedDateText} by {provider.updatedBy}
                            </dd>
                        </dl>
                    </Col>
                </Row>
            </Card.Body>

            <Card.Footer className="text-muted small text-break">
                Provider id: {provider.id}
            </Card.Footer>
        </Card>
    );
}
