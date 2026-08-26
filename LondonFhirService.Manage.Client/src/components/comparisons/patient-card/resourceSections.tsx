import { useState } from "react";
import { Badge, Col, Row } from "react-bootstrap";
import { CodeWithInfo } from "./CodeWithInfo";
import { ExpandableRow } from "./ExpandableRow";
import { ResourceJsonToggle } from "./ResourceJsonToggle";
import { buildMedicationStatementMatchKey } from "../../../helpers/comparisons/medicationStatementMatchKey";
import { formatFhirDate } from "./patientFormatters";
import { getInlineHighlightStyle } from "../../../helpers/comparisons/diffHighlighting";
import { readString } from "../../../helpers/fhir/fhirJson";
import { useItemExpansion } from "./useItemExpansion";
import {
    parseAllergyIntolerance,
    parseCondition,
    parseMedication,
    parseMedicationStatement,
    parseObservation,
    parseOrganization,
    parsePractitioner,
    parsePractitionerRole
} from "../../../helpers/fhir/fhirResourceParsers";
import type { DiffItemView } from "../../../models/views/comparisons/DiffItemView";
import type { FhirResourceIndex } from "../../../models/foundations/fhir/FhirResource";
import type { ListData } from "../../../models/foundations/fhir/ListData";

// The tree is mutually recursive - a medication points at a practitioner role, which points at a
// practitioner and an organisation - so it lives in one module. Splitting it across files would
// mean import cycles between components that render each other.

const badgeFontSize = { fontSize: "0.65rem" };

// The bundle and the shared expansion state travel to every node in the tree. Passing them as one
// value keeps each component's own props to what it actually decides.
export type ResourceTreeContext = {
    resourceIndex: FhirResourceIndex;
    expandedItems: Set<string>;
    setExpandedItems: (expandedItems: Set<string>) => void;
};

type ListsSectionProps = {
    lists: ListData[];
    listDiffs: DiffItemView[];
    medicationDiffs: DiffItemView[];
    expandedLists: Set<string>;
    setExpandedLists: (expandedLists: Set<string>) => void;
    context: ResourceTreeContext;
};

export function ListsSection({
    lists,
    listDiffs,
    medicationDiffs,
    expandedLists,
    setExpandedLists,
    context
}: ListsSectionProps) {
    return (
        <div className="mb-2">
            {lists.map(list => (
                <ListSection
                    key={list.id}
                    list={list}
                    listDiffs={listDiffs}
                    medicationDiffs={medicationDiffs}
                    expandedLists={expandedLists}
                    setExpandedLists={setExpandedLists}
                    context={context} />
            ))}
        </div>
    );
}

type ListSectionProps = {
    list: ListData;
    listDiffs: DiffItemView[];
    medicationDiffs: DiffItemView[];
    expandedLists: Set<string>;
    setExpandedLists: (expandedLists: Set<string>) => void;
    context: ResourceTreeContext;
};

// Lists are keyed by title rather than id, because the two providers mint different ids for the
// same clinical list and expansion has to stay in step across the two cards.
function ListSection({
    list,
    listDiffs,
    medicationDiffs,
    expandedLists,
    setExpandedLists,
    context
}: ListSectionProps) {
    const { isExpanded, toggleExpanded } =
        useItemExpansion(list.title, expandedLists, setExpandedLists);

    const visitedRefs = new Set<string>([`List/${list.id}`]);

    const sizeDiff = listDiffs.find(diff =>
        diff.identifierText === list.title && diff.type === "entry-count-mismatch");

    const otherDiff = listDiffs.find(diff =>
        diff.identifierText === list.title && diff.type !== "entry-count-mismatch");

    const countBadgeVariant = sizeDiff ? "warning" : otherDiff ? "info" : "secondary";

    const countBadgeTitle = sizeDiff
        ? `List size differs: the primary source has ${sizeDiff.oldValueText} items, `
        + `the secondary has ${sizeDiff.newValueText}`
        : undefined;

    return (
        <div className="mb-2">
            <ExpandableRow
                expanded={isExpanded}
                onToggle={toggleExpanded}
                label={<span>{list.title}</span>}
                badges={
                    <>
                        {list.status !== null && (
                            <Badge bg="info" className="ms-1" style={badgeFontSize}>
                                {list.status}
                            </Badge>
                        )}

                        <Badge
                            bg={countBadgeVariant}
                            className={sizeDiff ? "border border-2 border-danger" : undefined}
                            style={badgeFontSize}
                            title={countBadgeTitle}>
                            {list.itemCount}
                        </Badge>
                    </>
                } />

            {isExpanded && (
                <div className="ms-3 mt-2">
                    {list.itemRefs.length === 0
                        ? <div className="text-muted small">No items in list</div>
                        : (
                            <div className="list-group list-group-flush">
                                {list.itemRefs.map((itemRef, index) => (
                                    <ResourceReference
                                        key={`${itemRef}-${index}`}
                                        reference={itemRef}
                                        visitedRefs={visitedRefs}
                                        medicationDiffs={medicationDiffs}
                                        context={context} />
                                ))}
                            </div>
                        )}
                </div>
            )}
        </div>
    );
}

