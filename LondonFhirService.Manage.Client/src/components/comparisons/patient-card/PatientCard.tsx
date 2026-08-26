import { useMemo } from "react";
import { Card, Form } from "react-bootstrap";
import { EpisodeOfCareList } from "./EpisodeOfCareList";
import { ListsSection } from "./resourceSections";
import { PatientDetails } from "./PatientDetails";
import { PatientHeader } from "./PatientHeader";
import { formatPatientName } from "./patientFormatters";
import { getDiffTypeForField, getHighlightStyle } from "../../../helpers/comparisons/diffHighlighting";
import type { PatientCardProps } from "../../../models/components/comparisons/PatientCardProps";
import type { ResourceTreeContext } from "./resourceSections";

// One side of a comparison, rendered from an already parsed bundle. The differences it is handed
// are the whole comparison's; each section picks out the ones for its own resource type, so a
// section only outlines what actually differs in it.
export function PatientCard({
    source,
    side,
    diffs,
    showPatientDetails,
    onShowPatientDetails,
    expandedLists,
    setExpandedLists,
    expandedItems,
    setExpandedItems
}: PatientCardProps) {
    const { patient, resourceIndex, lists, episodesOfCare } = source.bundle;

    const context = useMemo<ResourceTreeContext>(
        () => ({
            resourceIndex: resourceIndex,
            expandedItems: expandedItems,
            setExpandedItems: setExpandedItems
        }),
        [resourceIndex, expandedItems, setExpandedItems]);

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

    const patientDiffs = diffsByResourceType.get("Patient") ?? [];

    const getHighlightStyleForField = (field: string) =>
        getHighlightStyle(getDiffTypeForField(patientDiffs, field, side));

    return (
        <Card className="h-100 border-0">
            <PatientHeader
                name={formatPatientName(patient) || "Unknown patient"}
                nhsNumber={patient.nhsNumber}
                sourceName={source.sourceName}
                roleText={source.roleText}
                roleClassName={source.roleClassName}
                formattedJsonPayload={source.formattedJsonPayload}
                nameStyle={getHighlightStyleForField("nameFamily")}
                nhsNumberStyle={getHighlightStyleForField("nhsNumber")} />

            <Card.Body>
                <PatientDetails
                    patient={patient}
                    showPatientDetails={showPatientDetails}
                    onShowPatientDetails={onShowPatientDetails}
                    getHighlightStyleForField={getHighlightStyleForField}
                    context={context} />

                <EpisodeOfCareList
                    episodesOfCare={episodesOfCare}
                    diffs={diffsByResourceType.get("EpisodeOfCare") ?? []}
                    context={context} />

                {lists.length > 0 && (
                    <Form.Group className="mb-3">
                        <Form.Label className="text-muted small mb-1">Clinical lists</Form.Label>

                        <ListsSection
                            lists={lists}
                            listDiffs={diffsByResourceType.get("List") ?? []}
                            medicationDiffs={
                                diffsByResourceType.get("MedicationStatement") ?? []}
                            expandedLists={expandedLists}
                            setExpandedLists={setExpandedLists}
                            context={context} />
                    </Form.Group>
                )}
            </Card.Body>
        </Card>
    );
}
