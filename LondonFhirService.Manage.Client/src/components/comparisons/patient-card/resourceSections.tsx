import { Badge, Col, Row } from "react-bootstrap";
import { CodeWithInfo } from "./CodeWithInfo";
import { DiffHighlight } from "./DiffHighlight";
import { ExpandableRow } from "./ExpandableRow";
import { ResourceJsonToggle } from "./ResourceJsonToggle";
import { buildAllergyIntoleranceMatchKey } from "../../../helpers/comparisons/allergyIntoleranceMatchKey";
import { buildConditionMatchKey } from "../../../helpers/comparisons/conditionMatchKey";
import { buildDdsIdentifierMatchKey } from "../../../helpers/comparisons/ddsIdentifierMatchKey";
import { buildMedicationStatementMatchKey } from "../../../helpers/comparisons/medicationStatementMatchKey";
import { formatFhirDate } from "./patientFormatters";
import { findIdentifierBySystem, readString } from "../../../helpers/fhir/fhirJson";
import { getAmbiguousDiffs, getMatchedDiffs } from "../../../helpers/comparisons/matchedDiffs";
import { dedicatedSectionResourceTypes } from "../../../helpers/fhir/unlistedResources";
import { expansionKeys } from "./expansionKeys";
import {
    parseAllergyIntolerance,
    parseAppointment,
    parseCondition,
    parseDiagnosticReport,
    parseEncounter,
    parseFamilyMemberHistory,
    parseImmunization,
    parseLocation,
    parseMedication,
    parseMedicationRequest,
    parseMedicationStatement,
    parseObservation,
    parseOrganization,
    parsePractitioner,
    parsePractitionerRole,
    parseProcedure,
    parseProcedureRequest,
    parseReferralRequest,
    parseRelatedPerson
} from "../../../helpers/fhir/fhirResourceParsers";
import type { DiffAcceptance } from "../../../models/components/comparisons/DiffAcceptance";
import type { CardExpansion } from "../../../models/components/comparisons/CardExpansion";
import type { DiffItemView } from "../../../models/views/comparisons/DiffItemView";
import type { FhirResource, FhirResourceIndex } from "../../../models/foundations/fhir/FhirResource";
import type { ListData } from "../../../models/foundations/fhir/ListData";

// The tree is mutually recursive - a medication points at a practitioner role, which points at a
// practitioner and an organisation - so it lives in one module. Splitting it across files would
// mean import cycles between components that render each other.

const badgeFontSize = { fontSize: "0.65rem" };

// The bundle and the shared expansion state travel to every node in the tree. Passing them as one
// value keeps each component's own props to what it actually decides.
export type ResourceTreeContext = {
    resourceIndex: FhirResourceIndex;
    expansion: CardExpansion;
    acceptance: DiffAcceptance;
    diffsByResourceType: Map<string, DiffItemView[]>;
};

// The one badge every item in the tree shows when the comparison recorded a difference against
// it: how many of those differences are still outstanding, or that all of them have been
// accepted. Ticking a difference happens from the differences list, not from here - this is only
// where its outcome is seen while browsing the record.
export function DiffCountBadge({ diffs }: { diffs: DiffItemView[] }) {
    if (diffs.length === 0) {
        return null;
    }

    const outstandingCount = diffs.filter(diff => diff.acceptableDiff === false).length;

    if (outstandingCount > 0) {
        const label = `${outstandingCount} difference${outstandingCount === 1 ? "" : "s"}`;

        return (
            <Badge
                bg="danger"
                style={badgeFontSize}
                title={`${outstandingCount} unresolved difference${outstandingCount === 1 ? "" : "s"}`}>
                {label}
            </Badge>
        );
    }

    return (
        <Badge bg="success" style={badgeFontSize}>
            {diffs.length} accepted
        </Badge>
    );
}

// The plain count every clinical list and section shows - never colored, since a count of items
// is not itself a difference. Kept as one component so every list and section agrees on wording.
function ItemCountBadge({ count }: { count: number }) {
    const label = `${count} item${count === 1 ? "" : "s"}`;

    return (
        <Badge bg="secondary" style={badgeFontSize} title={`${label} in list`}>
            {label}
        </Badge>
    );
}

// A manual-review-required diff shown on the resource whose own key produced it, rather than only
// in the full differences list - see getAmbiguousDiffs. Kept out of DiffCountBadge's count
// deliberately: it is not a confirmed difference on this resource, only a note that the engine
// could not tell this resource apart from another one sharing its identifier, so folding it into
// the same red count would overstate how much of this resource is actually known to differ.
function AmbiguousMatchNotice({ diffs, acceptance }: { diffs: DiffItemView[]; acceptance: DiffAcceptance }) {
    if (diffs.length === 0) {
        return null;
    }

    return (
        <DiffHighlight fieldDiffs={diffs} acceptance={acceptance}>
            <div className="small text-muted">
                Possible match: this resource&apos;s identifier is shared with another resource
                on this side, so the comparison could not confirm which one this difference
                belongs to.
            </div>
        </DiffHighlight>
    );
}

// Mirrors the switch in ResourceReference, but to find the key a resource's own differences are
// recorded against rather than to render it - null for anything the card does not track per-item
// differences for at all.
function getResourceMatchKey(
    resourceType: string,
    resource: FhirResource,
    resourceIndex: FhirResourceIndex)
    : string | null {
    switch (resourceType) {
        case "MedicationStatement":
            return buildMedicationStatementMatchKey(resource, resourceIndex);

        case "Condition":
            return buildConditionMatchKey(resource);

        case "AllergyIntolerance":
            return buildAllergyIntoleranceMatchKey(resource);

        case "Observation":
        case "Immunization":
        case "Encounter":
        case "FamilyMemberHistory":
        case "MedicationRequest":
        case "DiagnosticReport":
        case "Procedure":
        case "ProcedureRequest":
        case "ReferralRequest":
        case "Appointment":
            return buildDdsIdentifierMatchKey(resource);

        // The SDS role profile id is what PractitionerRoleMatcherService pairs these on server
        // side - the same fragment-matched identifier lookup parsePractitionerRole itself reads.
        case "PractitionerRole":
            return readString(findIdentifierBySystem(resource, "sds-role-profile-id")?.value);

        // The ODS organisation and site codes are what OrganizationMatcherService and
        // LocationMatcherService pair these on server side - the same lookups parseOrganization
        // and parseLocation themselves read.
        case "Organization":
            return readString(findIdentifierBySystem(resource, "ods-organization-code")?.value);

        case "Location":
            return readString(findIdentifierBySystem(resource, "ods-site-code")?.value);

        // The SDS user id is what PractitionerMatcherService pairs these on server side - the
        // same lookup parsePractitioner itself reads.
        case "Practitioner":
            return readString(findIdentifierBySystem(resource, "sds-user-id")?.value);

        // RelatedPerson has no matcher registered server side at all, so there is no key to
        // rebuild - its differences stay in the card's unattributed list rather than being
        // silently dropped by a badge that can never show them.
        default:
            return null;
    }
}

// Every difference recorded against any of these references' own resource, added up so a
// collapsed section can show that it holds outstanding differences before anyone opens it -
// otherwise the only sign a list has something wrong inside it was to expand every item in turn.
//
// Two references can legitimately share a match key - the DDS feed itself carries duplicate
// identifiers for some resources - so the same difference can come back for more than one
// reference here. De-duplicated by index, or a section with such a resource would count that one
// difference once per reference that shares its key.
function getDiffsForReferences(
    references: string[],
    context: ResourceTreeContext)
    : DiffItemView[] {
    const diffs = references.flatMap(reference => {
        const resource = context.resourceIndex.get(reference);

        if (resource === undefined) {
            return [];
        }

        const resourceType = readString(resource.resourceType) ?? reference.split("/")[0];
        const matchKey = getResourceMatchKey(resourceType, resource, context.resourceIndex);

        return getMatchedDiffs(context.diffsByResourceType.get(resourceType) ?? [], matchKey);
    });

    return [...new Map(diffs.map(diff => [diff.index, diff])).values()];
}

type ListsSectionProps = {
    lists: ListData[];
    listDiffs: DiffItemView[];
    context: ResourceTreeContext;
};

export function ListsSection({
    lists,
    listDiffs,
    context
}: ListsSectionProps) {
    // A diff against the List resource itself carries only the list's title, not which of
    // possibly several same-titled lists it came from - the engine has no id to tell them apart
    // either. Giving every list with that title its own copy of the diff would let one real
    // difference be accepted, or counted, more than once, so only the first list claims it.
    const seenTitles = new Set<string>();

    return (
        <div className="mb-2">
            {lists.map(list => {
                const claimsOwnDiffs = seenTitles.has(list.title) === false;
                seenTitles.add(list.title);

                return (
                    <ListSection
                        key={list.id}
                        list={list}
                        listDiffs={listDiffs}
                        claimsOwnDiffs={claimsOwnDiffs}
                        context={context} />
                );
            })}
        </div>
    );
}

