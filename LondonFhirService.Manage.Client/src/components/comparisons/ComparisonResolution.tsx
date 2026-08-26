import { Alert, Button, Card, Col, Form, Row } from "react-bootstrap";
import { SecuredComponent } from "../securitys/securedComponents";
import securityPoints from "../../securityMatrix";
import type { ComparisonResolutionProps } from "../../models/components/comparisons/ComparisonResolutionProps";

// The triage record for a comparison: how many of its differences have been reviewed and
// accepted, whether it is done with, and why. The differences themselves are written by the
// comparison service and are read only here.
export function ComparisonResolution({
    comparison,
    editing,
    saving,
    saveError,
    values,
    errors,
    onEdit,
    onFieldChange,
    onSave,
    onCancelEdit
}: ComparisonResolutionProps) {
    return (
        <Card className="mb-3">
            <Card.Header className="d-flex justify-content-between align-items-center gap-2">
                <h2 className="h6 mb-0">Review</h2>

                {editing === false && (
                    <SecuredComponent allowedRoles={securityPoints.comparisons.edit}>
                        <Button variant="outline-primary" size="sm" onClick={onEdit}>
                            Edit
                        </Button>
                    </SecuredComponent>
                )}
            </Card.Header>

            <Card.Body>
                {saveError !== null && (
                    <Alert variant="danger" role="alert" className="mb-3">
                        {saveError.message}
                    </Alert>
                )}

                {editing === false
                    ? (
                        <Row className="g-3">
                            <Col xs={6} md={3}>
                                <div className="text-muted small">Accepted</div>
                                <div>{comparison.acceptableDiffCountText}</div>
                            </Col>

                            <Col xs={6} md={3}>
                                <div className="text-muted small">Outstanding</div>
                                <div>{comparison.outstandingDiffCountText}</div>
                            </Col>

                            <Col xs={12} md={3}>
                                <div className="text-muted small">State</div>

                                <div>
                                    <span className={comparison.resolutionClassName}>
                                        {comparison.resolutionText}
                                    </span>
                                </div>
                            </Col>

                            <Col xs={12} md={3}>
                                <div className="text-muted small">Last updated</div>

                                <div>
                                    {comparison.updatedDateText} by {comparison.updatedByText}
                                </div>
                            </Col>

                            <Col xs={12}>
                                <div className="text-muted small">Comment</div>
                                <div className="text-break">{comparison.commentText}</div>
                            </Col>
                        </Row>
                    )
                    : (
                        <Row className="g-3">
                            <Col xs={12} md={4}>
                                <Form.Group controlId="comparisonAcceptableDiffCount">
                                    <Form.Label>Accepted differences</Form.Label>

                                    <Form.Control
                                        type="number"
                                        min={0}
                                        max={comparison.diffCount}
                                        value={values.acceptableDiffCount}
                                        isInvalid={errors.acceptableDiffCount.length > 0}
                                        onChange={event => onFieldChange(
                                            "acceptableDiffCount",
                                            event.currentTarget.value)} />

                                    <Form.Control.Feedback type="invalid">
                                        {errors.acceptableDiffCount}
                                    </Form.Control.Feedback>

                                    <Form.Text muted>
                                        Of the {comparison.diffCount} this comparison found.
                                    </Form.Text>
                                </Form.Group>
                            </Col>

                            <Col xs={12} md={8}>
                                <Form.Group controlId="comparisonComment">
                                    <Form.Label>Comment</Form.Label>

                                    <Form.Control
                                        as="textarea"
                                        rows={3}
                                        value={values.comment}
                                        onChange={event => onFieldChange(
                                            "comment",
                                            event.currentTarget.value)} />
                                </Form.Group>
                            </Col>

                            <Col xs={12}>
                                <Form.Check
                                    type="switch"
                                    id="comparisonIsResolved"
                                    label="Mark this comparison as resolved"
                                    checked={values.isResolved}
                                    onChange={event => onFieldChange(
                                        "isResolved",
                                        event.currentTarget.checked)} />
                            </Col>

                            <Col xs={12} className="d-flex gap-2">
                                <Button variant="primary" onClick={onSave} disabled={saving}>
                                    {saving ? "Saving..." : "Save"}
                                </Button>

                                <Button
                                    variant="outline-secondary"
                                    onClick={onCancelEdit}
                                    disabled={saving}>
                                    Cancel
                                </Button>
                            </Col>
                        </Row>
                    )}
            </Card.Body>
        </Card>
    );
}
