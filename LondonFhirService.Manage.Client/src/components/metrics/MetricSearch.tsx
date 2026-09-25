import { Button, Col, Row } from "react-bootstrap";
import SearchBase from "../bases/search/SearchBase";
import TextInputBase from "../bases/inputs/TextInputBase";
import type { MetricSearchProps } from "../../models/components/metrics/MetricSearchProps";

export function MetricSearch({
    filter,
    correlationIdIsIncomplete,
    searching,
    loadedCount,
    exporting,
    onFilterChange,
    onFilterClear,
    onExport
}: MetricSearchProps) {
    const hasFilter = filter.correlationId.length > 0
        || filter.userId.length > 0
        || filter.fromDate.length > 0
        || filter.toDate.length > 0;

    return (
        <Row className="align-items-end g-2">
            <Col xs={12} md={6} lg={3}>
                <label htmlFor="metricCorrelationId" className="form-label">
                    Correlation id
                </label>

                <SearchBase
                    id="metricCorrelationId"
                    value={filter.correlationId}
                    placeholder="Paste a correlation id"
                    onChange={event =>
                        onFilterChange("correlationId", event.currentTarget.value)} />

                {correlationIdIsIncomplete && (
                    <small className="text-muted">
                        Enter the whole correlation id to search for it.
                    </small>
                )}
            </Col>

            <Col xs={12} md={6} lg={3}>
                <label htmlFor="metricUserId" className="form-label">
                    User id
                </label>

                <SearchBase
                    id="metricUserId"
                    value={filter.userId}
                    placeholder="Paste a user id"
                    onChange={event => onFilterChange("userId", event.currentTarget.value)} />
            </Col>

            <Col xs={6} md={3} lg={2}>
                <TextInputBase
                    id="metricFromDate"
                    name="fromDate"
                    label="From"
                    type="date"
                    value={filter.fromDate}
                    onChange={event => onFilterChange("fromDate", event.target.value)} />
            </Col>

            <Col xs={6} md={3} lg={2}>
                <TextInputBase
                    id="metricToDate"
                    name="toDate"
                    label="To"
                    type="date"
                    value={filter.toDate}
                    onChange={event => onFilterChange("toDate", event.target.value)} />
            </Col>

            <Col xs="auto">
                <Button variant="outline-secondary" onClick={onFilterClear} disabled={!hasFilter}>
                    Clear
                </Button>
            </Col>

            <Col xs={12} md="auto" className="ms-md-auto d-flex align-items-center gap-3">
                {/* Every matching request, not just the rows loaded so far. Held back while a
                    correlation id is half typed, since the file would not match what was asked. */}
                <Button
                    variant="outline-primary"
                    onClick={onExport}
                    disabled={exporting || searching || correlationIdIsIncomplete}>
                    {exporting ? "Exporting..." : "Export to CSV"}
                </Button>

                <p className="text-muted mb-0" aria-live="polite">
                    {searching ? "Searching..." : `${loadedCount} requests loaded`}
                </p>
            </Col>
        </Row>
    );
}
