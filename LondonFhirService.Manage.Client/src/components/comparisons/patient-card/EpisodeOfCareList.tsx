import { useState } from "react";
import { Badge, Col, Form, Row } from "react-bootstrap";
import { ExpandableRow } from "./ExpandableRow";
import { ResourceJsonToggle } from "./ResourceJsonToggle";
import { ResourceReference } from "./resourceSections";
import { formatFhirDate } from "./patientFormatters";
import { getInlineHighlightStyle } from "../../../helpers/comparisons/diffHighlighting";
import { readString } from "../../../helpers/fhir/fhirJson";
import type { DiffItemView } from "../../../models/views/comparisons/DiffItemView";
import type { EpisodeOfCareData } from "../../../models/foundations/fhir/EpisodeOfCareData";
import type { ResourceTreeContext } from "./resourceSections";

const badgeFontSize = { fontSize: "0.65rem" };

type EpisodeOfCareListProps = {
    episodesOfCare: EpisodeOfCareData[];
    diffs: DiffItemView[];
    context: ResourceTreeContext;
};

export function EpisodeOfCareList({ episodesOfCare, diffs, context }: EpisodeOfCareListProps) {
    if (episodesOfCare.length === 0) {
        return null;
    }

    return (
        <Form.Group className="mb-3">
            <Form.Label className="text-muted small mb-1">Episode of care</Form.Label>

            {episodesOfCare.map(episodeOfCare => (
                <EpisodeOfCareItem
                    key={episodeOfCare.id}
                    episodeOfCare={episodeOfCare}
                    diffs={diffs}
                    context={context} />
            ))}
        </Form.Group>
    );
}

type EpisodeOfCareItemProps = {
    episodeOfCare: EpisodeOfCareData;
    diffs: DiffItemView[];
    context: ResourceTreeContext;
};

const statusBadgeVariants: Record<string, string> = {
    "active": "success",
    "finished": "secondary",
    "cancelled": "danger"
};

function EpisodeOfCareItem({ episodeOfCare, diffs, context }: EpisodeOfCareItemProps) {
    const [expanded, setExpanded] = useState<boolean>(false);
    const resource = context.resourceIndex.get(`EpisodeOfCare/${episodeOfCare.id}`) ?? null;

    const findDiff = (path: string) =>
        diffs.some(diff => diff.path === path && diff.identifierText === episodeOfCare.id);

    const hasStatusDiff = findDiff("status");
    const hasTypeDiff = findDiff("type");
    const hasCareManagerDiff = findDiff("careManager");
    const hasOrganizationDiff = findDiff("managingOrganization");

    const hasAnyDiff =
        hasStatusDiff || hasTypeDiff || hasCareManagerDiff || hasOrganizationDiff;

    const organizationResource = episodeOfCare.organizationRef === null
        ? null
        : context.resourceIndex.get(episodeOfCare.organizationRef) ?? null;

    const organizationName = readString(organizationResource?.name)
        ?? episodeOfCare.organizationRef
        ?? "Unknown organisation";

    const title = episodeOfCare.periodStart === null
        ? organizationName
        : `${formatFhirDate(episodeOfCare.periodStart)} - ${organizationName}`;

    return (
        <div className="mb-2">
            <ExpandableRow
                expanded={expanded}
                onToggle={() => setExpanded(currentValue => currentValue === false)}
                label={<span>{title}</span>}
                badges={
                    <>
                        {episodeOfCare.status !== null && (
                            <Badge
                                bg={statusBadgeVariants[episodeOfCare.status] ?? "info"}
                                style={badgeFontSize}>
                                {episodeOfCare.status}
                            </Badge>
                        )}

                        {episodeOfCare.typeDisplay !== null && (
                            <Badge bg="info" style={badgeFontSize}>
                                {episodeOfCare.typeDisplay}
                            </Badge>
                        )}

                        {hasAnyDiff && (
                            <Badge bg="warning" style={badgeFontSize}>
                                modified
                            </Badge>
                        )}
                    </>
                } />

            {expanded && (
                <div className="ms-3 mt-2 p-2 border rounded bg-light">
                    <Row className="g-1">
                        <Col xs={12}>
                            <small className="text-muted d-block">Id</small>
                            <div>{episodeOfCare.id || "N/A"}</div>
                        </Col>

                        <Col xs={6}>
                            <small className="text-muted d-block">Status</small>

                            <div style={getInlineHighlightStyle(hasStatusDiff)}>
                                {episodeOfCare.status ?? "N/A"}
                            </div>
                        </Col>

                        <Col xs={6}>
                            <small className="text-muted d-block">Type</small>

                            <div style={getInlineHighlightStyle(hasTypeDiff)}>
                                {episodeOfCare.typeDisplay ?? episodeOfCare.typeCode ?? "N/A"}
                            </div>
                        </Col>

                        {episodeOfCare.careManagerRef !== null && (
                            <Col xs={12}>
                                <small className="text-muted d-block">Care manager</small>

                                <div style={getInlineHighlightStyle(hasCareManagerDiff)}>
                                    <ResourceReference
                                        reference={episodeOfCare.careManagerRef}
                                        visitedRefs={
                                            new Set([`EpisodeOfCare/${episodeOfCare.id}`])}
                                        context={context} />
                                </div>
                            </Col>
                        )}

                        {episodeOfCare.organizationRef !== null && (
                            <Col xs={12}>
                                <small className="text-muted d-block">Managing organisation</small>

                                <div style={getInlineHighlightStyle(hasOrganizationDiff)}>
                                    <ResourceReference
                                        reference={episodeOfCare.organizationRef}
                                        visitedRefs={
                                            new Set([`EpisodeOfCare/${episodeOfCare.id}`])}
                                        context={context} />
                                </div>
                            </Col>
                        )}
                    </Row>

                    <ResourceJsonToggle resource={resource} />
                </div>
            )}
        </div>
    );
}
