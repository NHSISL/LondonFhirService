import { Button, Card, Col, Form, Row } from "react-bootstrap";
import TextInputBase from "../bases/inputs/TextInputBase";
import ToggleBase from "../bases/inputs/ToggleBase";
import type { StructuredRecordFormProps } from "../../models/components/patients/StructuredRecordFormProps";

export function StructuredRecordForm({
    values,
    errors,
    submitting,
    onFieldChange,
    onSubmit,
    onClear
}: StructuredRecordFormProps) {
    return (
        <Form
            noValidate
            onSubmit={event => {
                event.preventDefault();
                onSubmit();
            }}>

            <Card className="mb-3">
                <Card.Header as="h2" className="h6 mb-0">Consumer credentials</Card.Header>
                <Card.Body>
                    <p className="text-muted small">
                        Leave a field blank to use the credential this environment is configured
                        with. Fill one in to call as that consumer instead.
                    </p>

                    {/*
                        autoComplete is off on both credential fields, and "new-password" rather
                        than "off" on the secret because Chrome ignores "off" for a password input.
                        These are a consumer's credentials being tested, not the viewer's own, so a
                        browser that saves and later autofills them puts consumer A's secret behind
                        consumer B's client id - producing a 401 nobody can explain, on the one
                        screen whose whole purpose is proving which credentials work.
                    */}

                    <Row className="mb-3">
                        <Col md={6}>
                            <TextInputBase
                                id="clientId"
                                name="clientId"
                                label="Client id"
                                description="Optional. Blank uses the configured client id."
                                autoComplete="off"
                                value={values.clientId}
                                disabled={submitting}
                                onChange={event =>
                                    onFieldChange("clientId", event.target.value)} />
                        </Col>

                        <Col md={6}>
                            <TextInputBase
                                id="clientSecret"
                                name="clientSecret"
                                label="Client secret"
                                type="password"
                                description="Optional. Blank uses the configured client secret."
                                autoComplete="new-password"
                                value={values.clientSecret}
                                disabled={submitting}
                                onChange={event =>
                                    onFieldChange("clientSecret", event.target.value)} />
                        </Col>
                    </Row>

                    <Row className="mb-3">
                        <Col md={6}>
                            <TextInputBase
                                id="scope"
                                name="scope"
                                label="Scope"
                                description="Optional. Blank uses the configured scope."
                                value={values.scope}
                                disabled={submitting}
                                onChange={event => onFieldChange("scope", event.target.value)} />
                        </Col>

                        <Col md={6}>
                            <TextInputBase
                                id="grantType"
                                name="grantType"
                                label="Grant type"
                                placeholder="client_credentials"
                                description="Optional. Blank uses the configured grant type."
                                value={values.grantType}
                                disabled={submitting}
                                onChange={event =>
                                    onFieldChange("grantType", event.target.value)} />
                        </Col>
                    </Row>
                </Card.Body>
            </Card>

            <Card className="mb-3">
                <Card.Header as="h2" className="h6 mb-0">Patient</Card.Header>
                <Card.Body>
                    <Row className="mb-3">
                        <Col md={6}>
                            <TextInputBase
                                id="nhsNumber"
                                name="nhsNumber"
                                label="NHS number"
                                placeholder="9435797881"
                                description="Required."
                                required
                                maxLength={50}
                                value={values.nhsNumber}
                                error={errors.nhsNumber}
                                disabled={submitting}
                                onChange={event =>
                                    onFieldChange("nhsNumber", event.target.value)} />
                            {errors.nhsNumber && (
                                <small className="text-danger">{errors.nhsNumber}</small>
                            )}
                        </Col>

                        <Col md={6}>
                            <TextInputBase
                                id="dateOfBirth"
                                name="dateOfBirth"
                                label="Patient date of birth"
                                placeholder="1994-05-21"
                                description="Optional. Format YYYY-MM-DD."
                                maxLength={10}
                                value={values.dateOfBirth}
                                disabled={submitting}
                                onChange={event =>
                                    onFieldChange("dateOfBirth", event.target.value)} />
                        </Col>
                    </Row>

                    <Row className="mb-3">
                        <Col md={6}>
                            <ToggleBase
                                id="demographicsOnly"
                                name="demographicsOnly"
                                label="Demographics only"
                                description="On returns the patient's demographics without the clinical record."
                                checked={values.demographicsOnly}
                                disabled={submitting}
                                onChange={event =>
                                    onFieldChange("demographicsOnly", event.target.checked)} />
                        </Col>
                    </Row>
                </Card.Body>
            </Card>

            <div className="d-flex gap-2">
                <Button type="submit" variant="primary" disabled={submitting}>
                    {submitting ? "Retrieving..." : "Get structured record"}
                </Button>

                <Button
                    type="button"
                    variant="outline-secondary"
                    onClick={onClear}
                    disabled={submitting}>
                    Clear
                </Button>
            </div>
        </Form>
    );
}
