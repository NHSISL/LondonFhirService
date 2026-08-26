import { Alert, Button, Card, Col, Form, Row } from "react-bootstrap";
import { SecuredComponent } from "../securitys/securedComponents";
import securityPoints from "../../securityMatrix";
import type { ComparisonResolutionProps } from "../../models/components/comparisons/ComparisonResolutionProps";

// The triage record for a comparison: how many of its differences have been accepted, whether it
// is done with, and why.
//
// The accepted count is read only in both states. It is not a figure to type: it is the number of
// differences ticked as acceptable in the viewer or the differences list, counted from the stored
// result. Letting it be typed here would let it disagree with the ticks it is meant to summarise.
export function ComparisonResolution({
    comparison,
    editing,
    saving,
    saveError,
    values,
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

                <Row className="g-3">
                    {/* Editing stacks: the form beneath is one column, so a summary field beside
                        it would leave the comment box starting half way down the card. */}
                    <Col xs={12} md={editing ? 12 : 4}>
                        <div className="text-muted small">Accepted</div>

                        <div>
                            <span className={comparison.acceptableDiffCountClassName}>
                                {comparison.acceptableDiffCount}
                            </span>

                            <span className="text-muted small ms-2">
                                of {comparison.diffCount}
                            </span>
                        </div>

                        {editing && (
                            <Form.Text muted>
                                Tick a difference as acceptable in the records below, or in the
                                differences list, to change this.
                            </Form.Text>
                        )}
                    </Col>

                    {editing === false && (
                        <>
                            <Col xs={12} md={4}>
                                <div className="text-muted small">State</div>

                                <div>
                                    <span className={comparison.resolutionClassName}>
                                        {comparison.resolutionText}
                                    </span>
                                </div>
                            </Col>

                            <Col xs={12} md={4}>
                                <div className="text-muted small">Last updated</div>

                                <div>
                                    {comparison.updatedDateText} by {comparison.updatedByText}
                                </div>
                            </Col>

                            <Col xs={12}>
                                <div className="text-muted small">Comment</div>
                                <div className="text-break">{comparison.commentText}</div>
                            </Col>
                        </>
                    )}

                    {editing && (
                        <>
                            <Col xs={12}>
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
                        </>
                    )}
                </Row>
            </Card.Body>
        </Card>
    );
}
