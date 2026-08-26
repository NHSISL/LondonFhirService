import { Button, Col, Form, Row } from "react-bootstrap";
import SearchBase from "../bases/search/SearchBase";
import type { ComparisonSearchProps } from "../../models/components/comparisons/ComparisonSearchProps";

export function ComparisonSearch({
    searchTerm,
    unresolvedOnly,
    loadedCount,
    searching,
    onSearchTermChange,
    onSearchClear,
    onUnresolvedOnlyChange
}: ComparisonSearchProps) {
    return (
        <Row className="align-items-end g-2">
            <Col xs={12} md={6} lg={5}>
                <label htmlFor="comparisonSearch" className="form-label">
                    Search comparisons
                </label>

                <SearchBase
                    id="comparisonSearch"
                    value={searchTerm}
                    placeholder="Correlation id or comment"
                    onChange={event => onSearchTermChange(event.currentTarget.value)} />
            </Col>

            <Col xs="auto">
                <Button
                    variant="outline-secondary"
                    onClick={onSearchClear}
                    disabled={searchTerm.length === 0}>
                    Clear
                </Button>
            </Col>

            <Col xs="auto">
                <Form.Check
                    type="switch"
                    id="comparisonUnresolvedOnly"
                    label="Unresolved only"
                    checked={unresolvedOnly}
                    onChange={event => onUnresolvedOnlyChange(event.currentTarget.checked)} />
            </Col>

            <Col xs={12} md="auto" className="ms-md-auto">
                <p className="text-muted mb-2" aria-live="polite">
                    {searching ? "Searching..." : `${loadedCount} comparisons loaded`}
                </p>
            </Col>
        </Row>
    );
}
