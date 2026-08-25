import { Button, Col, Form, Row } from "react-bootstrap";
import CheckboxBase from "../bases/inputs/CheckboxBase";
import TextInputBase from "../bases/inputs/TextInputBase";
import type { ProviderFormProps } from "../../models/components/providers/ProviderFormProps";

export function ProviderForm({
    values,
    errors,
    saving,
    submitLabel,
    savingLabel,
    onFieldChange,
    onSubmit,
    onCancel
}: ProviderFormProps) {
    return (
        <Form
            noValidate
            onSubmit={event => {
                event.preventDefault();
                onSubmit();
            }}>
            <Row className="mb-3">
                <Col md={6}>
                    <TextInputBase
                        id="friendlyName"
                        name="friendlyName"
                        label="Friendly name"
                        placeholder="Discovery Data Service"
                        description="The name operators will recognise this provider by."
                        required
                        maxLength={255}
                        value={values.friendlyName}
                        error={errors.friendlyName}
                        onChange={event => onFieldChange("friendlyName", event.target.value)} />
                    {errors.friendlyName && (
                        <small className="text-danger">{errors.friendlyName}</small>
                    )}
                </Col>

                <Col md={6}>
                    <TextInputBase
                        id="fhirVersion"
                        name="fhirVersion"
                        label="FHIR version"
                        placeholder="STU3"
                        description="Maximum 10 characters."
                        required
                        maxLength={10}
                        value={values.fhirVersion}
                        error={errors.fhirVersion}
                        onChange={event => onFieldChange("fhirVersion", event.target.value)} />
                    {errors.fhirVersion && (
                        <small className="text-danger">{errors.fhirVersion}</small>
                    )}
                </Col>
            </Row>

            <Row className="mb-3">
                <Col>
                    <TextInputBase
                        id="fullyQualifiedName"
                        name="fullyQualifiedName"
                        label="Fully qualified name"
                        placeholder="https://provider.example.nhs.uk/STU3"
                        description="The endpoint the fan-out calls. Maximum 500 characters."
                        required
                        maxLength={500}
                        value={values.fullyQualifiedName}
                        error={errors.fullyQualifiedName}
                        onChange={event =>
                            onFieldChange("fullyQualifiedName", event.target.value)} />
                    {errors.fullyQualifiedName && (
                        <small className="text-danger">{errors.fullyQualifiedName}</small>
                    )}
                </Col>
            </Row>

            <Row className="mb-3">
                <Col md={6}>
                    <TextInputBase
                        id="activeFrom"
                        name="activeFrom"
                        label="Active from"
                        type="datetime-local"
                        description="Leave blank for no start date."
                        value={values.activeFrom}
                        onChange={event => onFieldChange("activeFrom", event.target.value)} />
                </Col>

                <Col md={6}>
                    <TextInputBase
                        id="activeTo"
                        name="activeTo"
                        label="Active to"
                        type="datetime-local"
                        description="Leave blank for no end date."
                        value={values.activeTo}
                        onChange={event => onFieldChange("activeTo", event.target.value)} />
                </Col>
            </Row>

            <Row className="mb-3">
                <Col md={4}>
                    <CheckboxBase
                        id="isActive"
                        name="isActive"
                        label="Active"
                        checked={values.isActive}
                        onChange={event => onFieldChange("isActive", event.target.checked)} />
                </Col>

                <Col md={4}>
                    <CheckboxBase
                        id="isPrimary"
                        name="isPrimary"
                        label="Primary provider"
                        checked={values.isPrimary}
                        onChange={event => onFieldChange("isPrimary", event.target.checked)} />
                </Col>

                <Col md={4}>
                    <CheckboxBase
                        id="isForComparisonOnly"
                        name="isForComparisonOnly"
                        label="Comparison only"
                        checked={values.isForComparisonOnly}
                        onChange={event =>
                            onFieldChange("isForComparisonOnly", event.target.checked)} />
                </Col>
            </Row>

            <div className="d-flex gap-2">
                <Button type="submit" variant="primary" disabled={saving}>
                    {saving ? savingLabel : submitLabel}
                </Button>

                <Button type="button" variant="outline-secondary" onClick={onCancel} disabled={saving}>
                    Cancel
                </Button>
            </div>
        </Form>
    );
}
