import { Col, Container, Row } from "react-bootstrap";
import InfiniteScroll from "../../components/bases/pagers/InfiniteScroll";
import { ErrorSummary } from "../../components/shared/ErrorSummary";
import { LoadingIndicator } from "../../components/shared/LoadingIndicator";
import { MetricAverages } from "../../components/metrics/MetricAverages";
import { MetricList } from "../../components/metrics/MetricList";
import { MetricSearch } from "../../components/metrics/MetricSearch";
import { useMetricsPage } from "../../hooks/pages/useMetricsPage";

export function MetricsPage() {
    const {
        metrics,
        averages,
        filter,
        correlationIdIsIncomplete,
        searching,
        handleFilterChange,
        handleFilterClear,
        loading,
        loadingMore,
        hasNextPage,
        error,
        handleLoadMore
    } = useMetricsPage();

    if (loading) {
        return (
            <Container fluid className="mt-4">
                <LoadingIndicator message="Loading request metrics..." />
            </Container>
        );
    }

    if (error) {
        return (
            <Container fluid className="mt-4">
                <ErrorSummary title="Metrics could not be loaded" message={error.message} />
            </Container>
        );
    }

    return (
        <Container fluid className="mt-4">
            <Row className="mb-3 p-2">
                <Col>
                    <h1 className="h3 mb-1">Metrics</h1>
                    <p className="text-muted mb-0">
                        Measured requests, newest first. Each row is the root span of one request;
                        select View to see every span recorded under its correlation id. Records
                        are read only.
                    </p>
                </Col>
            </Row>

            {averages && (
                <Row className="mb-3 p-2">
                    <Col>
                        <MetricAverages averages={averages} />
                    </Col>
                </Row>
            )}

            <Row className="mb-3 p-2">
                <Col>
                    <MetricSearch
                        filter={filter}
                        correlationIdIsIncomplete={correlationIdIsIncomplete}
                        searching={searching}
                        loadedCount={metrics.length}
                        onFilterChange={handleFilterChange}
                        onFilterClear={handleFilterClear} />
                </Col>
            </Row>

            <Row className="p-2">
                <Col>
                    <InfiniteScroll
                        loading={loadingMore}
                        hasNextPage={hasNextPage}
                        loadMore={handleLoadMore}>
                        <MetricList metrics={metrics} />
                    </InfiniteScroll>

                    {loadingMore && <LoadingIndicator message="Loading more requests..." />}
                </Col>
            </Row>
        </Container>
    );
}
