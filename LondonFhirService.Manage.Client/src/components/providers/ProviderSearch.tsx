import { Button, Col, Row } from "react-bootstrap";
import SearchBase from "../bases/search/SearchBase";
import type { ProviderSearchProps } from "../../models/components/providers/ProviderSearchProps";

export function ProviderSearch({
    searchTerm,
    resultCount,
    totalCount,
    onSearchTermChange,
    onSearchClear
}: ProviderSearchProps) {
    return (
        <Row className="align-items-end g-2">
            <Col xs={12} md={6} lg={5}>
                <label htmlFor="providerSearch" className="form-label">
                    Search providers
                </label>

                <SearchBase
                    id="providerSearch"
                    value={searchTerm}
                    placeholder="Name, endpoint, FHIR version, status or role"
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
                    Showing {resultCount} of {totalCount} providers
                </p>
            </Col>
        </Row>
    );
}