type ResourceReferenceProps = {
    reference: string;
    visitedRefs: Set<string>;
    context: ResourceTreeContext;
    medicationDiffs?: DiffItemView[];
};

// One place that turns a reference into the right section. Bundles are graphs rather than trees -
// a practitioner role points at a practitioner who asserts an allergy - so a reference already on
// the path is shown as circular instead of followed, and rendering never recurses forever.
export function ResourceReference({
    reference,
    visitedRefs,
    context,
    medicationDiffs
}: ResourceReferenceProps) {
    if (visitedRefs.has(reference)) {
        return (
            <div className="py-1 ps-2">
                <span className="small text-muted">{reference}</span>

                <Badge bg="warning" className="ms-1" style={badgeFontSize}>
                    circular
                </Badge>
            </div>
        );
    }

    const resource = context.resourceIndex.get(reference);

    if (resource === undefined) {
        return (
            <div className="d-flex align-items-center gap-2 py-1 ps-2 border-bottom">
                <span className="small text-muted">{reference}</span>

                <Badge bg="secondary" style={badgeFontSize}>
                    not in bundle
                </Badge>
            </div>
        );
    }

    const nextVisitedRefs = new Set(visitedRefs);
    nextVisitedRefs.add(reference);

    // The indexed resource is the truth about what this is. The reference prefix is only a
    // fallback, for a bundle whose resource omits its own resourceType.
    const resourceType = readString(resource.resourceType) ?? reference.split("/")[0];

    switch (resourceType) {
        case "Condition":
            return <ConditionItem reference={reference} context={context} />;

        case "AllergyIntolerance":
            return (
                <AllergyItem
                    reference={reference}
                    visitedRefs={nextVisitedRefs}
                    context={context} />
            );

        case "MedicationStatement":
            return (
                <MedicationStatementItem
                    reference={reference}
                    visitedRefs={nextVisitedRefs}
                    medicationDiffs={medicationDiffs ?? []}
                    context={context} />
            );

        case "Observation":
            return (
                <ObservationItem
                    reference={reference}
                    visitedRefs={nextVisitedRefs}
                    context={context} />
            );

        case "PractitionerRole":
            return (
                <PractitionerRoleItem
                    reference={reference}
                    visitedRefs={nextVisitedRefs}
                    context={context} />
            );

        case "Practitioner":
            return <PractitionerSection reference={reference} context={context} />;

        case "Organization":
            return <OrganizationSection reference={reference} context={context} />;

        case "Medication":
            return <MedicationItem reference={reference} context={context} />;

        default:
            return <GenericItem reference={reference} context={context} />;
    }
}

type ReferenceOnlyProps = {
    reference: string;
    context: ResourceTreeContext;
};