type ListSectionProps = {
    list: ListData;
    listDiffs: DiffItemView[];
    claimsOwnDiffs: boolean;
    context: ResourceTreeContext;
};

// Lists are keyed by title rather than id, because the two providers mint different ids for the
// same clinical list and expansion has to stay in step across the two cards.
function ListSection({
    list,
    listDiffs,
    claimsOwnDiffs,
    context
}: ListSectionProps) {
    const expansionKey = expansionKeys.list(list.title);
    const isExpanded = context.expansion.isExpanded(expansionKey);

    const visitedRefs = new Set<string>([`List/${list.id}`]);

    // A resource type with a dedicated section of its own is never shown here too - a feed that
    // folds Observations into a list titled "Miscellaneous record" still gets them grouped under
    // a name that says what they are, and the provider's own list renders only what is left.
    const visibleItemRefs = list.itemRefs.filter(itemRef =>
        dedicatedSectionResourceTypes.includes(itemRef.split("/")[0]) === false);

    // Diffs against the List resource itself - which entry points at which item, or its subject -
    // rather than against any one item it points to. entry-count-mismatch is handled separately
    // as the sizeDiff badge below, and manual-review-required has no matched list to attach to
    // (it surfaces in Other differences instead), so both are left out here. Only the list that
    // claimed this title shows them at all - see the comment in ListsSection.
    const ownDiffs = claimsOwnDiffs
        ? listDiffs.filter(diff =>
            diff.identifierText === list.title
            && diff.type !== "entry-count-mismatch"
            && diff.type !== "manual-review-required")
        : [];

    // Everything this list had is shown elsewhere now - nothing left here is worth a row, unless
    // the list itself still has an unresolved difference to show, as opposed to a list the
    // provider itself sent empty, which still says so.
    if (list.itemRefs.length > 0 && visibleItemRefs.length === 0 && ownDiffs.length === 0) {
        return null;
    }

    const sizeDiff = claimsOwnDiffs
        ? listDiffs.find(diff =>
            diff.identifierText === list.title && diff.type === "entry-count-mismatch")
        : undefined;

    const countBadgeTitle = sizeDiff
        ? `List size differs: the primary source has ${sizeDiff.oldValueText} items, `
        + `the secondary has ${sizeDiff.newValueText}`
        : undefined;

    const itemDiffs = [...getDiffsForReferences(visibleItemRefs, context), ...ownDiffs];

    return (
        <div className="mb-2">
            <ExpandableRow
                expanded={isExpanded}
                onToggle={() => context.expansion.toggleExpanded(expansionKey)}
                label={<span>{list.title}</span>}
                badges={
                    <>
                        {sizeDiff
                            ? (
                                <Badge
                                    bg="warning"
                                    className="border border-2 border-danger"
                                    style={badgeFontSize}
                                    title={countBadgeTitle}>
                                    {list.itemCount} item{list.itemCount === 1 ? "" : "s"}
                                </Badge>
                            )
                            : <ItemCountBadge count={list.itemCount} />}

                        <DiffCountBadge diffs={itemDiffs} />
                    </>
                } />

            {isExpanded && (
                <div className="ms-3 mt-2">
                    {visibleItemRefs.length === 0 && ownDiffs.length === 0 && (
                        <div className="text-muted small">No items in list</div>
                    )}

                    {visibleItemRefs.length === 0 && ownDiffs.length > 0 && (
                        <div className="text-muted small mb-2">
                            This list's items are shown in their own section above.
                        </div>
                    )}

                    {ownDiffs.length > 0 && (
                        <DiffHighlight fieldDiffs={ownDiffs} acceptance={context.acceptance}>
                            <div className="small">
                                Differences against this list itself, such as which item an
                                entry points at
                            </div>
                        </DiffHighlight>
                    )}

                    {visibleItemRefs.length > 0 && (
                        <div className="list-group list-group-flush">
                            {visibleItemRefs.map((itemRef, index) => (
                                <ResourceReference
                                    key={`${itemRef}-${index}`}
                                    reference={itemRef}
                                    expansionKey={
                                        expansionKeys.listItem(list.title, index)}
                                    visitedRefs={visitedRefs}
                                    context={context} />
                            ))}
                        </div>
                    )}
                </div>
            )}
        </div>
    );
}

type UnlistedResourceSectionProps = {
    title: string;
    references: string[];
    context: ResourceTreeContext;
};

// A section for resources of one type that no provider List points at - a feed can bundle
// Immunizations or Encounters without ever mentioning them in a List, and those would otherwise
// have nowhere on the card to appear at all, raw JSON or not. Keyed and laid out the same way as
// a List's own section, so it reads as one more clinical list rather than a different kind of
// thing.
export function UnlistedResourceSection({ title, references, context }: UnlistedResourceSectionProps) {
    if (references.length === 0) {
        return null;
    }

    const expansionKey = expansionKeys.list(title);
    const isExpanded = context.expansion.isExpanded(expansionKey);
    const itemDiffs = getDiffsForReferences(references, context);

    return (
        <div className="mb-2">
            <ExpandableRow
                expanded={isExpanded}
                onToggle={() => context.expansion.toggleExpanded(expansionKey)}
                label={<span>{title}</span>}
                badges={
                    <>
                        <ItemCountBadge count={references.length} />

                        <DiffCountBadge diffs={itemDiffs} />
                    </>
                } />

            {isExpanded && (
                <div className="ms-3 mt-2">
                    <div className="list-group list-group-flush">
                        {references.map((reference, index) => (
                            <ResourceReference
                                key={`${reference}-${index}`}
                                reference={reference}
                                expansionKey={expansionKeys.listItem(title, index)}
                                visitedRefs={new Set()}
                                context={context} />
                        ))}
                    </div>
                </div>
            )}
        </div>
    );
}

type ResourceReferenceProps = {
    reference: string;

    // Where this reference sits in the card, not what it points at. The two providers give the
    // same clinical fact different ids, so only the slot is comparable across the two sides.
    expansionKey: string;

    visitedRefs: Set<string>;
    context: ResourceTreeContext;

    // Differences recorded against the field that points here rather than against the target
    // resource itself - forwarded to whichever renderer below accepts it (Organization,
    // Practitioner, Medication); every other renderer finds its own differences by resource type
    // and ignores this.
    fieldDiffs?: DiffItemView[];
};

