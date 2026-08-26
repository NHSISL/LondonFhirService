import { useState } from "react";
import { Button, Col, Form, Row } from "react-bootstrap";
import { ExpandableRow } from "./ExpandableRow";
import { OrganizationSection, PractitionerSection } from "./resourceSections";
import { formatPatientAddress } from "./patientFormatters";
import type { CSSProperties } from "react";
import type { PatientData } from "../../../models/foundations/fhir/PatientData";
import type { ResourceTreeContext } from "./resourceSections";

type PatientDetailsProps = {
    patient: PatientData;
    showPatientDetails: boolean;
    onShowPatientDetails: (showPatientDetails: boolean) => void;
    getHighlightStyleForField: (field: string) => CSSProperties;
    context: ResourceTreeContext;
};

export function PatientDetails({
    patient,
    showPatientDetails,
    onShowPatientDetails,
    getHighlightStyleForField,
    context
}: PatientDetailsProps) {
    const [showAddressComponents, setShowAddressComponents] = useState<boolean>(false);
    const [showPatientJson, setShowPatientJson] = useState<boolean>(false);

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

                        <div style={getHighlightStyleForField("addressLine")}>
                            {formatPatientAddress(patient) || "N/A"}
                        </div>

                        {showAddressComponents && (
                            <Row className="g-1 mt-2">
                                <Col xs={12}>
                                    <small className="text-muted d-block">Line</small>

                                    <div style={getHighlightStyleForField("addressLine")}>
                                        {patient.addressLine ?? "N/A"}
                                    </div>
                                </Col>

                                <Col xs={6}>
                                    <small className="text-muted d-block">City</small>

                                    <div style={getHighlightStyleForField("addressCity")}>
                                        {patient.addressCity ?? "N/A"}
                                    </div>
                                </Col>

                                <Col xs={6}>
                                    <small className="text-muted d-block">District</small>

                                    <div style={getHighlightStyleForField("addressDistrict")}>
                                        {patient.addressDistrict ?? "N/A"}
                                    </div>
                                </Col>

                                <Col xs={6}>
                                    <small className="text-muted d-block">Postcode</small>

                                    <div style={getHighlightStyleForField("addressPostalCode")}>
                                        {patient.addressPostalCode ?? "N/A"}
                                    </div>
                                </Col>

                                <Col xs={6}>
                                    <small className="text-muted d-block">Country</small>

                                    <div style={getHighlightStyleForField("addressCountry")}>
                                        {patient.addressCountry ?? "N/A"}
                                    </div>
                                </Col>
                            </Row>
                        )}
                    </Form.Group>

                    <Form.Group className="mb-3">
                        <Form.Label className="text-muted small mb-1">Birth date</Form.Label>

                        <div style={getHighlightStyleForField("birthDate")}>
                            {patient.birthDate ?? "N/A"}
                        </div>
                    </Form.Group>

                    <Form.Group className="mb-3">
                        <Form.Label className="text-muted small mb-1">Gender</Form.Label>

                        <div
                            className="text-capitalize"
                            style={getHighlightStyleForField("gender")}>
                            {patient.gender ?? "N/A"}
                        </div>
                    </Form.Group>

                    <Form.Group className="mb-3">
                        <Form.Label className="text-muted small mb-1">Telecom</Form.Label>

                        <div style={getHighlightStyleForField("telecom")}>
                            {patient.telecom ?? "N/A"}
                        </div>
                    </Form.Group>

                    <Form.Group className="mb-3">
                        <Form.Label className="text-muted small mb-1">Communication</Form.Label>

                        <div style={getHighlightStyleForField("communication")}>
                            {patient.communication ?? "N/A"}
                        </div>
                    </Form.Group>

                    {patient.managingOrganizationRef !== null && (
                        <Form.Group className="mb-3">
                            <Form.Label className="text-muted small mb-1">
                                Managing organisation
                            </Form.Label>

                            <div style={getHighlightStyleForField("managingOrganizationRef")}>
                                <OrganizationSection
                                    reference={patient.managingOrganizationRef}
                                    context={context} />
                            </div>
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
