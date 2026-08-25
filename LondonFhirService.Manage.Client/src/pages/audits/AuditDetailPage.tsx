import { Button, Col, Container, Row } from "react-bootstrap";
import { useParams } from "react-router-dom";
import BreadCrumbBase from "../../components/bases/layouts/BreadCrumb/BreadCrumbBase";
import { AuditDetail } from "../../components/audits/AuditDetail";
import { EmptyState } from "../../components/shared/EmptyState";
import { ErrorSummary } from "../../components/shared/ErrorSummary";
import { LoadingIndicator } from "../../components/shared/LoadingIndicator";
import { useAuditDetailPage } from "../../hooks/pages/useAuditDetailPage";

export function AuditDetailPage() {
    const { auditId } = useParams<{ auditId: string }>();
    const { audit, loading, error, handleBackToAudits } = useAuditDetailPage(auditId ?? "");

    const breadCrumb = (
        <BreadCrumbBase link="/admin/audits" backLink="Audits" currentLink={audit?.title ?? "Audit"} />
    );

    if (loading) {
        return (
            <Container fluid className="mt-4">
                {breadCrumb}
                <LoadingIndicator message="Loading audit..." />
            </Container>
        );
    }

    if (error) {
        return (
            <Container fluid className="mt-4">
                {breadCrumb}
                <ErrorSummary title="Audit could not be loaded" message={error.message} />
            </Container>
        );
    }

    if (audit === null) {
        return (
            <Container fluid className="mt-4">
                {breadCrumb}
                <EmptyState
                    title="Audit not found"
                    message="This audit no longer exists, or the link you followed is incomplete." />
            </Container>
        );
    }

    return (
        <Container fluid className="mt-4">
            {breadCrumb}

            <Row className="mb-3 p-2 align-items-center">
                <Col>
                    <h1 className="h3 mb-0">{audit.title}</h1>
                </Col>

                <Col xs="auto">
                    <Button variant="outline-secondary" onClick={handleBackToAudits}>
                        Back to audits
                    </Button>
                </Col>
            </Row>

            <Row className="p-2">
                <Col>
                    <AuditDetail audit={audit} />
                </Col>
            </Row>
        </Container>
    );
}
