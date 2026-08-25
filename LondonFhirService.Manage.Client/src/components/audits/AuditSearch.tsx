import { Button, Col, Row } from "react-bootstrap";
import SearchBase from "../bases/search/SearchBase";
import type { AuditSearchProps } from "../../models/components/audits/AuditSearchProps";

export function AuditSearch({
    searchTerm,
    loadedCount,
    searching,
    onSearchTermChange,
    onSearchClear
}: AuditSearchProps) {
    return (
        <Row className="align-items-end g-2">
            <Col xs={12} md={6} lg={5}>
                <label htmlFor="auditSearch" className="form-label">
                    Search audits
                </label>

                <SearchBase
                    id="auditSearch"
                    value={searchTerm}
                    placeholder="Title, type, message, correlation id or user"
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

            <Col xs={12} md="auto" className="ms-md-auto">
                <p className="text-muted mb-2" aria-live="polite">
                    {searching ? "Searching..." : `${loadedCount} audits loaded`}
                </p>
            </Col>
        </Row>
    );
}
