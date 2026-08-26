import { Alert, Button, Col, Container, Row } from "react-bootstrap";
import { useParams } from "react-router-dom";
import { BothJsonModal } from "../../components/comparisons/BothJsonModal";
import { ComparisonResolution } from "../../components/comparisons/ComparisonResolution";
import { DiffSidebar } from "../../components/comparisons/DiffSidebar";
import { ErrorSummary } from "../../components/shared/ErrorSummary";
import { LoadingIndicator } from "../../components/shared/LoadingIndicator";
import { SideBySideViewer } from "../../components/comparisons/SideBySideViewer";
import { useComparisonDetailPage } from "../../hooks/pages/useComparisonDetailPage";

export function ComparisonDetailPage() {
    const { fhirRecordDifferenceId } = useParams<{ fhirRecordDifferenceId: string }>();

    const {
        comparison,
        loading,
        error,
        expandedKeys,
        handleToggleExpanded,
        showDifferences,
        showBothJson,
        syncScrollEnabled,
        handleShowDifferences,
        handleHideDifferences,
        handleShowBothJson,
        handleHideBothJson,
        handleToggleSyncScroll,
        handleBackToComparisons,
        editing,
        saving,
        saveError,
        values,
        handleEdit,
        handleFieldChange,
        handleSave,
        handleCancelEdit,
        acceptanceSaving,
        acceptanceError,
        handleToggleDiffAcceptance
    } = useComparisonDetailPage(fhirRecordDifferenceId ?? "");

    if (loading) {
        return (
            <Container fluid className="mt-4">
                <LoadingIndicator message="Loading comparison..." />
            </Container>
        );
    }

    if (error || comparison === null) {
        return (
            <Container fluid className="mt-4">
                <ErrorSummary
                    title="Comparison could not be loaded"
                    message={error?.message ?? "This comparison could not be found."} />

                <Button
                    variant="outline-secondary"
                    className="mt-3"
                    onClick={handleBackToComparisons}>
                    Back to comparisons
                </Button>
            </Container>
        );
    }

    return (
        <Container fluid className="mt-4">
            <Row className="mb-3 p-2 align-items-start g-2">
                <Col>
                    <h1 className="h3 mb-1">Comparison</h1>

                    <p className="text-muted mb-2">
                        Correlation id <code>{comparison.correlationId}</code>, compared{" "}
                        {comparison.comparedAtText}.
                    </p>

                    <div className="d-flex flex-wrap align-items-center gap-2">
                        <span className={comparison.diffCountClassName}>
                            {comparison.diffCountText}
                        </span>

                        <span className={comparison.acceptableDiffCountClassName}>
                            {comparison.acceptableDiffCountText}
                        </span>

                        <span className={comparison.resolutionClassName}>
                            {comparison.resolutionText}
                        </span>

                        <span className="text-muted small">{comparison.breakdownText}</span>
                    </div>
                </Col>

                <Col xs={12} md="auto" className="d-flex flex-wrap gap-2">
                    <Button variant="outline-secondary" onClick={handleBackToComparisons}>
                        Back to comparisons
                    </Button>

                    <Button
                        variant={syncScrollEnabled ? "outline-primary" : "outline-secondary"}
                        onClick={handleToggleSyncScroll}
                        aria-pressed={syncScrollEnabled}
                        title={"Keeps the two records in step: scrolling one scrolls the other, "
                            + "and opening a section opens the matching one."}>
                        {syncScrollEnabled ? "Sync on" : "Sync off"}
                    </Button>

                    <Button variant="outline-secondary" onClick={handleShowBothJson}>
                        Show both JSON
                    </Button>

                    <Button variant="outline-primary" onClick={handleShowDifferences}>
                        Show {comparison.diffs.length} differences
                    </Button>
                </Col>
            </Row>

            {comparison.sourcesError !== null && (
                <Row className="mb-3 p-2">
                    <Col>
                        <Alert variant="warning" role="alert" className="mb-0">
                            {comparison.sourcesError}
                        </Alert>
                    </Col>
                </Row>
            )}

            {acceptanceError !== null && (
                <Row className="mb-3 p-2">
                    <Col>
                        <Alert variant="danger" role="alert" className="mb-0">
                            {acceptanceError.message}
                        </Alert>
                    </Col>
                </Row>
            )}

            <Row className="mb-3 p-2">
                <Col>
                    <ComparisonResolution
                        comparison={comparison}
                        editing={editing}
                        saving={saving}
                        saveError={saveError}
                        values={values}
                        onEdit={handleEdit}
                        onFieldChange={handleFieldChange}
                        onSave={handleSave}
                        onCancelEdit={handleCancelEdit} />
                </Col>
            </Row>

            <Row className="p-2">
                <Col>
                    <SideBySideViewer
                        comparison={comparison}
                        syncEnabled={syncScrollEnabled}
                        expandedKeys={expandedKeys}
                        onToggleExpanded={handleToggleExpanded}
                        acceptanceSaving={acceptanceSaving}
                        onToggleDiffAcceptance={handleToggleDiffAcceptance} />
                </Col>
            </Row>

            <DiffSidebar
                show={showDifferences}
                onHide={handleHideDifferences}
                diffs={comparison.diffs}
                correlationId={comparison.correlationId}
                acceptanceSaving={acceptanceSaving}
                acceptanceError={acceptanceError}
                onToggleDiffAcceptance={handleToggleDiffAcceptance} />

            <BothJsonModal
                show={showBothJson}
                onHide={handleHideBothJson}
                primarySource={comparison.primarySource}
                secondarySource={comparison.secondarySource}
                syncScrollEnabled={syncScrollEnabled}
                onToggleSyncScroll={handleToggleSyncScroll} />
        </Container>
    );
}
