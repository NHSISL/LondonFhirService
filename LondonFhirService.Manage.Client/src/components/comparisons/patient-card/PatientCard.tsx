import { useMemo } from "react";
import { Card, Form } from "react-bootstrap";
import { EpisodeOfCareList } from "./EpisodeOfCareList";
import { ListsSection } from "./resourceSections";
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
import type { PatientCardProps } from "../../../models/components/comparisons/PatientCardProps";
import type { ResourceTreeContext } from "./resourceSections";

// One side of a comparison, rendered from an already parsed bundle. The differences it is handed
// are the whole comparison's; each section picks out the ones for its own resource type, so a
// section only outlines what actually differs in it.
export function PatientCard({
    source,
    diffs,
    acceptance,
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
            setExpandedItems: setExpandedItems,
            acceptance: acceptance
        }),
        [resourceIndex, expandedItems, setExpandedItems, acceptance]);

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
                    showPatientDetails={showPatientDetails}
                    onShowPatientDetails={onShowPatientDetails}
                    getFieldDiffs={getFieldDiffs}
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

                <OtherDiffList otherDiffs={otherDiffs} acceptance={acceptance} />
            </Card.Body>
        </Card>
    );
}
