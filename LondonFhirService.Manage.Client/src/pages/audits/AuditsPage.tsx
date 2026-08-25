import { Col, Container, Row } from "react-bootstrap";
import InfiniteScroll from "../../components/bases/pagers/InfiniteScroll";
import { AuditList } from "../../components/audits/AuditList";
import { AuditSearch } from "../../components/audits/AuditSearch";
import { ErrorSummary } from "../../components/shared/ErrorSummary";
import { LoadingIndicator } from "../../components/shared/LoadingIndicator";
import { useAuditsPage } from "../../hooks/pages/useAuditsPage";

export function AuditsPage() {
    const {
        audits,
        searchTerm,
        loading,
        loadingMore,
        searching,
        hasNextPage,
        error,
        handleSearchTermChange,
        handleSearchClear,
        handleLoadMore
    } = useAuditsPage();

    if (loading) {
        return (
            <Container fluid className="mt-4">
                <LoadingIndicator message="Loading audits..." />
            </Container>
        );
    }

    if (error) {
        return (
            <Container fluid className="mt-4">
                <ErrorSummary title="Audits could not be loaded" message={error.message} />
            </Container>
        );
    }

    return (
        <Container fluid className="mt-4">
            <Row className="mb-3 p-2">
                <Col>
                    <h1 className="h3 mb-1">Audits</h1>
                    <p className="text-muted mb-0">
                        The audit trail this service writes as it runs, newest first. Records are
                        read only. Select an audit to see its full message.
                    </p>
                </Col>
            </Row>

            <Row className="mb-3 p-2">
                <Col>
                    <AuditSearch
                        searchTerm={searchTerm}
                        loadedCount={audits.length}
                        searching={searching}
                        onSearchTermChange={handleSearchTermChange}
                        onSearchClear={handleSearchClear} />
                </Col>
            </Row>

            <Row className="p-2">
                <Col>
                    <InfiniteScroll
                        loading={loadingMore}
                        hasNextPage={hasNextPage}
                        loadMore={handleLoadMore}>
                        <AuditList audits={audits} />
                    </InfiniteScroll>

                    {loadingMore && <LoadingIndicator message="Loading more audits..." />}
                </Col>
            </Row>
        </Container>
    );
}
