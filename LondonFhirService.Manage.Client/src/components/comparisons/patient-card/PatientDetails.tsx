import { useState } from "react";
import { Button, Col, Form, Row } from "react-bootstrap";
import { DiffCountBadge, OrganizationSection, PractitionerSection } from "./resourceSections";
import { DiffHighlight } from "./DiffHighlight";
import { ExpandableRow } from "./ExpandableRow";
import { expansionKeys } from "./expansionKeys";
import { formatPatientAddress } from "./patientFormatters";
import type { DiffItemView } from "../../../models/views/comparisons/DiffItemView";
import type { PatientData } from "../../../models/foundations/fhir/PatientData";
import type { ResourceTreeContext } from "./resourceSections";

type PatientDetailsProps = {
    patient: PatientData;
    getFieldDiffs: (field: string) => DiffItemView[];
    context: ResourceTreeContext;
};

// Every field this panel lays out, excluding the ones the header renders instead (name, NHS
// number) - added up for a badge that shows this panel has something outstanding before anyone
// opens it, the same way a collapsed clinical list does.
const detailsFields = [
    "addressLine",
    "addressCity",
    "addressDistrict",
    "addressPostalCode",
    "addressCountry",
    "birthDate",
    "gender",
    "telecom",
    "communication",
    "managingOrganizationRef",
    "generalPractitionerRefs"
];

export function PatientDetails({
    patient,
    getFieldDiffs,
    context
}: PatientDetailsProps) {
    const showPatientDetails = context.expansion.isExpanded(expansionKeys.patientDetails);

    const showAddressComponents =
        context.expansion.isExpanded(expansionKeys.patientAddress);

    // The raw JSON is per card by nature - the two payloads are different documents - so it stays
    // local rather than joining the sections the two sides keep in step.
    const [showPatientJson, setShowPatientJson] = useState<boolean>(false);

    const detailsDiffs = detailsFields.flatMap(fieldName => getFieldDiffs(fieldName));

    // Every field renders the same way: its value, outlined and tickable when the comparison found
    // a difference in it, plain when it did not.
    const field = (fieldName: string, value: string | null, className?: string) => (
        <DiffHighlight fieldDiffs={getFieldDiffs(fieldName)} acceptance={context.acceptance}>
            <div className={className}>{value ?? "N/A"}</div>
        </DiffHighlight>
    );

    return (
        <div className="mb-3 p-2 border rounded">
            <ExpandableRow
                expanded={showPatientDetails}
                onToggle={() =>
                    context.expansion.toggleExpanded(expansionKeys.patientDetails)}
                label={<strong>Patient details</strong>}
                badges={<DiffCountBadge diffs={detailsDiffs} />} />

            {showPatientDetails && (
                <div className="mt-2 pt-2 border-top">
                    <Form.Group className="mb-3">
                        <Form.Label className="text-muted small mb-1" as="div">
                            <ExpandableRow
                                expanded={showAddressComponents}
                                onToggle={() =>
                                    context.expansion.toggleExpanded(
                                        expansionKeys.patientAddress)}
                                label={<span>Address</span>} />
                        </Form.Label>

                        {field("addressLine", formatPatientAddress(patient) || null)}

                        {showAddressComponents && (
                            <Row className="g-1 mt-2">
                                <Col xs={12}>
                                    <small className="text-muted d-block">Line</small>
                                    {field("addressLine", patient.addressLine)}
                                </Col>

                                <Col xs={6}>
                                    <small className="text-muted d-block">City</small>
                                    {field("addressCity", patient.addressCity)}
                                </Col>

                                <Col xs={6}>
                                    <small className="text-muted d-block">District</small>
                                    {field("addressDistrict", patient.addressDistrict)}
                                </Col>

                                <Col xs={6}>
                                    <small className="text-muted d-block">Postcode</small>
                                    {field("addressPostalCode", patient.addressPostalCode)}
                                </Col>

                                <Col xs={6}>
                                    <small className="text-muted d-block">Country</small>
                                    {field("addressCountry", patient.addressCountry)}
                                </Col>
                            </Row>
                        )}
                    </Form.Group>

                    <Form.Group className="mb-3">
                        <Form.Label className="text-muted small mb-1">Birth date</Form.Label>
                        {field("birthDate", patient.birthDate)}
                    </Form.Group>

                    <Form.Group className="mb-3">
                        <Form.Label className="text-muted small mb-1">Gender</Form.Label>
                        {field("gender", patient.gender, "text-capitalize")}
                    </Form.Group>

                    <Form.Group className="mb-3">
                        <Form.Label className="text-muted small mb-1">Telecom</Form.Label>
                        {field("telecom", patient.telecom)}
                    </Form.Group>

                    <Form.Group className="mb-3">
                        <Form.Label className="text-muted small mb-1">Communication</Form.Label>
                        {field("communication", patient.communication)}
                    </Form.Group>

                    {patient.managingOrganizationRef !== null && (
                        <Form.Group className="mb-3">
                            <Form.Label className="text-muted small mb-1">
                                Managing organisation
                            </Form.Label>

                            <OrganizationSection
                                reference={patient.managingOrganizationRef}
                                expansionKey={expansionKeys.managingOrganization}
                                fieldDiffs={getFieldDiffs("managingOrganizationRef")}
                                context={context} />
                        </Form.Group>
                    )}

                    {patient.generalPractitionerRefs.length > 0 && (
                        <Form.Group className="mb-3">
                            <Form.Label className="text-muted small mb-1">
                                General practitioner
                            </Form.Label>

                            {patient.generalPractitionerRefs.map(
                                (generalPractitionerRef, index) => (
                                    <PractitionerSection
                                        key={generalPractitionerRef}
                                        reference={generalPractitionerRef}
                                        expansionKey={
                                            expansionKeys.generalPractitioner(index)}
                                        fieldDiffs={getFieldDiffs("generalPractitionerRefs")}
                                        context={context} />
                                ))}
                        </Form.Group>
                    )}

                    <Form.Group className="mb-3">
                        <Button
                            variant="link"
                            size="sm"
                            className="p-0"
                            onClick={() =>
                                setShowPatientJson(currentValue => currentValue === false)}>
                            {showPatientJson ? "Hide patient JSON" : "Show patient JSON"}
                        </Button>

                        {showPatientJson && (
                            <pre
                                className="bg-light p-2 mt-2 small"
                                style={{
                                    maxHeight: "200px",
                                    overflow: "auto",
                                    whiteSpace: "pre-wrap",
                                    wordBreak: "break-word"
                                }}>
                                {JSON.stringify(patient.resource, null, 2)}
                            </pre>
                        )}
                    </Form.Group>
                </div>
            )}
        </div>
    );
}