export function OrganizationSection({ reference, context }: ReferenceOnlyProps) {
    const [expanded, setExpanded] = useState<boolean>(false);
    const resource = context.resourceIndex.get(reference) ?? null;
    const organization = resource === null ? null : parseOrganization(resource);

    return (
        <div className="mb-2">
            <ExpandableRow
                expanded={expanded}
                onToggle={() => setExpanded(currentValue => currentValue === false)}
                label={<span>{organization?.name ?? reference}</span>}
                badges={resource === null && (
                    <Badge bg="secondary" className="ms-1" style={badgeFontSize}>
                        not in bundle
                    </Badge>
                )} />

            {expanded && organization !== null && (
                <div className="ms-3 mt-2 p-2 border rounded bg-light">
                    <Row className="g-1">
                        <Col xs={12}>
                            <small className="text-muted d-block">Name</small>
                            <div>{organization.name ?? "N/A"}</div>
                        </Col>

                        {organization.odsCode !== null && (
                            <Col xs={6}>
                                <small className="text-muted d-block">ODS code</small>

                                <div>
                                    <CodeWithInfo
                                        display={null}
                                        code={organization.odsCode}
                                        system={organization.odsSystem} />
                                </div>
                            </Col>
                        )}

                        {organization.addressLine !== null && (
                            <Col xs={12}>
                                <small className="text-muted d-block">Address line</small>
                                <div>{organization.addressLine}</div>
                            </Col>
                        )}

                        {organization.addressCity !== null && (
                            <Col xs={6}>
                                <small className="text-muted d-block">City</small>
                                <div>{organization.addressCity}</div>
                            </Col>
                        )}

                        {organization.addressPostalCode !== null && (
                            <Col xs={6}>
                                <small className="text-muted d-block">Postcode</small>
                                <div>{organization.addressPostalCode}</div>
                            </Col>
                        )}
                    </Row>

                    <ResourceJsonToggle resource={resource} />
                </div>
            )}
        </div>
    );
}