// One place that turns a reference into the right section. Bundles are graphs rather than trees -
// a practitioner role points at a practitioner who asserts an allergy - so a reference already on
// the path is shown as circular instead of followed, and rendering never recurses forever.
export function ResourceReference({
    reference,
    expansionKey,
    visitedRefs,
    context,
    fieldDiffs = []
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
            return (
                <ConditionItem
                    reference={reference}
                    expansionKey={expansionKey}
                    context={context} />
            );

        case "AllergyIntolerance":
            return (
                <AllergyItem
                    reference={reference}
                    expansionKey={expansionKey}
                    visitedRefs={nextVisitedRefs}
                    context={context} />
            );

        case "MedicationStatement":
            return (
                <MedicationStatementItem
                    reference={reference}
                    expansionKey={expansionKey}
                    visitedRefs={nextVisitedRefs}
                    context={context} />
            );

        case "Observation":
            return (
                <ObservationItem
                    reference={reference}
                    expansionKey={expansionKey}
                    visitedRefs={nextVisitedRefs}
                    context={context} />
            );

        case "Immunization":
            return (
                <ImmunizationItem
                    reference={reference}
                    expansionKey={expansionKey}
                    visitedRefs={nextVisitedRefs}
                    context={context} />
            );

        case "Encounter":
            return (
                <EncounterItem
                    reference={reference}
                    expansionKey={expansionKey}
                    visitedRefs={nextVisitedRefs}
                    context={context} />
            );

        case "FamilyMemberHistory":
            return (
                <FamilyMemberHistoryItem
                    reference={reference}
                    expansionKey={expansionKey}
                    visitedRefs={nextVisitedRefs}
                    context={context} />
            );

        case "PractitionerRole":
            return (
                <PractitionerRoleItem
                    reference={reference}
                    expansionKey={expansionKey}
                    visitedRefs={nextVisitedRefs}
                    context={context} />
            );

        case "Practitioner":
            return (
                <PractitionerSection
                    reference={reference}
                    expansionKey={expansionKey}
                    fieldDiffs={fieldDiffs}
                    context={context} />
            );

        case "Organization":
            return (
                <OrganizationSection
                    reference={reference}
                    expansionKey={expansionKey}
                    fieldDiffs={fieldDiffs}
                    context={context} />
            );

        case "Medication":
            return (
                <MedicationItem
                    reference={reference}
                    expansionKey={expansionKey}
                    fieldDiffs={fieldDiffs}
                    context={context} />
            );

        case "Location":
            return (
                <LocationSection
                    reference={reference}
                    expansionKey={expansionKey}
                    fieldDiffs={fieldDiffs}
                    context={context} />
            );

        case "MedicationRequest":
            return (
                <MedicationRequestItem
                    reference={reference}
                    expansionKey={expansionKey}
                    visitedRefs={nextVisitedRefs}
                    context={context} />
            );

        case "DiagnosticReport":
            return (
                <DiagnosticReportItem
                    reference={reference}
                    expansionKey={expansionKey}
                    context={context} />
            );

        case "Procedure":
            return (
                <ProcedureItem
                    reference={reference}
                    expansionKey={expansionKey}
                    context={context} />
            );

        case "ProcedureRequest":
            return (
                <ProcedureRequestItem
                    reference={reference}
                    expansionKey={expansionKey}
                    context={context} />
            );

        case "ReferralRequest":
            return (
                <ReferralRequestItem
                    reference={reference}
                    expansionKey={expansionKey}
                    context={context} />
            );

        case "RelatedPerson":
            return (
                <RelatedPersonItem
                    reference={reference}
                    expansionKey={expansionKey}
                    context={context} />
            );

        case "Appointment":
            return (
                <AppointmentItem
                    reference={reference}
                    expansionKey={expansionKey}
                    context={context} />
            );

        default:
            return (
                <GenericItem
                    reference={reference}
                    expansionKey={expansionKey}
                    context={context} />
            );
    }
}

type ReferenceOnlyProps = {
    reference: string;
    expansionKey: string;
    context: ResourceTreeContext;

    // Differences recorded against the field that points here - e.g. Patient.managingOrganization
    // - rather than against the target resource itself. Two providers mint different ids for what
    // may be the same real-world organisation or practitioner, so the diff is about *which*
    // resource is referenced, not about a field the resource's own summary renders - the collapsed
    // row's badge is enough to flag that, and the reference itself is shown in the expanded panel
    // so the acceptance tick is not left floating over a summary with nothing visibly different in
    // it.
    fieldDiffs?: DiffItemView[];
};

export function OrganizationSection({
    reference,
    expansionKey,
    context,
    fieldDiffs = []
}: ReferenceOnlyProps) {
    const expanded = context.expansion.isExpanded(expansionKey);
    const resource = context.resourceIndex.get(reference) ?? null;
    const organization = resource === null ? null : parseOrganization(resource);

    // Which organisation is referenced (fieldDiffs) and what that organisation's own fields say
    // (ownDiffs) are different facts the comparison can report - both belong on this one row.
    const ownMatchKey = resource === null ? null : getResourceMatchKey(
        "Organization",
        resource,
        context.resourceIndex);

    const ownDiffs = getMatchedDiffs(
        context.diffsByResourceType.get("Organization") ?? [],
        ownMatchKey);

    const allDiffs = [...fieldDiffs, ...ownDiffs];

    return (
        <div className="mb-2">
            <ExpandableRow
                expanded={expanded}
                onToggle={() => context.expansion.toggleExpanded(expansionKey)}
                label={<span>{organization?.name ?? reference}</span>}
                badges={
                    <>
                        {resource === null && (
                            <Badge bg="secondary" className="ms-1" style={badgeFontSize}>
                                not in bundle
                            </Badge>
                        )}

                        <DiffCountBadge diffs={allDiffs} />
                    </>
                } />

            {expanded && organization !== null && (
                <DiffHighlight fieldDiffs={allDiffs} acceptance={context.acceptance}>
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

                            {fieldDiffs.length > 0 && (
                                <Col xs={12}>
                                    <small className="text-muted d-block">Reference</small>
                                    <div>
                                        <code style={{ overflowWrap: "anywhere" }}>{reference}</code>
                                    </div>
                                </Col>
                            )}
                        </Row>

                        <ResourceJsonToggle resource={resource} />
                    </div>
                </DiffHighlight>
            )}
        </div>
    );
}

export function PractitionerSection({
    reference,
    expansionKey,
    context,
    fieldDiffs = []
}: ReferenceOnlyProps) {
    const expanded = context.expansion.isExpanded(expansionKey);
    const resource = context.resourceIndex.get(reference) ?? null;
    const practitioner = resource === null ? null : parsePractitioner(resource);

    // Which practitioner is referenced (fieldDiffs) and what that practitioner's own fields say
    // (ownDiffs) are different facts the comparison can report - both belong on this one row.
    const ownMatchKey = resource === null ? null : getResourceMatchKey(
        "Practitioner",
        resource,
        context.resourceIndex);

    const ownDiffs = getMatchedDiffs(
        context.diffsByResourceType.get("Practitioner") ?? [],
        ownMatchKey);

    const allDiffs = [...fieldDiffs, ...ownDiffs];

    return (
        <div className="mb-2">
            <ExpandableRow
                expanded={expanded}
                onToggle={() => context.expansion.toggleExpanded(expansionKey)}
                label={<span>{practitioner?.displayName ?? reference}</span>}
                badges={
                    <>
                        {resource === null && (
                            <Badge bg="secondary" className="ms-1" style={badgeFontSize}>
                                not in bundle
                            </Badge>
                        )}

                        <DiffCountBadge diffs={allDiffs} />
                    </>
                } />

            {expanded && practitioner !== null && (
                <DiffHighlight fieldDiffs={allDiffs} acceptance={context.acceptance}>
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

                            {fieldDiffs.length > 0 && (
                                <Col xs={12}>
                                    <small className="text-muted d-block">Reference</small>
                                    <div>
                                        <code style={{ overflowWrap: "anywhere" }}>{reference}</code>
                                    </div>
                                </Col>
                            )}
                        </Row>

                        <ResourceJsonToggle resource={resource} />
                    </div>
                </DiffHighlight>
            )}
        </div>
    );
}

type NestingProps = {
    reference: string;
    expansionKey: string;
    visitedRefs: Set<string>;
    context: ResourceTreeContext;
};

function PractitionerRoleItem({ reference, expansionKey, visitedRefs, context }: NestingProps) {
    const isExpanded = context.expansion.isExpanded(expansionKey);
    const toggleExpanded = () => context.expansion.toggleExpanded(expansionKey);

    const resource = context.resourceIndex.get(reference) ?? null;
    const practitionerRole = resource === null ? null : parsePractitionerRole(resource);

    const matchKey = resource === null ? null : getResourceMatchKey(
        "PractitionerRole",
        resource,
        context.resourceIndex);

    const itemDiffs = getMatchedDiffs(
        context.diffsByResourceType.get("PractitionerRole") ?? [],
        matchKey);

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
                badges={
                    <>
                        {resource === null && (
                            <Badge bg="secondary" style={badgeFontSize}>
                                not in bundle
                            </Badge>
                        )}

                        <DiffCountBadge diffs={itemDiffs} />
                    </>
                } />

            {isExpanded && practitionerRole !== null && (
                <DiffHighlight fieldDiffs={itemDiffs} acceptance={context.acceptance}>
                    <div className="ms-3 mt-2 p-2 border rounded bg-light">
                        {practitionerRole.practitionerRef !== null && (
                            <div className="mb-2">
                                <small className="text-muted d-block">Practitioner</small>

                                <ResourceReference
                                    reference={practitionerRole.practitionerRef}
                                    expansionKey={expansionKeys.nested(expansionKey, "practitioner")}
                                    visitedRefs={visitedRefs}
                                    context={context} />
                            </div>
                        )}

                        {practitionerRole.organizationRef !== null && (
                            <div className="mb-2">
                                <small className="text-muted d-block">Organisation</small>

                                <ResourceReference
                                    reference={practitionerRole.organizationRef}
                                    expansionKey={expansionKeys.nested(expansionKey, "organization")}
                                    visitedRefs={visitedRefs}
                                    context={context} />
                            </div>
                        )}

                        <ResourceJsonToggle resource={resource} />
                    </div>
                </DiffHighlight>
            )}
        </div>
    );
}

