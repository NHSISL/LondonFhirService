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

            {/*
                Side by side from xl, stacked below it. The fields inside each panel are paired at
                md, so a panel narrower than about half a 1200px viewport puts two inputs and their
                descriptions into a space neither reads in - which is why this waits for xl rather
                than starting at lg.

                h-100 on the cards rather than a margin, so the two panels square off against each
                other instead of ending at whatever height their own contents reach.
            */}
            <Row>
                <Col xl={6} className="mb-3">
                    <Card className="h-100">
                        <Card.Header as="h2" className="h6 mb-0">
                            Consumer credentials
                        </Card.Header>
                        <Card.Body>
                            <p className="text-muted small">
                                Leave a field blank to use the credential this environment
                                is configured with. Fill one in to call as that consumer
                                instead.
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
                                        error={errors.clientId}
                                        disabled={submitting}
                                        onChange={event =>
                                            onFieldChange("clientId", event.target.value)} />
                                        {errors.clientId && (
                                            <small className="text-danger">{errors.clientId}</small>
                                        )}
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
                                        error={errors.clientSecret}
                                        disabled={submitting}
                                        onChange={event =>
                                            onFieldChange("clientSecret", event.target.value)} />
                                        {errors.clientSecret && (
                                            <small className="text-danger">{errors.clientSecret}</small>
                                        )}
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
                                        error={errors.scope}
                                        disabled={submitting}
                                        onChange={event => onFieldChange("scope", event.target.value)} />
{errors.scope && (
    <small className="text-danger">{errors.scope}</small>
)}
                                </Col>

                                <Col md={6}>
                                    <TextInputBase
                                        id="grantType"
                                        name="grantType"
                                        label="Grant type"
                                        placeholder="client_credentials"
                                        description="Optional. Blank uses the configured grant type."
                                        value={values.grantType}
                                        error={errors.grantType}
                                        disabled={submitting}
                                        onChange={event =>
                                            onFieldChange("grantType", event.target.value)} />
                                        {errors.grantType && (
                                            <small className="text-danger">{errors.grantType}</small>
                                        )}
                                </Col>
                            </Row>
                        </Card.Body>
                    </Card>
                </Col>

                <Col xl={6} className="mb-3">
                    <Card className="h-100">
                        <Card.Header as="h2" className="h6 mb-0">Patient</Card.Header>
                        <Card.Body>
                            <Row className="mb-3">
                                <Col md={6}>
                                    <TextInputBase
                                        id="nhsNumber"
                                        name="nhsNumber"
                                        label="NHS number"
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
                                        description="Optional. Format YYYY-MM-DD."
                                        maxLength={10}
                                        value={values.dateOfBirth}
                                        error={errors.dateOfBirth}
                                        disabled={submitting}
                                        onChange={event =>
                                            onFieldChange("dateOfBirth", event.target.value)} />
                                        {errors.dateOfBirth && (
                                            <small className="text-danger">{errors.dateOfBirth}</small>
                                        )}
                                </Col>
                            </Row>

                            {/*
                                Full width rather than half. There is no second control to pair
                                this with, and in a panel that is now half the page a half of a
                                half wraps its description onto four lines.
                            */}
                            <Row className="mb-3">
                                <Col>
                                    <ToggleBase
                                        id="demographicsOnly"
                                        name="demographicsOnly"
                                        label="Demographics only"
                                        description={"On returns the patient's demographics "
                                            + "without the clinical record."}
                                        checked={values.demographicsOnly}
                                        disabled={submitting}
                                        onChange={event =>
                                            onFieldChange("demographicsOnly", event.target.checked)} />
                                </Col>
                            </Row>
                        </Card.Body>
                    </Card>
                </Col>
            </Row>

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
