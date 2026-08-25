import { Col, Container, Row } from "react-bootstrap";
import BreadCrumbBase from "../../components/bases/layouts/BreadCrumb/BreadCrumbBase";
import { ErrorSummary } from "../../components/shared/ErrorSummary";
import { ProviderForm } from "../../components/providers/ProviderForm";
import { useProviderAddPage } from "../../hooks/pages/useProviderAddPage";

export function ProviderAddPage() {
    const {
        values,
        errors,
        saving,
        error,
        handleFieldChange,
        handleSubmit,
        handleCancel
    } = useProviderAddPage();

    return (
        <Container fluid className="mt-4">
            <BreadCrumbBase link="/admin/providers" backLink="Providers" currentLink="Add provider" />

            <Row className="mb-3 p-2">
                <Col>
                    <h1 className="h3 mb-1">Add provider</h1>
                    <p className="text-muted mb-0">
                        A new provider joins the patient fan-out as soon as it is active, so check
                        the endpoint and the active period before adding it.
                    </p>
                </Col>
            </Row>

            {error && (
                <Row className="mb-3 p-2">
                    <Col>
                        <ErrorSummary title="Provider could not be added" message={error.message} />
                    </Col>
                </Row>
            )}

            <Row className="p-2">
                <Col lg={9} xl={7}>
                    <ProviderForm
                        values={values}
                        errors={errors}
                        saving={saving}
                        submitLabel="Add provider"
                        savingLabel="Adding..."
                        onFieldChange={handleFieldChange}
                        onSubmit={handleSubmit}
                        onCancel={handleCancel} />
                </Col>
            </Row>
        </Container>
    );
}