function ConditionItem({ reference, expansionKey, context }: ReferenceOnlyProps) {
    const isExpanded = context.expansion.isExpanded(expansionKey);
    const toggleExpanded = () => context.expansion.toggleExpanded(expansionKey);

    const resource = context.resourceIndex.get(reference) ?? null;
    const condition = resource === null ? null : parseCondition(resource);

    const matchKey = resource === null ? null : buildConditionMatchKey(resource);
    const itemDiffs = getMatchedDiffs(context.diffsByResourceType.get("Condition") ?? [], matchKey);
    const subjectDiffs = itemDiffs.filter(diff => diff.path.endsWith(".subject.reference"));

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
                badges={<DiffCountBadge diffs={itemDiffs} />} />

            {isExpanded && condition !== null && (
                <DiffHighlight fieldDiffs={itemDiffs} acceptance={context.acceptance}>
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

                            {condition.subjectRef !== null && (
                                <Col xs={12}>
                                    <small className="text-muted d-block">Subject</small>

                                    <DiffHighlight
                                        fieldDiffs={subjectDiffs}
                                        acceptance={context.acceptance}
                                        inline>
                                        <div>
                                            <code style={{ overflowWrap: "anywhere" }}>{condition.subjectRef}</code>
                                        </div>
                                    </DiffHighlight>
                                </Col>
                            )}
                        </Row>

                        <ResourceJsonToggle resource={resource} />
                    </div>
                </DiffHighlight>
            )}
        </div>
    );
}

function AllergyItem({ reference, expansionKey, visitedRefs, context }: NestingProps) {
    const isExpanded = context.expansion.isExpanded(expansionKey);
    const toggleExpanded = () => context.expansion.toggleExpanded(expansionKey);

    const resource = context.resourceIndex.get(reference) ?? null;
    const allergy = resource === null ? null : parseAllergyIntolerance(resource);

    const matchKey = resource === null ? null : buildAllergyIntoleranceMatchKey(resource);

    const itemDiffs = getMatchedDiffs(
        context.diffsByResourceType.get("AllergyIntolerance") ?? [],
        matchKey);

    const asserterDiffs = itemDiffs.filter(diff => diff.path.endsWith(".asserter.reference"));

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
                badges={<DiffCountBadge diffs={itemDiffs} />} />

            {isExpanded && allergy !== null && (
                <DiffHighlight fieldDiffs={itemDiffs} acceptance={context.acceptance}>
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
                                        expansionKey={expansionKeys.nested(expansionKey, "asserter")}
                                        visitedRefs={visitedRefs}
                                        fieldDiffs={asserterDiffs}
                                        context={context} />
                                </Col>
                            )}
                        </Row>

                        <ResourceJsonToggle resource={resource} />
                    </div>
                </DiffHighlight>
            )}
        </div>
    );
}

type MedicationStatementItemProps = {
    reference: string;
    expansionKey: string;
    visitedRefs: Set<string>;
    context: ResourceTreeContext;
};

function MedicationStatementItem({
    reference,
    expansionKey,
    visitedRefs,
    context
}: MedicationStatementItemProps) {
    const isExpanded = context.expansion.isExpanded(expansionKey);
    const toggleExpanded = () => context.expansion.toggleExpanded(expansionKey);

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

    const itemDiffs = getMatchedDiffs(
        context.diffsByResourceType.get("MedicationStatement") ?? [],
        matchKey);

    const dosageDiffs = itemDiffs.filter(diff => diff.path.endsWith(".dosage[0].text"));

    const informationSourceDiffs = itemDiffs.filter(diff =>
        diff.path.endsWith(".informationSource.reference"));

    const medicationRefDiffs = itemDiffs.filter(diff =>
        diff.path.endsWith(".medicationReference.reference"));

    const subjectDiffs = itemDiffs.filter(diff => diff.path.endsWith(".subject.reference"));

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
                badges={<DiffCountBadge diffs={itemDiffs} />} />

            {isExpanded && medicationStatement !== null && (
                <DiffHighlight fieldDiffs={itemDiffs} acceptance={context.acceptance}>
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

                                    <DiffHighlight
                                        fieldDiffs={dosageDiffs}
                                        acceptance={context.acceptance}
                                        inline>
                                        {medicationStatement.dosage}
                                    </DiffHighlight>
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
                                        expansionKey={
                                            expansionKeys.nested(expansionKey, "informationSource")}
                                        visitedRefs={visitedRefs}
                                        fieldDiffs={informationSourceDiffs}
                                        context={context} />
                                </Col>
                            )}

                            {medicationStatement.medicationRef !== null && (
                                <Col xs={12}>
                                    <small className="text-muted d-block">Medication resource</small>

                                    <ResourceReference
                                        reference={medicationStatement.medicationRef}
                                        expansionKey={expansionKeys.nested(expansionKey, "medication")}
                                        visitedRefs={visitedRefs}
                                        fieldDiffs={medicationRefDiffs}
                                        context={context} />
                                </Col>
                            )}

                            {medicationStatement.subjectRef !== null && (
                                <Col xs={12}>
                                    <small className="text-muted d-block">Subject</small>

                                    <DiffHighlight
                                        fieldDiffs={subjectDiffs}
                                        acceptance={context.acceptance}
                                        inline>
                                        <div>
                                            <code style={{ overflowWrap: "anywhere" }}>{medicationStatement.subjectRef}</code>
                                        </div>
                                    </DiffHighlight>
                                </Col>
                            )}
                        </Row>

                        <ResourceJsonToggle resource={resource} />
                    </div>
                </DiffHighlight>
            )}
        </div>
    );
}

function ObservationItem({ reference, expansionKey, visitedRefs, context }: NestingProps) {
    const isExpanded = context.expansion.isExpanded(expansionKey);
    const toggleExpanded = () => context.expansion.toggleExpanded(expansionKey);

    const resource = context.resourceIndex.get(reference) ?? null;
    const observation = resource === null ? null : parseObservation(resource);

    const matchKey = resource === null ? null : buildDdsIdentifierMatchKey(resource);
    const diffsForType = context.diffsByResourceType.get("Observation") ?? [];
    const itemDiffs = getMatchedDiffs(diffsForType, matchKey);
    const ambiguousDiffs = getAmbiguousDiffs(diffsForType, matchKey);
    const subjectDiffs = itemDiffs.filter(diff => diff.path.endsWith(".subject.reference"));

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
                badges={<DiffCountBadge diffs={itemDiffs} />} />

            {isExpanded && observation !== null && (
                <DiffHighlight fieldDiffs={itemDiffs} acceptance={context.acceptance}>
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

                            {observation.subjectRef !== null && (
                                <Col xs={12}>
                                    <small className="text-muted d-block">Subject</small>

                                    <DiffHighlight
                                        fieldDiffs={subjectDiffs}
                                        acceptance={context.acceptance}
                                        inline>
                                        <div>
                                            <code style={{ overflowWrap: "anywhere" }}>{observation.subjectRef}</code>
                                        </div>
                                    </DiffHighlight>
                                </Col>
                            )}

                            {observation.performerRefs.length > 0 && (
                                <Col xs={12}>
                                    <small className="text-muted d-block">Performer(s)</small>

                                    {observation.performerRefs.map((performerRef, performerIndex) => (
                                        <ResourceReference
                                            key={performerRef}
                                            reference={performerRef}
                                            expansionKey={
                                                expansionKeys.nested(
                                                    expansionKey,
                                                    `performer[${performerIndex}]`)}
                                            visitedRefs={visitedRefs}
                                            fieldDiffs={itemDiffs.filter(diff =>
                                                diff.path.endsWith(
                                                    `.performer[${performerIndex}].reference`))}
                                            context={context} />
                                    ))}
                                </Col>
                            )}
                        </Row>

                        <AmbiguousMatchNotice diffs={ambiguousDiffs} acceptance={context.acceptance} />

                        <ResourceJsonToggle resource={resource} />
                    </div>
                </DiffHighlight>
            )}
        </div>
    );
}

