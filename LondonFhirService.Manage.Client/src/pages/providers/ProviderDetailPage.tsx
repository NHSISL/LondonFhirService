import { Button, Col, Container, Row } from "react-bootstrap";
import { useParams } from "react-router-dom";
import BreadCrumbBase from "../../components/bases/layouts/BreadCrumb/BreadCrumbBase";
import { useProviderDetailPage } from "../../hooks/pages/useProviderDetailPage";
import { EmptyState } from "../../components/shared/EmptyState";
import { ErrorSummary } from "../../components/shared/ErrorSummary";
import { LoadingIndicator } from "../../components/shared/LoadingIndicator";
import { ProviderDetail } from "../../components/providers/ProviderDetail";
import { ProviderForm } from "../../components/providers/ProviderForm";
import { SecuredComponent } from "../../components/securitys/securedComponents";
import securityPoints from "../../securityMatrix";

export function ProviderDetailPage() {
    const { providerId } = useParams<{ providerId: string }>();

    const {
        provider,
        loading,
        error,
        editing,
        saving,
        saveError,
        values,
        errors,
        handleBackToProviders,
        handleEdit,
        handleFieldChange,
        handleSave,
        handleCancelEdit
    } = useProviderDetailPage(providerId ?? "");

    const breadCrumb = (
        <BreadCrumbBase link="/admin/providers" backLink="Providers" currentLink={provider?.friendlyName ?? "Provider"} />
    );

    if (loading) {
        return (
            <Container fluid className="mt-4">
                {breadCrumb}
                <LoadingIndicator message="Loading provider..." />
            </Container>
        );
    }

    if (error) {
        return (
            <Container fluid className="mt-4">
                {breadCrumb}
                <ErrorSummary title="Provider could not be loaded" message={error.message} />
            </Container>
        );
    }

    if (provider === null) {
        return (
            <Container fluid className="mt-4">
                {breadCrumb}
                <EmptyState
                    title="Provider not found"
                    message="This provider no longer exists, or the link you followed is incomplete." />
            </Container>
        );
    }

    return (
        <Container fluid className="mt-4">
            {breadCrumb}

            <Row className="mb-3 p-2 align-items-center">
                <Col>
                    <h1 className="h3 mb-0">{provider.friendlyName}</h1>
                </Col>

                {editing === false && (
                    <Col xs="auto" className="d-flex gap-2">
                        <SecuredComponent allowedRoles={securityPoints.providers.edit}>
                            <Button variant="primary" onClick={handleEdit}>
                                Edit
                            </Button>
                        </SecuredComponent>

                        <Button variant="outline-secondary" onClick={handleBackToProviders}>
                            Back to providers
                        </Button>
                    </Col>
                )}
            </Row>

            {saveError && (
                <Row className="mb-3 p-2">
                    <Col>
                        <ErrorSummary title="Provider could not be saved" message={saveError.message} />
                    </Col>
                </Row>
            )}

            <Row className="p-2">
                <Col lg={editing ? 9 : 12} xl={editing ? 7 : 12}>
                    {editing
                        ? (
                            <ProviderForm
                                values={values}
                                errors={errors}
                                saving={saving}
                                submitLabel="Save"
                                savingLabel="Saving..."
                                onFieldChange={handleFieldChange}
                                onSubmit={handleSave}
                                onCancel={handleCancelEdit} />
                        )
                        : <ProviderDetail provider={provider} />}
                </Col>
            </Row>
        </Container>
    );
}
