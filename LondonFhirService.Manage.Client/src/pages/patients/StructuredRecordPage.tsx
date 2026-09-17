import { Col, Container, Row } from "react-bootstrap";
import { ErrorSummary } from "../../components/shared/ErrorSummary";
import { LoadingIndicator } from "../../components/shared/LoadingIndicator";
import { StructuredRecordForm } from "../../components/patients/StructuredRecordForm";
import { StructuredRecordPayload } from "../../components/patients/StructuredRecordPayload";
import { useStructuredRecordPage } from "../../hooks/pages/useStructuredRecordPage";

export function StructuredRecordPage() {
    const {
        values,
        errors,
        structuredRecord,
        submitting,
        error,
        handleFieldChange,
        handleSubmit,
        handleClear
    } = useStructuredRecordPage();

    return (
        <Container fluid className="mt-4">
            <Row className="mb-3 p-2">
                <Col>
                    <h1 className="h3 mb-1">Get Structured Record</h1>
                    <p className="text-muted mb-0">
                        Calls $getstructuredrecord as a consumer would and shows the response
                        exactly as it came back. Nothing is saved - the record is fetched live for
                        this screen and is gone when you leave it.
                    </p>
                </Col>
            </Row>

            {error && (
                <Row className="mb-3 p-2">
                    <Col>
                        <ErrorSummary
                            title="Structured record could not be retrieved"
                            message={error.message} />
                    </Col>
                </Row>
            )}

            <Row className="p-2">
                <Col lg={9} xl={7}>
                    <StructuredRecordForm
                        values={values}
                        errors={errors}
                        submitting={submitting}
                        onFieldChange={handleFieldChange}
                        onSubmit={handleSubmit}
                        onClear={handleClear} />
                </Col>
            </Row>

            {submitting && (
                <Row className="p-2">
                    <Col>
                        <LoadingIndicator message="Retrieving the structured record..." />
                    </Col>
                </Row>
            )}

            {structuredRecord && (
                <Row className="p-2">
                    <Col>
                        <StructuredRecordPayload structuredRecord={structuredRecord} />
                    </Col>
                </Row>
            )}
        </Container>
    );
}
