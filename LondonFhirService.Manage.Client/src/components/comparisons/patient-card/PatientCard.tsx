import { useMemo } from "react";
import { Card, Form } from "react-bootstrap";
import { EpisodeOfCareList } from "./EpisodeOfCareList";
import { ListsSection, UnlistedResourceSection } from "./resourceSections";
import { OtherDiffList } from "./OtherDiffList";
import { PatientDetails } from "./PatientDetails";
import { PatientHeader } from "./PatientHeader";
import { formatPatientName } from "./patientFormatters";
import {
    getDiffsForField,
    getDiffsForFields,
    getDiffState,
    getHighlightStyle,
    getOtherDiffs
} from "../../../helpers/comparisons/diffHighlighting";
import {
    dedicatedSectionResourceTypes,
    findAllReferences,
    findUnlistedReferences
} from "../../../helpers/fhir/unlistedResources";
import type { PatientCardProps } from "../../../models/components/comparisons/PatientCardProps";
import type { ResourceTreeContext } from "./resourceSections";

// Resource types a provider's own Lists cannot be relied on to mention or to mention under a
// name that says what they are - a feed can bundle Immunizations without ever pointing a List
// at them, or fold Observations into a list titled "Miscellaneous record". A type in
// dedicatedSectionResourceTypes always gets a section of its own covering every resource of that
// type in the bundle; everything else here only picks up what no List already shows, since a
// provider's own list for it already reads as a dedicated section (typically "Problems",
// "Active Allergies", "Medication List").
const unlistedSections: Array<{ title: string; resourceType: string }> = [
    { title: "Observations", resourceType: "Observation" },
    { title: "Conditions", resourceType: "Condition" },
    { title: "Allergies", resourceType: "AllergyIntolerance" },
    { title: "Medications", resourceType: "MedicationStatement" },
    { title: "Medication resources", resourceType: "Medication" },
    { title: "Medication requests", resourceType: "MedicationRequest" },
    { title: "Diagnostic reports", resourceType: "DiagnosticReport" },
    { title: "Procedures", resourceType: "Procedure" },
    { title: "Procedure requests", resourceType: "ProcedureRequest" },
    { title: "Referral requests", resourceType: "ReferralRequest" },
    { title: "Immunizations", resourceType: "Immunization" },
    { title: "Encounters", resourceType: "Encounter" },
    { title: "Appointments", resourceType: "Appointment" },
    { title: "Locations", resourceType: "Location" },
    { title: "Family history", resourceType: "FamilyMemberHistory" },
    { title: "Related persons", resourceType: "RelatedPerson" }
];

// One side of a comparison, rendered from an already parsed bundle. The differences it is handed
// are the whole comparison's; each section picks out the ones for its own resource type, so a
// section only outlines what actually differs in it.
export function PatientCard({
    source,
    diffs,
    acceptance,
    expansion
}: PatientCardProps) {
    const { patient, resourceIndex, lists, episodesOfCare } = source.bundle;

    const diffsByResourceType = useMemo(() => {
        const grouped = new Map<string, typeof diffs>();

        for (const diff of diffs) {
            const resourceType = diff.resourceTypeText ?? "";
            const existing = grouped.get(resourceType);

            if (existing === undefined) {
                grouped.set(resourceType, [diff]);
            } else {
                existing.push(diff);
            }
        }

        return grouped;
    }, [diffs]);

    const context = useMemo<ResourceTreeContext>(
        () => ({
            resourceIndex: resourceIndex,
            expansion: expansion,
            acceptance: acceptance,
            diffsByResourceType: diffsByResourceType
        }),
        [resourceIndex, expansion, acceptance, diffsByResourceType]);

    const unlistedReferencesByType = useMemo(() => {
        const grouped = new Map<string, string[]>();

        for (const section of unlistedSections) {
            const references = dedicatedSectionResourceTypes.includes(section.resourceType)
                ? findAllReferences(resourceIndex, section.resourceType)
                : findUnlistedReferences(resourceIndex, lists, section.resourceType);

            grouped.set(section.resourceType, references);
        }

        return grouped;
    }, [resourceIndex, lists]);

    const hasUnlistedResources = [...unlistedReferencesByType.values()]
        .some(references => references.length > 0);

    const patientDiffs = diffsByResourceType.get("Patient") ?? [];

    const getFieldDiffs = (field: string) =>
        getDiffsForField(patientDiffs, field, acceptance.side);

    // The header is a two line block rather than a field box, so it takes the outline alone -
    // its differences are ticked from the Patient details panel or the differences list.
    const getHeaderStyleForFields = (fields: string[]) =>
        getHighlightStyle(getDiffState(getDiffsForFields(patientDiffs, fields, acceptance.side)));

    const otherDiffs = getOtherDiffs(diffs, acceptance.side);

    return (
        <Card className="h-100 border-0">
            <PatientHeader
                name={formatPatientName(patient) || "Unknown patient"}
                nhsNumber={patient.nhsNumber}
                sourceName={source.sourceName}
                roleText={source.roleText}
                roleClassName={source.roleClassName}
                formattedJsonPayload={source.formattedJsonPayload}
                nameStyle={getHeaderStyleForFields(
                    ["nameFamily", "nameGiven", "namePrefix", "nameSuffix"])}
                nhsNumberStyle={getHeaderStyleForFields(["nhsNumber"])} />

            <Card.Body>
                <PatientDetails
                    patient={patient}
                    getFieldDiffs={getFieldDiffs}
                    context={context} />

                <EpisodeOfCareList
                    episodesOfCare={episodesOfCare}
                    diffs={diffsByResourceType.get("EpisodeOfCare") ?? []}
                    context={context} />

                {(lists.length > 0 || hasUnlistedResources) && (
                    <Form.Group className="mb-3">
                        <Form.Label className="text-muted small mb-1">Clinical lists</Form.Label>

                        <ListsSection
                            lists={lists}
                            listDiffs={diffsByResourceType.get("List") ?? []}
                            context={context} />

                        {unlistedSections.map(section => (
                            <UnlistedResourceSection
                                key={section.resourceType}
                                title={section.title}
                                references={unlistedReferencesByType.get(section.resourceType) ?? []}
                                context={context} />
                        ))}
                    </Form.Group>
                )}

                <OtherDiffList otherDiffs={otherDiffs} acceptance={acceptance} />
            </Card.Body>
        </Card>
    );
}
