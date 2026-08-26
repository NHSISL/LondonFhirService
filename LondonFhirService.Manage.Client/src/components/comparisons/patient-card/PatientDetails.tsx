import { useState } from "react";
import { Button, Col, Form, Row } from "react-bootstrap";
import { DiffHighlight } from "./DiffHighlight";
import { ExpandableRow } from "./ExpandableRow";
import { OrganizationSection, PractitionerSection } from "./resourceSections";
import { formatPatientAddress } from "./patientFormatters";
import type { DiffItemView } from "../../../models/views/comparisons/DiffItemView";
import type { PatientData } from "../../../models/foundations/fhir/PatientData";
import type { ResourceTreeContext } from "./resourceSections";

type PatientDetailsProps = {
    patient: PatientData;
    showPatientDetails: boolean;
    onShowPatientDetails: (showPatientDetails: boolean) => void;
    getFieldDiffs: (field: string) => DiffItemView[];
    context: ResourceTreeContext;
};

export function PatientDetails({
    patient,
    showPatientDetails,
    onShowPatientDetails,
    getFieldDiffs,
    context
}: PatientDetailsProps) {
    const [showAddressComponents, setShowAddressComponents] = useState<boolean>(false);
    const [showPatientJson, setShowPatientJson] = useState<boolean>(false);

    // Every field renders the same way: its value, outlined and tickable when the comparison found
    // a difference in it, plain when it did not.
    const field = (fieldName: string, value: string | null, className?: string) => (
        <DiffHighlight fieldDiffs={getFieldDiffs(fieldName)} acceptance={context.acceptance}>
            <div className={className}>{value ?? "N/A"}</div>
        </DiffHighlight>
    );

    return (
        <>
            <div className="mb-3 p-2 border rounded">
                <ExpandableRow
                    expanded={showPatientDetails}
                    onToggle={() => onShowPatientDetails(showPatientDetails === false)}
                    label={<strong>Patient details</strong>} />
            </div>

            {showPatientDetails && (
                <div className="p-3 border rounded mb-3 bg-light">
                    <Form.Group className="mb-3">
                        <Form.Label className="text-muted small mb-1" as="div">
                            <ExpandableRow
                                expanded={showAddressComponents}
                                onToggle={() =>
                                    setShowAddressComponents(
                                        currentValue => currentValue === false)}
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

                            <DiffHighlight
                                fieldDiffs={getFieldDiffs("managingOrganizationRef")}
                                acceptance={context.acceptance}>
                                <OrganizationSection
                                    reference={patient.managingOrganizationRef}
                                    context={context} />
                            </DiffHighlight>
                        </Form.Group>
                    )}

                    {patient.generalPractitionerRefs.length > 0 && (
                        <Form.Group className="mb-3">
                            <Form.Label className="text-muted small mb-1">
                                General practitioner
                            </Form.Label>

                            {patient.generalPractitionerRefs.map(generalPractitionerRef => (
                                <PractitionerSection
                                    key={generalPractitionerRef}
                                    reference={generalPractitionerRef}
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
        </>
    );
}
