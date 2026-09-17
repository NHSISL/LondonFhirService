import { useEffect, useRef } from "react";
import { Col, Container, Row } from "react-bootstrap";
import { ErrorSummary } from "../../components/shared/ErrorSummary";
import { LoadingIndicator } from "../../components/shared/LoadingIndicator";
import { StructuredRecordForm } from "../../components/patients/StructuredRecordForm";
import { StructuredRecordPayload } from "../../components/patients/StructuredRecordPayload";
import { useStructuredRecordPage } from "../../hooks/pages/useStructuredRecordPage";

// An operator submits from the bottom of a long form, and whatever comes back renders below it -
// off screen. The request looked like it did nothing at all, and a failure was worse than a
// success: the error summary used to render above the form, so it appeared off screen upwards.
//
// Both now live in one region that the page moves to. Scrolled when the request starts, so the
// progress indicator is what fills the space; focused when the outcome lands, so it is announced
// rather than only shown - this screen is the one that tells an operator whether a consumer's
// credentials work, and a result nobody is told about is a result nobody has.
export function StructuredRecordPage() {
    const {
        values,
        errors,
        structuredRecord,
        submitting,
        error,
        handleFieldChange,
        handleSubmit,
        handleClear
    } = useStructuredRecordPage();

    const outcomeRef = useRef<HTMLDivElement>(null);

    // The outcome object itself, not a boolean. Two consecutive failures with the same message are
    // two results and both are worth moving to, and identity is what tells them apart.
    const announcedOutcome = useRef<unknown>(null);
    const outcome = structuredRecord ?? error;

    useEffect(() => {
        if (submitting === false) {
            return;
        }

        // No focus move here. The operator has just pressed a button and still owns the keyboard;
        // taking focus away mid-request would strand it on a region that says "please wait".
        scrollIntoView(outcomeRef.current);
    }, [submitting]);

    useEffect(() => {
        if (outcome === null || outcome === announcedOutcome.current) {
            return;
        }

        announcedOutcome.current = outcome;
        const region = outcomeRef.current;

        if (region === null) {
            return;
        }

        // Focus first and without scrolling, so the browser's own jump to the focused element
        // does not fight the smooth scroll below it.
        region.focus({ preventScroll: true });
        scrollIntoView(region);
    }, [outcome]);

    return (
        <Container fluid className="mt-4">
            <Row className="mb-3 p-2">
                <Col>
                    <h1 className="h3 mb-1">Get Structured Record</h1>
                    <p className="text-muted mb-0">
                        Calls $getstructuredrecord as a consumer would and shows the response
                        exactly as it came back. Nothing is saved - the record is fetched live for
                        this screen and is gone when you leave it.
                    </p>
                </Col>
            </Row>

            {/*
                Capped while the panels are stacked, because a single column of fields stretched
                across a large screen is unreadable. Uncapped from xl, where the panels sit side by
                side and the width is what lets them.
            */}
            <Row className="p-2">
                <Col lg={9} xl={12}>
                    <StructuredRecordForm
                        values={values}
                        errors={errors}
                        submitting={submitting}
                        onFieldChange={handleFieldChange}
                        onSubmit={handleSubmit}
                        onClear={handleClear} />
                </Col>
            </Row>

            {/*
                Always rendered, empty or not, so there is somewhere to scroll to the moment the
                request starts rather than only once its answer exists. tabIndex -1 makes it
                focusable without putting it in the tab order.
            */}
            <div ref={outcomeRef} tabIndex={-1} aria-label="Result">
                {submitting && (
                    <Row className="p-2">
                        <Col>
                            <LoadingIndicator message="Retrieving the structured record..." />
                        </Col>
                    </Row>
                )}

                {error && (
                    <Row className="p-2">
                        <Col>
                            <ErrorSummary
                                title="Structured record could not be retrieved"
                                message={error.message} />
                        </Col>
                    </Row>
                )}

                {structuredRecord && (
                    <Row className="p-2">
                        <Col>
                            <StructuredRecordPayload structuredRecord={structuredRecord} />
                        </Col>
                    </Row>
                )}
            </div>
        </Container>
    );
}

/**
 * Smoothly, unless the reader has asked their system not to animate - a long page travelling under
 * someone who gets motion sick from it is not a courtesy. Guarded for the test environment and for
 * any browser without either api.
 */
function scrollIntoView(element: HTMLElement | null): void {
    if (element === null || typeof element.scrollIntoView !== "function") {
        return;
    }

    const prefersReducedMotion =
        typeof window.matchMedia === "function"
        && window.matchMedia("(prefers-reduced-motion: reduce)").matches;

    element.scrollIntoView({
        behavior: prefersReducedMotion ? "auto" : "smooth",
        block: "start"
    });
}
