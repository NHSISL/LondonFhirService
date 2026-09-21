import { Badge, Col, Form, Row } from "react-bootstrap";
import { DiffHighlight } from "./DiffHighlight";
import { ExpandableRow } from "./ExpandableRow";
import { expansionKeys } from "./expansionKeys";
import { ResourceJsonToggle } from "./ResourceJsonToggle";
import { ResourceReference } from "./resourceSections";
import { formatFhirDate } from "./patientFormatters";
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

            {episodesOfCare.map((episodeOfCare, index) => (
                <EpisodeOfCareItem
                    key={episodeOfCare.id}
                    episodeOfCare={episodeOfCare}
                    expansionKey={expansionKeys.episodeOfCare(index)}
                    diffs={diffs}
                    context={context} />
            ))}
        </Form.Group>
    );
}

type EpisodeOfCareItemProps = {
    episodeOfCare: EpisodeOfCareData;
    expansionKey: string;
    diffs: DiffItemView[];
    context: ResourceTreeContext;
};

function EpisodeOfCareItem({
    episodeOfCare,
    expansionKey,
    diffs,
    context
}: EpisodeOfCareItemProps) {
    const expanded = context.expansion.isExpanded(expansionKey);
    const resource = context.resourceIndex.get(`EpisodeOfCare/${episodeOfCare.id}`) ?? null;

    const findDiffs = (path: string) =>
        diffs.filter(diff => diff.path === path && diff.identifierText === episodeOfCare.id);

    const statusDiffs = findDiffs("status");
    const typeDiffs = findDiffs("type");
    const careManagerDiffs = findDiffs("careManager");
    const organizationDiffs = findDiffs("managingOrganization");

    const episodeDiffs =
        [...statusDiffs, ...typeDiffs, ...careManagerDiffs, ...organizationDiffs];

    const outstandingDiffCount = episodeDiffs.filter(diff => diff.acceptableDiff === false).length;

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
                onToggle={() => context.expansion.toggleExpanded(expansionKey)}
                label={<span>{title}</span>}
                badges={
                    <>
                        {/*
                          * Status and type are not repeated as chips here. They are laid out in
                          * the panel below, where they carry a DiffHighlight - so a chip beside
                          * the row showed the same value with none of the outlining, and a status
                          * the two providers disagreed about read as settled until the row was
                          * opened. The only badge worth carrying on a collapsed row is the one
                          * saying whether there is anything inside it to look at.
                          */}
                        {episodeDiffs.length > 0 && (
                            <Badge
                                bg={outstandingDiffCount > 0 ? "warning" : "success"}
                                style={badgeFontSize}>
                                {outstandingDiffCount > 0
                                    ? `${outstandingDiffCount} outstanding`
                                    : `${episodeDiffs.length} accepted`}
                            </Badge>
                        )}
                    </>
                } />

            {expanded && (
                <div className="ms-3 mt-2 p-2 border rounded bg-light">
                    {/*
                      * The resource id is deliberately not shown. Providers mint it per request,
                      * so the two sides always carry different ones - the comparison masks it for
                      * exactly that reason and never reports it as a difference. Printing it on
                      * two panels set side by side put the one value that always disagrees in
                      * front of a reviewer whose whole job is spotting disagreement, and it read
                      * as a finding on records the engine had passed as identical.
                      *
                      * It stays reachable through the JSON toggle below for anyone tracing a
                      * resource back to the payload.
                      */}
                    <Row className="g-1">
                        <Col xs={6}>
                            <small className="text-muted d-block">Status</small>

                            <DiffHighlight
                                fieldDiffs={statusDiffs}
                                acceptance={context.acceptance}
                                inline>
                                {episodeOfCare.status ?? "N/A"}
                            </DiffHighlight>
                        </Col>

                        <Col xs={6}>
                            <small className="text-muted d-block">Type</small>

                            <DiffHighlight
                                fieldDiffs={typeDiffs}
                                acceptance={context.acceptance}
                                inline>
                                {episodeOfCare.typeDisplay ?? episodeOfCare.typeCode ?? "N/A"}
                            </DiffHighlight>
                        </Col>

                        {episodeOfCare.careManagerRef !== null && (
                            <Col xs={12}>
                                <small className="text-muted d-block">Care manager</small>

                                <DiffHighlight
                                    fieldDiffs={careManagerDiffs}
                                    acceptance={context.acceptance}
                                    inline>
                                    <ResourceReference
                                        reference={episodeOfCare.careManagerRef}
                                        expansionKey={
                                            expansionKeys.nested(expansionKey, "careManager")}
                                        visitedRefs={
                                            new Set([`EpisodeOfCare/${episodeOfCare.id}`])}
                                        context={context} />
                                </DiffHighlight>
                            </Col>
                        )}

                        {episodeOfCare.organizationRef !== null && (
                            <Col xs={12}>
                                <small className="text-muted d-block">Managing organisation</small>

                                <DiffHighlight
                                    fieldDiffs={organizationDiffs}
                                    acceptance={context.acceptance}
                                    inline>
                                    <ResourceReference
                                        reference={episodeOfCare.organizationRef}
                                        expansionKey={
                                            expansionKeys.nested(
                                                expansionKey,
                                                "managingOrganization")}
                                        visitedRefs={
                                            new Set([`EpisodeOfCare/${episodeOfCare.id}`])}
                                        context={context} />
                                </DiffHighlight>
                            </Col>
                        )}
                    </Row>

                    <ResourceJsonToggle resource={resource} />
                </div>
            )}
        </div>
    );
}
