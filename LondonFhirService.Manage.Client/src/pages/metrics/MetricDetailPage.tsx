import { Button, Col, Container, Row } from "react-bootstrap";
import { useParams } from "react-router-dom";
import BreadCrumbBase from "../../components/bases/layouts/BreadCrumb/BreadCrumbBase";
import { EmptyState } from "../../components/shared/EmptyState";
import { ErrorSummary } from "../../components/shared/ErrorSummary";
import { LoadingIndicator } from "../../components/shared/LoadingIndicator";
import { MetricCorrelationSummary } from "../../components/metrics/MetricCorrelationSummary";
import { MetricSpanList } from "../../components/metrics/MetricSpanList";
import { MetricTimeline } from "../../components/metrics/MetricTimeline";
import { useMetricDetailPage } from "../../hooks/pages/useMetricDetailPage";

export function MetricDetailPage() {
    const { correlationId } = useParams<{ correlationId: string }>();

    const { correlation, loading, error, handleBackToMetrics } =
        useMetricDetailPage(correlationId ?? "");

    const breadCrumb = (
        <BreadCrumbBase
            link="/admin/metrics"
            backLink="Metrics"
            currentLink={correlation?.methodText ?? "Request"} />
    );

    if (loading) {
        return (
            <Container fluid className="mt-4">
                {breadCrumb}
                <LoadingIndicator message="Loading request metrics..." />
            </Container>
        );
    }

    if (error) {
        return (
            <Container fluid className="mt-4">
                {breadCrumb}
                <ErrorSummary title="Request metrics could not be loaded" message={error.message} />
            </Container>
        );
    }

    if (correlation === null || correlation.spanCount === 0) {
        return (
            <Container fluid className="mt-4">
                {breadCrumb}
                <EmptyState
                    title="Request not found"
                    message="Nothing was recorded against this correlation id, or it has been purged." />
            </Container>
        );
    }

    return (
        <Container fluid className="mt-4">
            {breadCrumb}

            <Row className="mb-3 p-2 align-items-center">
                <Col>
                    <h1 className="h3 mb-0 text-break">{correlation.methodText}</h1>
                </Col>

                <Col xs="auto">
                    <Button variant="outline-secondary" onClick={handleBackToMetrics}>
                        Back to metrics
                    </Button>
                </Col>
            </Row>

            <Row className="p-2">
                <Col>
                    <MetricCorrelationSummary correlation={correlation} />
                    <MetricTimeline correlation={correlation} />
                    <MetricSpanList spans={correlation.spans} />
                </Col>
            </Row>
        </Container>
    );
}
