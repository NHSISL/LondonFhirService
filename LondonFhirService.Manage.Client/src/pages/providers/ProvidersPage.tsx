import { Col, Container, Row } from "react-bootstrap";
import { useProvidersPage } from "../../hooks/pages/useProvidersPage";
import { ErrorSummary } from "../../components/shared/ErrorSummary";
import { LoadingIndicator } from "../../components/shared/LoadingIndicator";
import { ProviderList } from "../../components/providers/ProviderList";
import { ProviderSearch } from "../../components/providers/ProviderSearch";

export function ProvidersPage() {
    const {
        providers,
        totalCount,
        searchTerm,
        loading,
        error,
        handleSearchTermChange,
        handleSearchClear
    } = useProvidersPage();

    if (loading) {
        return (
            <Container fluid className="mt-4">
                <LoadingIndicator message="Loading providers..." />
            </Container>
        );
    }

    if (error) {
        return (
            <Container fluid className="mt-4">
                <ErrorSummary title="Providers could not be loaded" message={error.message} />
            </Container>
        );
    }

    return (
        <Container fluid className="mt-4">
            <Row className="mb-3 p-2">
                <Col>
                    <h1 className="h3 mb-1">Providers</h1>
                    <p className="text-muted mb-0">
                        The registry of upstream FHIR data sources the patient fan-out calls.
                        Select a provider to see its details.
                    </p>
                </Col>
            </Row>

            <Row className="mb-3 p-2">
                <Col>
                    <ProviderSearch
                        searchTerm={searchTerm}
                        resultCount={providers.length}
                        totalCount={totalCount}
                        onSearchTermChange={handleSearchTermChange}
                        onSearchClear={handleSearchClear} />
                </Col>
            </Row>

            <Row className="p-2">
                <Col>
                    <ProviderList providers={providers} />
                </Col>
            </Row>
        </Container>
    );
}
