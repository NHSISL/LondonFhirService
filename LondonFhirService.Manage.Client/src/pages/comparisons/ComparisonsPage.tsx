import { Col, Container, Row } from "react-bootstrap";
import InfiniteScroll from "../../components/bases/pagers/InfiniteScroll";
import { ComparisonList } from "../../components/comparisons/ComparisonList";
import { ComparisonSearch } from "../../components/comparisons/ComparisonSearch";
import { ErrorSummary } from "../../components/shared/ErrorSummary";
import { LoadingIndicator } from "../../components/shared/LoadingIndicator";
import { useComparisonsPage } from "../../hooks/pages/useComparisonsPage";

export function ComparisonsPage() {
    const {
        comparisons,
        searchTerm,
        unresolvedOnly,
        loading,
        loadingMore,
        searching,
        hasNextPage,
        error,
        handleSearchTermChange,
        handleSearchClear,
        handleUnresolvedOnlyChange,
        handleLoadMore
    } = useComparisonsPage();

    if (loading) {
        return (
            <Container fluid className="mt-4">
                <LoadingIndicator message="Loading comparisons..." />
            </Container>
        );
    }

    if (error) {
        return (
            <Container fluid className="mt-4">
                <ErrorSummary title="Comparisons could not be loaded" message={error.message} />
            </Container>
        );
    }

    return (
        <Container fluid className="mt-4">
            <Row className="mb-3 p-2">
                <Col>
                    <h1 className="h3 mb-1">Comparisons</h1>

                    <p className="text-muted mb-0">
                        What the comparison service found when it checked a secondary provider's
                        answer against the primary's, newest first. Select a correlation id to see
                        the two records side by side.
                    </p>
                </Col>
            </Row>

            <Row className="mb-3 p-2">
                <Col>
                    <ComparisonSearch
                        searchTerm={searchTerm}
                        unresolvedOnly={unresolvedOnly}
                        loadedCount={comparisons.length}
                        searching={searching}
                        onSearchTermChange={handleSearchTermChange}
                        onSearchClear={handleSearchClear}
                        onUnresolvedOnlyChange={handleUnresolvedOnlyChange} />
                </Col>
            </Row>

            <Row className="p-2">
                <Col>
                    <InfiniteScroll
                        loading={loadingMore}
                        hasNextPage={hasNextPage}
                        loadMore={handleLoadMore}>
                        <ComparisonList comparisons={comparisons} />
                    </InfiniteScroll>

                    {loadingMore && <LoadingIndicator message="Loading more comparisons..." />}
                </Col>
            </Row>
        </Container>
    );
}