export function PractitionerSection({ reference, context }: ReferenceOnlyProps) {
    const [expanded, setExpanded] = useState<boolean>(false);
    const resource = context.resourceIndex.get(reference) ?? null;
    const practitioner = resource === null ? null : parsePractitioner(resource);

    return (
        <div className="mb-2">
            <ExpandableRow
                expanded={expanded}
                onToggle={() => setExpanded(currentValue => currentValue === false)}
                label={<span>{practitioner?.displayName ?? reference}</span>}
                badges={resource === null && (
                    <Badge bg="secondary" className="ms-1" style={badgeFontSize}>
                        not in bundle
                    </Badge>
                )} />

            {expanded && practitioner !== null && (
                <div className="ms-3 mt-2 p-2 border rounded bg-light">
                    <Row className="g-1">
                        <Col xs={12}>
                            <small className="text-muted d-block">Name</small>
                            <div>{practitioner.displayName ?? "N/A"}</div>
                        </Col>

                        {practitioner.sdsUserId !== null && (
                            <Col xs={6}>
                                <small className="text-muted d-block">SDS user id</small>

                                <div>
                                    <CodeWithInfo
                                        display={null}
                                        code={practitioner.sdsUserId}
                                        system={practitioner.sdsSystem} />
                                </div>
                            </Col>
                        )}

                        {practitioner.ddsId !== null && (
                            <Col xs={6}>
                                <small className="text-muted d-block">DDS id</small>

                                <div>
                                    <CodeWithInfo
                                        display={null}
                                        code={practitioner.ddsId}
                                        system={practitioner.ddsSystem} />
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

type NestingProps = {
    reference: string;
    visitedRefs: Set<string>;
    context: ResourceTreeContext;
};

function PractitionerRoleItem({ reference, visitedRefs, context }: NestingProps) {
    const { isExpanded, toggleExpanded } =
        useItemExpansion(reference, context.expandedItems, context.setExpandedItems);

    const resource = context.resourceIndex.get(reference) ?? null;
    const practitionerRole = resource === null ? null : parsePractitionerRole(resource);

    return (
        <div className="py-1 ps-2 border-bottom">
            <ExpandableRow
                expanded={isExpanded}
                onToggle={toggleExpanded}
                label={
                    <span className="small">
                        {practitionerRole?.roleCode
                            ? "Practitioner role:"
                            : `PractitionerRole: ${practitionerRole?.roleDisplay ?? reference}`}
                    </span>
                }
                trailing={practitionerRole?.roleCode && (
                    <span className="small">
                        <CodeWithInfo
                            display={practitionerRole.roleDisplay}
                            code={practitionerRole.roleCode}
                            system={practitionerRole.roleSystem} />
                    </span>
                )}
                badges={resource === null && (
                    <Badge bg="secondary" style={badgeFontSize}>
                        not in bundle
                    </Badge>
                )} />

            {isExpanded && practitionerRole !== null && (
                <div className="ms-3 mt-2 p-2 border rounded bg-light">
                    {practitionerRole.practitionerRef !== null && (
                        <div className="mb-2">
                            <small className="text-muted d-block">Practitioner</small>

                            <ResourceReference
                                reference={practitionerRole.practitionerRef}
                                visitedRefs={visitedRefs}
                                context={context} />
                        </div>
                    )}

                    {practitionerRole.organizationRef !== null && (
                        <div className="mb-2">
                            <small className="text-muted d-block">Organisation</small>

                            <ResourceReference
                                reference={practitionerRole.organizationRef}
                                visitedRefs={visitedRefs}
                                context={context} />
                        </div>
                    )}

                    <ResourceJsonToggle resource={resource} />
                </div>
            )}
        </div>
    );
}

function ConditionItem({ reference, context }: ReferenceOnlyProps) {
    const { isExpanded, toggleExpanded } =
        useItemExpansion(reference, context.expandedItems, context.setExpandedItems);

    const resource = context.resourceIndex.get(reference) ?? null;
    const condition = resource === null ? null : parseCondition(resource);

    return (
        <div className="py-1 ps-2 border-bottom">
            <ExpandableRow
                expanded={isExpanded}
                onToggle={toggleExpanded}
                label={
                    <span className="small">
                        Condition:{condition?.code ? "" : ` ${condition?.display ?? reference}`}
                    </span>
                }
                trailing={condition?.code && (
                    <span className="small">
                        <CodeWithInfo
                            display={condition.display}
                            code={condition.code}
                            system={condition.system} />
                    </span>
                )}
                badges={
                    <>
                        {condition?.clinicalStatus && (
                            <Badge
                                bg={condition.clinicalStatus === "active" ? "success" : "secondary"}
                                style={badgeFontSize}>
                                {condition.clinicalStatus}
                            </Badge>
                        )}

                        {condition?.significance && (
                            <Badge bg="info" style={badgeFontSize}>
                                {condition.significance}
                            </Badge>
                        )}
                    </>
                } />

            {isExpanded && condition !== null && (
                <div className="ms-3 mt-2 p-2 border rounded bg-light">
                    <Row className="g-1">
                        <Col xs={12}>
                            <small className="text-muted d-block">Display</small>
                            <div>{condition.display ?? "N/A"}</div>
                        </Col>

                        {condition.code !== null && (
                            <Col xs={12}>
                                <small className="text-muted d-block">Code</small>

                                <div>
                                    <CodeWithInfo
                                        display={condition.display}
                                        code={condition.code}
                                        system={condition.system} />
                                </div>
                            </Col>
                        )}

                        {condition.clinicalStatus !== null && (
                            <Col xs={6}>
                                <small className="text-muted d-block">Status</small>
                                <div>{condition.clinicalStatus}</div>
                            </Col>
                        )}

                        {condition.onsetDateTime !== null && (
                            <Col xs={6}>
                                <small className="text-muted d-block">Onset date</small>
                                <div>{formatFhirDate(condition.onsetDateTime)}</div>
                            </Col>
                        )}

                        {condition.significance !== null && (
                            <Col xs={6}>
                                <small className="text-muted d-block">Significance</small>
                                <div>{condition.significance}</div>
                            </Col>
                        )}
                    </Row>

                    <ResourceJsonToggle resource={resource} />
                </div>
            )}
        </div>
    );
}

function AllergyItem({ reference, visitedRefs, context }: NestingProps) {
    const { isExpanded, toggleExpanded } =
        useItemExpansion(reference, context.expandedItems, context.setExpandedItems);

    const resource = context.resourceIndex.get(reference) ?? null;
    const allergy = resource === null ? null : parseAllergyIntolerance(resource);

    return (
        <div className="py-1 ps-2 border-bottom">
            <ExpandableRow
                expanded={isExpanded}
                onToggle={toggleExpanded}
                label={
                    <span className="small">
                        Allergy:{allergy?.code ? "" : ` ${allergy?.display ?? reference}`}
                    </span>
                }
                trailing={allergy?.code && (
                    <span className="small">
                        <CodeWithInfo
                            display={allergy.display}
                            code={allergy.code}
                            system={allergy.system} />
                    </span>
                )}
                badges={allergy?.verificationStatus && (
                    <Badge
                        bg={allergy.verificationStatus === "confirmed" ? "success" : "warning"}
                        style={badgeFontSize}>
                        {allergy.verificationStatus}
                    </Badge>
                )} />

            {isExpanded && allergy !== null && (
                <div className="ms-3 mt-2 p-2 border rounded bg-light">
                    <Row className="g-1">
                        <Col xs={12}>
                            <small className="text-muted d-block">Display</small>
                            <div>{allergy.display ?? "N/A"}</div>
                        </Col>

                        {allergy.code !== null && (
                            <Col xs={12}>
                                <small className="text-muted d-block">Code</small>

                                <div>
                                    <CodeWithInfo
                                        display={allergy.display}
                                        code={allergy.code}
                                        system={allergy.system} />
                                </div>
                            </Col>
                        )}

                        {allergy.type !== null && (
                            <Col xs={6}>
                                <small className="text-muted d-block">Type</small>
                                <div>{allergy.type}</div>
                            </Col>
                        )}

                        {allergy.clinicalStatus !== null && (
                            <Col xs={6}>
                                <small className="text-muted d-block">Status</small>
                                <div>{allergy.clinicalStatus}</div>
                            </Col>
                        )}

                        {allergy.verificationStatus !== null && (
                            <Col xs={6}>
                                <small className="text-muted d-block">Verification</small>
                                <div>{allergy.verificationStatus}</div>
                            </Col>
                        )}

                        {allergy.onsetDateTime !== null && (
                            <Col xs={6}>
                                <small className="text-muted d-block">Onset date</small>
                                <div>{formatFhirDate(allergy.onsetDateTime)}</div>
                            </Col>
                        )}

                        {allergy.asserterRef !== null && (
                            <Col xs={12}>
                                <small className="text-muted d-block">Asserter</small>

                                <ResourceReference
                                    reference={allergy.asserterRef}
                                    visitedRefs={visitedRefs}
                                    context={context} />
                            </Col>
                        )}
                    </Row>

                    <ResourceJsonToggle resource={resource} />
                </div>
            )}
        </div>
    );
}

type MedicationStatementItemProps = {
    reference: string;
    visitedRefs: Set<string>;
    medicationDiffs: DiffItemView[];
    context: ResourceTreeContext;
};

function MedicationStatementItem({
    reference,
    visitedRefs,
    medicationDiffs,
    context
}: MedicationStatementItemProps) {
    const { isExpanded, toggleExpanded } =
        useItemExpansion(reference, context.expandedItems, context.setExpandedItems);

    const resource = context.resourceIndex.get(reference) ?? null;
    const medicationStatement = resource === null ? null : parseMedicationStatement(resource);

    // A statement can name its medication inline or point at a Medication resource. The
    // referenced one wins, because that is where the provider put the coded value.
    const referencedMedicationResource = medicationStatement?.medicationRef
        ? context.resourceIndex.get(medicationStatement.medicationRef) ?? null
        : null;

    const referencedMedication = referencedMedicationResource === null
        ? null
        : parseMedication(referencedMedicationResource);

    const medicationName =
        referencedMedication?.display ?? medicationStatement?.medicationName ?? null;

    const medicationCode =
        referencedMedication?.code ?? medicationStatement?.medicationCode ?? null;

    const medicationSystem =
        referencedMedication?.system ?? medicationStatement?.medicationSystem ?? null;

    // Only this statement's own differences, found by rebuilding the key the engine matched on.
    const matchKey = resource === null
        ? null
        : buildMedicationStatementMatchKey(resource, context.resourceIndex);

    const itemDiffs = matchKey === null
        ? []
        : medicationDiffs.filter(diff => diff.identifierText === matchKey);

    const hasDosageDiff = itemDiffs.some(diff => diff.path.endsWith(".dosage[0].text"));

    return (
        <div className="py-1 ps-2 border-bottom">
            <ExpandableRow
                expanded={isExpanded}
                onToggle={toggleExpanded}
                label={
                    <span className="small">
                        Medication:
                        {medicationCode !== null ? "" : ` ${medicationName ?? reference}`}
                    </span>
                }
                trailing={medicationCode !== null && (
                    <span className="small">
                        <CodeWithInfo
                            display={medicationName}
                            code={medicationCode}
                            system={medicationSystem} />
                    </span>
                )}
                badges={
                    <>
                        {medicationStatement?.status && (
                            <Badge
                                bg={medicationStatement.status === "active"
                                    ? "success"
                                    : "secondary"}
                                style={badgeFontSize}>
                                {medicationStatement.status}
                            </Badge>
                        )}

                        {itemDiffs.length > 0 && (
                            <Badge bg="warning" style={badgeFontSize}>
                                {itemDiffs.length === 1
                                    ? "1 difference"
                                    : `${itemDiffs.length} differences`}
                            </Badge>
                        )}
                    </>
                } />

            {isExpanded && medicationStatement !== null && (
                <div className="ms-3 mt-2 p-2 border rounded bg-light">
                    <Row className="g-1">
                        <Col xs={12}>
                            <small className="text-muted d-block">Medication</small>
                            <div>{medicationName ?? "N/A"}</div>
                        </Col>

                        {medicationCode !== null && (
                            <Col xs={12}>
                                <small className="text-muted d-block">Code</small>

                                <div>
                                    <CodeWithInfo
                                        display={medicationName}
                                        code={medicationCode}
                                        system={medicationSystem} />
                                </div>
                            </Col>
                        )}

                        {medicationStatement.dosage !== null && (
                            <Col xs={12}>
                                <small className="text-muted d-block">Dosage</small>

                                <div style={getInlineHighlightStyle(hasDosageDiff)}>
                                    {medicationStatement.dosage}
                                </div>
                            </Col>
                        )}

                        {medicationStatement.status !== null && (
                            <Col xs={6}>
                                <small className="text-muted d-block">Status</small>
                                <div>{medicationStatement.status}</div>
                            </Col>
                        )}

                        {medicationStatement.dateAsserted !== null && (
                            <Col xs={6}>
                                <small className="text-muted d-block">Date asserted</small>
                                <div>{formatFhirDate(medicationStatement.dateAsserted)}</div>
                            </Col>
                        )}

                        {medicationStatement.informationSourceRef !== null && (
                            <Col xs={12}>
                                <small className="text-muted d-block">Information source</small>

                                <ResourceReference
                                    reference={medicationStatement.informationSourceRef}
                                    visitedRefs={visitedRefs}
                                    context={context} />
                            </Col>
                        )}

                        {medicationStatement.medicationRef !== null && (
                            <Col xs={12}>
                                <small className="text-muted d-block">Medication resource</small>

                                <ResourceReference
                                    reference={medicationStatement.medicationRef}
                                    visitedRefs={visitedRefs}
                                    context={context} />
                            </Col>
                        )}
                    </Row>

                    <ResourceJsonToggle resource={resource} />
                </div>
            )}
        </div>
    );
}

function ObservationItem({ reference, visitedRefs, context }: NestingProps) {
    const { isExpanded, toggleExpanded } =
        useItemExpansion(reference, context.expandedItems, context.setExpandedItems);

    const resource = context.resourceIndex.get(reference) ?? null;
    const observation = resource === null ? null : parseObservation(resource);

    return (
        <div className="py-1 ps-2 border-bottom">
            <ExpandableRow
                expanded={isExpanded}
                onToggle={toggleExpanded}
                label={
                    <span className="small">
                        Observation:{observation?.code ? "" : ` ${observation?.display ?? reference}`}
                    </span>
                }
                trailing={observation?.code && (
                    <span className="small">
                        <CodeWithInfo
                            display={observation.display}
                            code={observation.code}
                            system={observation.system} />

                        {observation.value !== null && ` - ${observation.value}`}
                    </span>
                )}
                badges={observation?.status && (
                    <Badge
                        bg={observation.status === "final" ? "success" : "warning"}
                        style={badgeFontSize}>
                        {observation.status}
                    </Badge>
                )} />

            {isExpanded && observation !== null && (
                <div className="ms-3 mt-2 p-2 border rounded bg-light">
                    <Row className="g-1">
                        <Col xs={12}>
                            <small className="text-muted d-block">Display</small>
                            <div>{observation.display ?? "N/A"}</div>
                        </Col>

                        {observation.code !== null && (
                            <Col xs={12}>
                                <small className="text-muted d-block">Code</small>

                                <div>
                                    <CodeWithInfo
                                        display={observation.display}
                                        code={observation.code}
                                        system={observation.system} />
                                </div>
                            </Col>
                        )}

                        {observation.category !== null && (
                            <Col xs={6}>
                                <small className="text-muted d-block">Category</small>
                                <div>{observation.category}</div>
                            </Col>
                        )}

                        {observation.status !== null && (
                            <Col xs={6}>
                                <small className="text-muted d-block">Status</small>
                                <div>{observation.status}</div>
                            </Col>
                        )}

                        {observation.value !== null && (
                            <Col xs={12}>
                                <small className="text-muted d-block">Value</small>
                                <div className="fw-bold">{observation.value}</div>
                            </Col>
                        )}

                        {(observation.effectiveDateTime !== null
                            || observation.effectivePeriodStart !== null) && (
                            <Col xs={6}>
                                <small className="text-muted d-block">Effective date</small>

                                <div>
                                    {formatFhirDate(
                                        observation.effectiveDateTime
                                        ?? observation.effectivePeriodStart)}
                                </div>
                            </Col>
                        )}

                        {observation.performerRefs.length > 0 && (
                            <Col xs={12}>
                                <small className="text-muted d-block">Performer(s)</small>

                                {observation.performerRefs.map(performerRef => (
                                    <ResourceReference
                                        key={performerRef}
                                        reference={performerRef}
                                        visitedRefs={visitedRefs}
                                        context={context} />
                                ))}
                            </Col>
                        )}
                    </Row>

                    <ResourceJsonToggle resource={resource} />
                </div>
            )}
        </div>
    );
}

// A leaf: a Medication carries a code and nothing worth expanding into.
function MedicationItem({ reference, context }: ReferenceOnlyProps) {
    const resource = context.resourceIndex.get(reference) ?? null;
    const medication = resource === null ? null : parseMedication(resource);

    if (medication === null) {
        return (
            <div className="py-1 ps-2 border-bottom">
                <span className="small text-muted">{reference}</span>

                <Badge bg="secondary" className="ms-1" style={badgeFontSize}>
                    not in bundle
                </Badge>
            </div>
        );
    }

    return (
        <div className="py-1 ps-2 border-bottom">
            <span className="small">
                Medication:{" "}
                {medication.code !== null
                    ? (
                        <CodeWithInfo
                            display={medication.display}
                            code={medication.code}
                            system={medication.system} />
                    )
                    : medication.display ?? reference}
            </span>
        </div>
    );
}

// Whatever the tree has no dedicated view for. It still opens to its JSON, so an unmodelled
// resource type is inspectable rather than invisible.
function GenericItem({ reference, context }: ReferenceOnlyProps) {
    const { isExpanded, toggleExpanded } =
        useItemExpansion(reference, context.expandedItems, context.setExpandedItems);

    const resource = context.resourceIndex.get(reference) ?? null;
    const resourceType = readString(resource?.resourceType) ?? "Unknown";

    return (
        <div className="py-1 ps-2 border-bottom">
            <ExpandableRow
                expanded={isExpanded}
                onToggle={toggleExpanded}
                label={<span className="small">{resourceType}: {reference}</span>} />

            {isExpanded && resource !== null && (
                <div className="ms-3 mt-2 p-2 border rounded bg-light">
                    <ResourceJsonToggle resource={resource} className="p-0" />
                </div>
            )}
        </div>
    );
}