function ImmunizationItem({ reference, expansionKey, visitedRefs, context }: NestingProps) {
    const isExpanded = context.expansion.isExpanded(expansionKey);
    const toggleExpanded = () => context.expansion.toggleExpanded(expansionKey);

    const resource = context.resourceIndex.get(reference) ?? null;
    const immunization = resource === null ? null : parseImmunization(resource);

    const matchKey = resource === null ? null : buildDdsIdentifierMatchKey(resource);
    const itemDiffs = getMatchedDiffs(context.diffsByResourceType.get("Immunization") ?? [], matchKey);
    const patientDiffs = itemDiffs.filter(diff => diff.path.endsWith(".patient.reference"));

    return (
        <div className="py-1 ps-2 border-bottom">
            <ExpandableRow
                expanded={isExpanded}
                onToggle={toggleExpanded}
                label={
                    <span className="small">
                        Immunization:
                        {immunization?.code ? "" : ` ${immunization?.display ?? reference}`}
                    </span>
                }
                trailing={immunization?.code && (
                    <span className="small">
                        <CodeWithInfo
                            display={immunization.display}
                            code={immunization.code}
                            system={immunization.system} />
                    </span>
                )}
                badges={<DiffCountBadge diffs={itemDiffs} />} />

            {isExpanded && immunization !== null && (
                <DiffHighlight fieldDiffs={itemDiffs} acceptance={context.acceptance}>
                    <div className="ms-3 mt-2 p-2 border rounded bg-light">
                        <Row className="g-1">
                            <Col xs={12}>
                                <small className="text-muted d-block">Display</small>
                                <div>{immunization.display ?? "N/A"}</div>
                            </Col>

                            {immunization.code !== null && (
                                <Col xs={12}>
                                    <small className="text-muted d-block">Code</small>

                                    <div>
                                        <CodeWithInfo
                                            display={immunization.display}
                                            code={immunization.code}
                                            system={immunization.system} />
                                    </div>
                                </Col>
                            )}

                            {immunization.status !== null && (
                                <Col xs={6}>
                                    <small className="text-muted d-block">Status</small>
                                    <div>{immunization.status}</div>
                                </Col>
                            )}

                            {immunization.occurrenceDateTime !== null && (
                                <Col xs={6}>
                                    <small className="text-muted d-block">Date</small>
                                    <div>{formatFhirDate(immunization.occurrenceDateTime)}</div>
                                </Col>
                            )}

                            {immunization.patientRef !== null && (
                                <Col xs={12}>
                                    <small className="text-muted d-block">Patient</small>

                                    <DiffHighlight
                                        fieldDiffs={patientDiffs}
                                        acceptance={context.acceptance}
                                        inline>
                                        <div>
                                            <code style={{ overflowWrap: "anywhere" }}>{immunization.patientRef}</code>
                                        </div>
                                    </DiffHighlight>
                                </Col>
                            )}

                            {immunization.practitionerRefs.length > 0 && (
                                <Col xs={12}>
                                    <small className="text-muted d-block">Practitioner(s)</small>

                                    {immunization.practitionerRefs.map(
                                        (practitionerRef, practitionerIndex) => (
                                            <ResourceReference
                                                key={practitionerRef}
                                                reference={practitionerRef}
                                                expansionKey={
                                                    expansionKeys.nested(
                                                        expansionKey,
                                                        `practitioner[${practitionerIndex}]`)}
                                                visitedRefs={visitedRefs}
                                                fieldDiffs={itemDiffs.filter(diff =>
                                                    diff.path.endsWith(
                                                        `.practitioner[${practitionerIndex}]`
                                                        + ".actor.reference"))}
                                                context={context} />
                                        ))}
                                </Col>
                            )}
                        </Row>

                        <ResourceJsonToggle resource={resource} />
                    </div>
                </DiffHighlight>
            )}
        </div>
    );
}

function EncounterItem({ reference, expansionKey, visitedRefs, context }: NestingProps) {
    const isExpanded = context.expansion.isExpanded(expansionKey);
    const toggleExpanded = () => context.expansion.toggleExpanded(expansionKey);

    const resource = context.resourceIndex.get(reference) ?? null;
    const encounter = resource === null ? null : parseEncounter(resource);

    const matchKey = resource === null ? null : buildDdsIdentifierMatchKey(resource);
    const itemDiffs = getMatchedDiffs(context.diffsByResourceType.get("Encounter") ?? [], matchKey);
    const subjectDiffs = itemDiffs.filter(diff => diff.path.endsWith(".subject.reference"));

    return (
        <div className="py-1 ps-2 border-bottom">
            <ExpandableRow
                expanded={isExpanded}
                onToggle={toggleExpanded}
                label={
                    <span className="small">
                        Encounter:{encounter?.code ? "" : ` ${encounter?.display ?? reference}`}
                    </span>
                }
                trailing={encounter?.code && (
                    <span className="small">
                        <CodeWithInfo
                            display={encounter.display}
                            code={encounter.code}
                            system={encounter.system} />
                    </span>
                )}
                badges={<DiffCountBadge diffs={itemDiffs} />} />

            {isExpanded && encounter !== null && (
                <DiffHighlight fieldDiffs={itemDiffs} acceptance={context.acceptance}>
                    <div className="ms-3 mt-2 p-2 border rounded bg-light">
                        <Row className="g-1">
                            <Col xs={12}>
                                <small className="text-muted d-block">Display</small>
                                <div>{encounter.display ?? "N/A"}</div>
                            </Col>

                            {encounter.code !== null && (
                                <Col xs={12}>
                                    <small className="text-muted d-block">Code</small>

                                    <div>
                                        <CodeWithInfo
                                            display={encounter.display}
                                            code={encounter.code}
                                            system={encounter.system} />
                                    </div>
                                </Col>
                            )}

                            {encounter.status !== null && (
                                <Col xs={6}>
                                    <small className="text-muted d-block">Status</small>
                                    <div>{encounter.status}</div>
                                </Col>
                            )}

                            {encounter.periodStart !== null && (
                                <Col xs={6}>
                                    <small className="text-muted d-block">Start</small>
                                    <div>{formatFhirDate(encounter.periodStart)}</div>
                                </Col>
                            )}

                            {encounter.subjectRef !== null && (
                                <Col xs={12}>
                                    <small className="text-muted d-block">Subject</small>

                                    <DiffHighlight
                                        fieldDiffs={subjectDiffs}
                                        acceptance={context.acceptance}
                                        inline>
                                        <div>
                                            <code style={{ overflowWrap: "anywhere" }}>{encounter.subjectRef}</code>
                                        </div>
                                    </DiffHighlight>
                                </Col>
                            )}

                            {encounter.participantRefs.length > 0 && (
                                <Col xs={12}>
                                    <small className="text-muted d-block">Participant(s)</small>

                                    {encounter.participantRefs.map(
                                        (participantRef, participantIndex) => (
                                            <ResourceReference
                                                key={participantRef}
                                                reference={participantRef}
                                                expansionKey={
                                                    expansionKeys.nested(
                                                        expansionKey,
                                                        `participant[${participantIndex}]`)}
                                                visitedRefs={visitedRefs}
                                                fieldDiffs={itemDiffs.filter(diff =>
                                                    diff.path.endsWith(
                                                        `.participant[${participantIndex}]`
                                                        + ".individual.reference"))}
                                                context={context} />
                                        ))}
                                </Col>
                            )}
                        </Row>

                        <ResourceJsonToggle resource={resource} />
                    </div>
                </DiffHighlight>
            )}
        </div>
    );
}

