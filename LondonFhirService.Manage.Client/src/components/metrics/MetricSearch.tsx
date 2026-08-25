import { Button, Col, Row } from "react-bootstrap";
import SearchBase from "../bases/search/SearchBase";
import TextInputBase from "../bases/inputs/TextInputBase";
import type { MetricSearchProps } from "../../models/components/metrics/MetricSearchProps";

export function MetricSearch({
    filter,
    correlationIdIsIncomplete,
    searching,
    loadedCount,
    onFilterChange,
    onFilterClear
}: MetricSearchProps) {
    const hasFilter = filter.correlationId.length > 0
        || filter.fromDate.length > 0
        || filter.toDate.length > 0;

    return (
        <Row className="align-items-end g-2">
            <Col xs={12} md={5} lg={4}>
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

            <Col xs={12} md="auto" className="ms-md-auto">
                <p className="text-muted mb-2" aria-live="polite">
                    {searching ? "Searching..." : `${loadedCount} requests loaded`}
                </p>
            </Col>
        </Row>
    );
}
