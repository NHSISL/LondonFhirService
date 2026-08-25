import { Card, Col, Row } from "react-bootstrap";
import { MetricDurationBars } from "./MetricDurationBars";
import type { MetricCorrelationSummaryProps } from "../../models/components/metrics/MetricCorrelationSummaryProps";

export function MetricCorrelationSummary({ correlation }: MetricCorrelationSummaryProps) {
    return (
        <Card className="mb-3">
            <Card.Header className="d-flex flex-wrap align-items-center gap-2">
                <h2 className="h5 mb-0 me-auto text-break">{correlation.methodText}</h2>
                <span className={correlation.statusClassName}>{correlation.statusText}</span>
            </Card.Header>

            <Card.Body>
                <Row>
                    <Col md={6}>
                        <dl className="row mb-0">
                            <dt className="col-sm-5">Started</dt>
                            <dd className="col-sm-7">{correlation.startedText}</dd>

                            <dt className="col-sm-5">Request duration</dt>
                            <dd className="col-sm-7">{correlation.durationText}</dd>

                            <dt className="col-sm-5">Provider requests</dt>
                            <dd className="col-sm-7">{correlation.providerRequestsText}</dd>

                            <dt className="col-sm-5">Proxy overhead</dt>
                            <dd className="col-sm-7">
                                {correlation.proxyOverheadText}
                                <span className="text-muted small">
                                    {" "}(request less provider requests)
                                </span>
                            </dd>
                        </dl>
                    </Col>

                    <Col md={6}>
                        <dl className="row mb-0">
                            <dt className="col-sm-5">Consumer</dt>
                            <dd className="col-sm-7 text-break">{correlation.consumerText}</dd>

                            <dt className="col-sm-5">User</dt>
                            <dd className="col-sm-7 text-break">{correlation.userIdText}</dd>

                            <dt className="col-sm-5">Spans recorded</dt>
                            <dd className="col-sm-7">{correlation.spanCount}</dd>
                        </dl>
                    </Col>
                </Row>

                {/* Full width on purpose: the bar is the same span of time as the figures above,
                    so tying it to one column would make it look like part of that column. */}
                <Row className="mt-3">
                    <Col>
                        <MetricDurationBars bars={correlation.bars} />
                    </Col>
                </Row>
            </Card.Body>

            <Card.Footer className="text-muted small text-break">
                Correlation id: {correlation.correlationId}
            </Card.Footer>
        </Card>
    );
}