function FamilyMemberHistoryItem({ reference, expansionKey, visitedRefs, context }: NestingProps) {
    const isExpanded = context.expansion.isExpanded(expansionKey);
    const toggleExpanded = () => context.expansion.toggleExpanded(expansionKey);

    const resource = context.resourceIndex.get(reference) ?? null;
    const familyMemberHistory = resource === null ? null : parseFamilyMemberHistory(resource);

    const matchKey = resource === null ? null : buildDdsIdentifierMatchKey(resource);
    const diffsForType = context.diffsByResourceType.get("FamilyMemberHistory") ?? [];
    const itemDiffs = getMatchedDiffs(diffsForType, matchKey);
    const ambiguousDiffs = getAmbiguousDiffs(diffsForType, matchKey);

    // A relative's relationship wins when it is there; some feeds instead record a screening
    // statement about the whole family - "no family history of malignancy" - with no relative or
    // relationship at all, only a condition code.
    const primaryCode =
        (familyMemberHistory?.relationshipCode ?? familyMemberHistory?.conditionCode) ?? null;

    const primaryDisplay =
        (familyMemberHistory?.relationshipDisplay ?? familyMemberHistory?.conditionDisplay) ?? null;

    const primarySystem =
        (familyMemberHistory?.relationshipSystem ?? familyMemberHistory?.conditionSystem) ?? null;

    const patientDiffs = itemDiffs.filter(diff => diff.path.endsWith(".patient.reference"));

    // The recorder is carried as the one extension on this resource with a reference value, so
    // there is no numbered path segment worth matching on beyond that.
    const recorderDiffs = itemDiffs.filter(diff => diff.path.endsWith(".valueReference.reference"));

    return (
        <div className="py-1 ps-2 border-bottom">
            <ExpandableRow
                expanded={isExpanded}
                onToggle={toggleExpanded}
                label={
                    <span className="small">
                        Family history:
                        {primaryCode ? "" : ` ${familyMemberHistory?.name ?? reference}`}
                    </span>
                }
                trailing={primaryCode && (
                    <span className="small">
                        <CodeWithInfo
                            display={primaryDisplay}
                            code={primaryCode}
                            system={primarySystem} />
                    </span>
                )}
                badges={<DiffCountBadge diffs={itemDiffs} />} />

            {isExpanded && familyMemberHistory !== null && (
                <DiffHighlight fieldDiffs={itemDiffs} acceptance={context.acceptance}>
                    <div className="ms-3 mt-2 p-2 border rounded bg-light">
                        <Row className="g-1">
                            {familyMemberHistory.name !== null && (
                                <Col xs={12}>
                                    <small className="text-muted d-block">Name</small>
                                    <div>{familyMemberHistory.name}</div>
                                </Col>
                            )}

                            {familyMemberHistory.relationshipCode !== null && (
                                <Col xs={12}>
                                    <small className="text-muted d-block">Relationship</small>

                                    <div>
                                        <CodeWithInfo
                                            display={familyMemberHistory.relationshipDisplay}
                                            code={familyMemberHistory.relationshipCode}
                                            system={familyMemberHistory.relationshipSystem} />
                                    </div>
                                </Col>
                            )}

                            {familyMemberHistory.conditionCode !== null && (
                                <Col xs={12}>
                                    <small className="text-muted d-block">Condition</small>

                                    <div>
                                        <CodeWithInfo
                                            display={familyMemberHistory.conditionDisplay}
                                            code={familyMemberHistory.conditionCode}
                                            system={familyMemberHistory.conditionSystem} />
                                    </div>
                                </Col>
                            )}

                            {familyMemberHistory.status !== null && (
                                <Col xs={6}>
                                    <small className="text-muted d-block">Status</small>
                                    <div>{familyMemberHistory.status}</div>
                                </Col>
                            )}

                            {familyMemberHistory.bornDate !== null && (
                                <Col xs={6}>
                                    <small className="text-muted d-block">Born</small>
                                    <div>{formatFhirDate(familyMemberHistory.bornDate)}</div>
                                </Col>
                            )}

                            {familyMemberHistory.patientRef !== null && (
                                <Col xs={12}>
                                    <small className="text-muted d-block">Patient</small>

                                    <DiffHighlight
                                        fieldDiffs={patientDiffs}
                                        acceptance={context.acceptance}
                                        inline>
                                        <div>
                                            <code style={{ overflowWrap: "anywhere" }}>{familyMemberHistory.patientRef}</code>
                                        </div>
                                    </DiffHighlight>
                                </Col>
                            )}

                            {familyMemberHistory.recorderRef !== null && (
                                <Col xs={12}>
                                    <small className="text-muted d-block">Recorder</small>

                                    <ResourceReference
                                        reference={familyMemberHistory.recorderRef}
                                        expansionKey={expansionKeys.nested(expansionKey, "recorder")}
                                        visitedRefs={visitedRefs}
                                        fieldDiffs={recorderDiffs}
                                        context={context} />
                                </Col>
                            )}
                        </Row>

                        <AmbiguousMatchNotice diffs={ambiguousDiffs} acceptance={context.acceptance} />

                        <ResourceJsonToggle resource={resource} />
                    </div>
                </DiffHighlight>
            )}
        </div>
    );
}

function MedicationItem({
    reference,
    expansionKey,
    context,
    fieldDiffs = []
}: ReferenceOnlyProps) {
    const isExpanded = context.expansion.isExpanded(expansionKey);
    const toggleExpanded = () => context.expansion.toggleExpanded(expansionKey);

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
            <ExpandableRow
                expanded={isExpanded}
                onToggle={toggleExpanded}
                label={
                    <span className="small">
                        Medication:{medication.code !== null ? "" : ` ${medication.display ?? reference}`}
                    </span>
                }
                trailing={medication.code !== null && (
                    <span className="small">
                        <CodeWithInfo
                            display={medication.display}
                            code={medication.code}
                            system={medication.system} />
                    </span>
                )}
                badges={<DiffCountBadge diffs={fieldDiffs} />} />

            {isExpanded && (
                <DiffHighlight fieldDiffs={fieldDiffs} acceptance={context.acceptance}>
                    <div className="ms-3 mt-2 p-2 border rounded bg-light">
                        <Row className="g-1">
                            <Col xs={12}>
                                <small className="text-muted d-block">Display</small>
                                <div>{medication.display ?? "N/A"}</div>
                            </Col>

                            {medication.code !== null && (
                                <Col xs={12}>
                                    <small className="text-muted d-block">Code</small>

                                    <div>
                                        <CodeWithInfo
                                            display={medication.display}
                                            code={medication.code}
                                            system={medication.system} />
                                    </div>
                                </Col>
                            )}

                            {fieldDiffs.length > 0 && (
                                <Col xs={12}>
                                    <small className="text-muted d-block">Reference</small>
                                    <div>
                                        <code style={{ overflowWrap: "anywhere" }}>{reference}</code>
                                    </div>
                                </Col>
                            )}
                        </Row>

                        <ResourceJsonToggle resource={resource} />
                    </div>
                </DiffHighlight>
            )}
        </div>
    );
}

function MedicationRequestItem({ reference, expansionKey, visitedRefs, context }: NestingProps) {
    const isExpanded = context.expansion.isExpanded(expansionKey);
    const toggleExpanded = () => context.expansion.toggleExpanded(expansionKey);

    const resource = context.resourceIndex.get(reference) ?? null;
    const medicationRequest = resource === null ? null : parseMedicationRequest(resource);

    const matchKey = resource === null ? null : buildDdsIdentifierMatchKey(resource);

    const itemDiffs = getMatchedDiffs(
        context.diffsByResourceType.get("MedicationRequest") ?? [],
        matchKey);

    return (
        <div className="py-1 ps-2 border-bottom">
            <ExpandableRow
                expanded={isExpanded}
                onToggle={toggleExpanded}
                label={
                    <span className="small">
                        Medication request:
                        {medicationRequest?.code ? "" : ` ${medicationRequest?.display ?? reference}`}
                    </span>
                }
                trailing={medicationRequest?.code && (
                    <span className="small">
                        <CodeWithInfo
                            display={medicationRequest.display}
                            code={medicationRequest.code}
                            system={medicationRequest.system} />
                    </span>
                )}
                badges={<DiffCountBadge diffs={itemDiffs} />} />

            {isExpanded && medicationRequest !== null && (
                <DiffHighlight fieldDiffs={itemDiffs} acceptance={context.acceptance}>
                    <div className="ms-3 mt-2 p-2 border rounded bg-light">
                        <Row className="g-1">
                            <Col xs={12}>
                                <small className="text-muted d-block">Medication</small>
                                <div>{medicationRequest.display ?? "N/A"}</div>
                            </Col>

                            {medicationRequest.code !== null && (
                                <Col xs={12}>
                                    <small className="text-muted d-block">Code</small>

                                    <div>
                                        <CodeWithInfo
                                            display={medicationRequest.display}
                                            code={medicationRequest.code}
                                            system={medicationRequest.system} />
                                    </div>
                                </Col>
                            )}

                            {medicationRequest.dosage !== null && (
                                <Col xs={12}>
                                    <small className="text-muted d-block">Dosage</small>
                                    <div>{medicationRequest.dosage}</div>
                                </Col>
                            )}

                            {medicationRequest.status !== null && (
                                <Col xs={6}>
                                    <small className="text-muted d-block">Status</small>
                                    <div>{medicationRequest.status}</div>
                                </Col>
                            )}

                            {medicationRequest.authoredOn !== null && (
                                <Col xs={6}>
                                    <small className="text-muted d-block">Authored on</small>
                                    <div>{formatFhirDate(medicationRequest.authoredOn)}</div>
                                </Col>
                            )}

                            {medicationRequest.requesterRef !== null && (
                                <Col xs={12}>
                                    <small className="text-muted d-block">Requester</small>

                                    <ResourceReference
                                        reference={medicationRequest.requesterRef}
                                        expansionKey={expansionKeys.nested(expansionKey, "requester")}
                                        visitedRefs={visitedRefs}
                                        context={context} />
                                </Col>
                            )}
                        </Row>

                        <ResourceJsonToggle resource={resource} />
                    </div>
                </DiffHighlight>
            )}
        </div>
    );
}

function DiagnosticReportItem({ reference, expansionKey, context }: ReferenceOnlyProps) {
    const isExpanded = context.expansion.isExpanded(expansionKey);
    const toggleExpanded = () => context.expansion.toggleExpanded(expansionKey);

    const resource = context.resourceIndex.get(reference) ?? null;
    const diagnosticReport = resource === null ? null : parseDiagnosticReport(resource);

    const matchKey = resource === null ? null : buildDdsIdentifierMatchKey(resource);

    const itemDiffs = getMatchedDiffs(
        context.diffsByResourceType.get("DiagnosticReport") ?? [],
        matchKey);

    return (
        <div className="py-1 ps-2 border-bottom">
            <ExpandableRow
                expanded={isExpanded}
                onToggle={toggleExpanded}
                label={
                    <span className="small">
                        Diagnostic report:
                        {diagnosticReport?.code ? "" : ` ${diagnosticReport?.display ?? reference}`}
                    </span>
                }
                trailing={diagnosticReport?.code && (
                    <span className="small">
                        <CodeWithInfo
                            display={diagnosticReport.display}
                            code={diagnosticReport.code}
                            system={diagnosticReport.system} />
                    </span>
                )}
                badges={<DiffCountBadge diffs={itemDiffs} />} />

            {isExpanded && diagnosticReport !== null && (
                <DiffHighlight fieldDiffs={itemDiffs} acceptance={context.acceptance}>
                    <div className="ms-3 mt-2 p-2 border rounded bg-light">
                        <Row className="g-1">
                            <Col xs={12}>
                                <small className="text-muted d-block">Display</small>
                                <div>{diagnosticReport.display ?? "N/A"}</div>
                            </Col>

                            {diagnosticReport.code !== null && (
                                <Col xs={12}>
                                    <small className="text-muted d-block">Code</small>

                                    <div>
                                        <CodeWithInfo
                                            display={diagnosticReport.display}
                                            code={diagnosticReport.code}
                                            system={diagnosticReport.system} />
                                    </div>
                                </Col>
                            )}

                            {diagnosticReport.status !== null && (
                                <Col xs={6}>
                                    <small className="text-muted d-block">Status</small>
                                    <div>{diagnosticReport.status}</div>
                                </Col>
                            )}

                            {diagnosticReport.effectiveDateTime !== null && (
                                <Col xs={6}>
                                    <small className="text-muted d-block">Date</small>
                                    <div>{formatFhirDate(diagnosticReport.effectiveDateTime)}</div>
                                </Col>
                            )}
                        </Row>

                        <ResourceJsonToggle resource={resource} />
                    </div>
                </DiffHighlight>
            )}
        </div>
    );
}

export function LocationSection({
    reference,
    expansionKey,
    context,
    fieldDiffs = []
}: ReferenceOnlyProps) {
    const expanded = context.expansion.isExpanded(expansionKey);
    const resource = context.resourceIndex.get(reference) ?? null;
    const location = resource === null ? null : parseLocation(resource);

    const ownMatchKey = resource === null ? null : getResourceMatchKey(
        "Location",
        resource,
        context.resourceIndex);

    const ownDiffs = getMatchedDiffs(context.diffsByResourceType.get("Location") ?? [], ownMatchKey);
    const allDiffs = [...fieldDiffs, ...ownDiffs];

    return (
        <div className="mb-2">
            <ExpandableRow
                expanded={expanded}
                onToggle={() => context.expansion.toggleExpanded(expansionKey)}
                label={<span>{location?.name ?? reference}</span>}
                badges={
                    <>
                        {resource === null && (
                            <Badge bg="secondary" className="ms-1" style={badgeFontSize}>
                                not in bundle
                            </Badge>
                        )}

                        <DiffCountBadge diffs={allDiffs} />
                    </>
                } />

            {expanded && location !== null && (
                <DiffHighlight fieldDiffs={allDiffs} acceptance={context.acceptance}>
                    <div className="ms-3 mt-2 p-2 border rounded bg-light">
                        <Row className="g-1">
                            <Col xs={12}>
                                <small className="text-muted d-block">Name</small>
                                <div>{location.name ?? "N/A"}</div>
                            </Col>

                            {location.odsSiteCode !== null && (
                                <Col xs={6}>
                                    <small className="text-muted d-block">ODS site code</small>

                                    <div>
                                        <CodeWithInfo
                                            display={null}
                                            code={location.odsSiteCode}
                                            system={location.odsSiteSystem} />
                                    </div>
                                </Col>
                            )}

                            {location.addressLine !== null && (
                                <Col xs={12}>
                                    <small className="text-muted d-block">Address line</small>
                                    <div>{location.addressLine}</div>
                                </Col>
                            )}

                            {location.addressCity !== null && (
                                <Col xs={6}>
                                    <small className="text-muted d-block">City</small>
                                    <div>{location.addressCity}</div>
                                </Col>
                            )}

                            {location.addressPostalCode !== null && (
                                <Col xs={6}>
                                    <small className="text-muted d-block">Postcode</small>
                                    <div>{location.addressPostalCode}</div>
                                </Col>
                            )}

                            {fieldDiffs.length > 0 && (
                                <Col xs={12}>
                                    <small className="text-muted d-block">Reference</small>
                                    <div>
                                        <code style={{ overflowWrap: "anywhere" }}>{reference}</code>
                                    </div>
                                </Col>
                            )}
                        </Row>

                        <ResourceJsonToggle resource={resource} />
                    </div>
                </DiffHighlight>
            )}
        </div>
    );
}

function ProcedureItem({ reference, expansionKey, context }: ReferenceOnlyProps) {
    const isExpanded = context.expansion.isExpanded(expansionKey);
    const toggleExpanded = () => context.expansion.toggleExpanded(expansionKey);

    const resource = context.resourceIndex.get(reference) ?? null;
    const procedure = resource === null ? null : parseProcedure(resource);

    const matchKey = resource === null ? null : buildDdsIdentifierMatchKey(resource);
    const itemDiffs = getMatchedDiffs(context.diffsByResourceType.get("Procedure") ?? [], matchKey);

    return (
        <div className="py-1 ps-2 border-bottom">
            <ExpandableRow
                expanded={isExpanded}
                onToggle={toggleExpanded}
                label={
                    <span className="small">
                        Procedure:{procedure?.code ? "" : ` ${procedure?.display ?? reference}`}
                    </span>
                }
                trailing={procedure?.code && (
                    <span className="small">
                        <CodeWithInfo
                            display={procedure.display}
                            code={procedure.code}
                            system={procedure.system} />
                    </span>
                )}
                badges={<DiffCountBadge diffs={itemDiffs} />} />

            {isExpanded && procedure !== null && (
                <DiffHighlight fieldDiffs={itemDiffs} acceptance={context.acceptance}>
                    <div className="ms-3 mt-2 p-2 border rounded bg-light">
                        <Row className="g-1">
                            <Col xs={12}>
                                <small className="text-muted d-block">Display</small>
                                <div>{procedure.display ?? "N/A"}</div>
                            </Col>

                            {procedure.code !== null && (
                                <Col xs={12}>
                                    <small className="text-muted d-block">Code</small>

                                    <div>
                                        <CodeWithInfo
                                            display={procedure.display}
                                            code={procedure.code}
                                            system={procedure.system} />
                                    </div>
                                </Col>
                            )}

                            {procedure.status !== null && (
                                <Col xs={6}>
                                    <small className="text-muted d-block">Status</small>
                                    <div>{procedure.status}</div>
                                </Col>
                            )}

                            {procedure.performedDateTime !== null && (
                                <Col xs={6}>
                                    <small className="text-muted d-block">Performed</small>
                                    <div>{formatFhirDate(procedure.performedDateTime)}</div>
                                </Col>
                            )}
                        </Row>

                        <ResourceJsonToggle resource={resource} />
                    </div>
                </DiffHighlight>
            )}
        </div>
    );
}

function ProcedureRequestItem({ reference, expansionKey, context }: ReferenceOnlyProps) {
    const isExpanded = context.expansion.isExpanded(expansionKey);
    const toggleExpanded = () => context.expansion.toggleExpanded(expansionKey);

    const resource = context.resourceIndex.get(reference) ?? null;
    const procedureRequest = resource === null ? null : parseProcedureRequest(resource);

    const matchKey = resource === null ? null : buildDdsIdentifierMatchKey(resource);

    const itemDiffs = getMatchedDiffs(
        context.diffsByResourceType.get("ProcedureRequest") ?? [],
        matchKey);

    return (
        <div className="py-1 ps-2 border-bottom">
            <ExpandableRow
                expanded={isExpanded}
                onToggle={toggleExpanded}
                label={
                    <span className="small">
                        Procedure request:
                        {procedureRequest?.code ? "" : ` ${procedureRequest?.display ?? reference}`}
                    </span>
                }
                trailing={procedureRequest?.code && (
                    <span className="small">
                        <CodeWithInfo
                            display={procedureRequest.display}
                            code={procedureRequest.code}
                            system={procedureRequest.system} />
                    </span>
                )}
                badges={<DiffCountBadge diffs={itemDiffs} />} />

            {isExpanded && procedureRequest !== null && (
                <DiffHighlight fieldDiffs={itemDiffs} acceptance={context.acceptance}>
                    <div className="ms-3 mt-2 p-2 border rounded bg-light">
                        <Row className="g-1">
                            <Col xs={12}>
                                <small className="text-muted d-block">Display</small>
                                <div>{procedureRequest.display ?? "N/A"}</div>
                            </Col>

                            {procedureRequest.code !== null && (
                                <Col xs={12}>
                                    <small className="text-muted d-block">Code</small>

                                    <div>
                                        <CodeWithInfo
                                            display={procedureRequest.display}
                                            code={procedureRequest.code}
                                            system={procedureRequest.system} />
                                    </div>
                                </Col>
                            )}

                            {procedureRequest.status !== null && (
                                <Col xs={6}>
                                    <small className="text-muted d-block">Status</small>
                                    <div>{procedureRequest.status}</div>
                                </Col>
                            )}

                            {procedureRequest.authoredOn !== null && (
                                <Col xs={6}>
                                    <small className="text-muted d-block">Authored on</small>
                                    <div>{formatFhirDate(procedureRequest.authoredOn)}</div>
                                </Col>
                            )}
                        </Row>

                        <ResourceJsonToggle resource={resource} />
                    </div>
                </DiffHighlight>
            )}
        </div>
    );
}

function ReferralRequestItem({ reference, expansionKey, context }: ReferenceOnlyProps) {
    const isExpanded = context.expansion.isExpanded(expansionKey);
    const toggleExpanded = () => context.expansion.toggleExpanded(expansionKey);

    const resource = context.resourceIndex.get(reference) ?? null;
    const referralRequest = resource === null ? null : parseReferralRequest(resource);

    const matchKey = resource === null ? null : buildDdsIdentifierMatchKey(resource);

    const itemDiffs = getMatchedDiffs(
        context.diffsByResourceType.get("ReferralRequest") ?? [],
        matchKey);

    return (
        <div className="py-1 ps-2 border-bottom">
            <ExpandableRow
                expanded={isExpanded}
                onToggle={toggleExpanded}
                label={
                    <span className="small">
                        Referral request:
                        {referralRequest?.code ? "" : ` ${referralRequest?.display ?? reference}`}
                    </span>
                }
                trailing={referralRequest?.code && (
                    <span className="small">
                        <CodeWithInfo
                            display={referralRequest.display}
                            code={referralRequest.code}
                            system={referralRequest.system} />
                    </span>
                )}
                badges={<DiffCountBadge diffs={itemDiffs} />} />

            {isExpanded && referralRequest !== null && (
                <DiffHighlight fieldDiffs={itemDiffs} acceptance={context.acceptance}>
                    <div className="ms-3 mt-2 p-2 border rounded bg-light">
                        <Row className="g-1">
                            <Col xs={12}>
                                <small className="text-muted d-block">Display</small>
                                <div>{referralRequest.display ?? "N/A"}</div>
                            </Col>

                            {referralRequest.code !== null && (
                                <Col xs={12}>
                                    <small className="text-muted d-block">Code</small>

                                    <div>
                                        <CodeWithInfo
                                            display={referralRequest.display}
                                            code={referralRequest.code}
                                            system={referralRequest.system} />
                                    </div>
                                </Col>
                            )}

                            {referralRequest.status !== null && (
                                <Col xs={6}>
                                    <small className="text-muted d-block">Status</small>
                                    <div>{referralRequest.status}</div>
                                </Col>
                            )}

                            {referralRequest.authoredOn !== null && (
                                <Col xs={6}>
                                    <small className="text-muted d-block">Authored on</small>
                                    <div>{formatFhirDate(referralRequest.authoredOn)}</div>
                                </Col>
                            )}
                        </Row>

                        <ResourceJsonToggle resource={resource} />
                    </div>
                </DiffHighlight>
            )}
        </div>
    );
}

// RelatedPerson has no matcher registered server side, so it never carries a diff badge or an
// accept checkbox - it still gets a parsed view rather than falling back to raw JSON.
function RelatedPersonItem({ reference, expansionKey, context }: ReferenceOnlyProps) {
    const isExpanded = context.expansion.isExpanded(expansionKey);
    const toggleExpanded = () => context.expansion.toggleExpanded(expansionKey);

    const resource = context.resourceIndex.get(reference) ?? null;
    const relatedPerson = resource === null ? null : parseRelatedPerson(resource);

    return (
        <div className="py-1 ps-2 border-bottom">
            <ExpandableRow
                expanded={isExpanded}
                onToggle={toggleExpanded}
                label={
                    <span className="small">
                        Related person:
                        {relatedPerson?.relationshipCode
                            ? ""
                            : ` ${relatedPerson?.name ?? reference}`}
                    </span>
                }
                trailing={relatedPerson?.relationshipCode && (
                    <span className="small">
                        <CodeWithInfo
                            display={relatedPerson.relationshipDisplay}
                            code={relatedPerson.relationshipCode}
                            system={relatedPerson.relationshipSystem} />
                    </span>
                )} />

            {isExpanded && relatedPerson !== null && (
                <div className="ms-3 mt-2 p-2 border rounded bg-light">
                    <Row className="g-1">
                        <Col xs={12}>
                            <small className="text-muted d-block">Name</small>
                            <div>{relatedPerson.name ?? "N/A"}</div>
                        </Col>

                        {relatedPerson.relationshipCode !== null && (
                            <Col xs={12}>
                                <small className="text-muted d-block">Relationship</small>

                                <div>
                                    <CodeWithInfo
                                        display={relatedPerson.relationshipDisplay}
                                        code={relatedPerson.relationshipCode}
                                        system={relatedPerson.relationshipSystem} />
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

function AppointmentItem({ reference, expansionKey, context }: ReferenceOnlyProps) {
    const isExpanded = context.expansion.isExpanded(expansionKey);
    const toggleExpanded = () => context.expansion.toggleExpanded(expansionKey);

    const resource = context.resourceIndex.get(reference) ?? null;
    const appointment = resource === null ? null : parseAppointment(resource);

    const matchKey = resource === null ? null : buildDdsIdentifierMatchKey(resource);
    const itemDiffs = getMatchedDiffs(context.diffsByResourceType.get("Appointment") ?? [], matchKey);

    return (
        <div className="py-1 ps-2 border-bottom">
            <ExpandableRow
                expanded={isExpanded}
                onToggle={toggleExpanded}
                label={
                    <span className="small">
                        Appointment: {appointment?.description ?? reference}
                    </span>
                }
                badges={<DiffCountBadge diffs={itemDiffs} />} />

            {isExpanded && appointment !== null && (
                <DiffHighlight fieldDiffs={itemDiffs} acceptance={context.acceptance}>
                    <div className="ms-3 mt-2 p-2 border rounded bg-light">
                        <Row className="g-1">
                            <Col xs={12}>
                                <small className="text-muted d-block">Description</small>
                                <div>{appointment.description ?? "N/A"}</div>
                            </Col>

                            {appointment.status !== null && (
                                <Col xs={6}>
                                    <small className="text-muted d-block">Status</small>
                                    <div>{appointment.status}</div>
                                </Col>
                            )}

                            {appointment.start !== null && (
                                <Col xs={6}>
                                    <small className="text-muted d-block">Start</small>
                                    <div>{formatFhirDate(appointment.start)}</div>
                                </Col>
                            )}

                            {appointment.end !== null && (
                                <Col xs={6}>
                                    <small className="text-muted d-block">End</small>
                                    <div>{formatFhirDate(appointment.end)}</div>
                                </Col>
                            )}
                        </Row>

                        <ResourceJsonToggle resource={resource} />
                    </div>
                </DiffHighlight>
            )}
        </div>
    );
}

// Whatever the tree has no dedicated view for. It still opens to its JSON, so an unmodelled
// resource type is inspectable rather than invisible.
function GenericItem({ reference, expansionKey, context }: ReferenceOnlyProps) {
    const isExpanded = context.expansion.isExpanded(expansionKey);
    const toggleExpanded = () => context.expansion.toggleExpanded(expansionKey);

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
