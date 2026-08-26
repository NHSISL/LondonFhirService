import { useMemo, useRef } from "react";
import { Col, Row } from "react-bootstrap";
import { EmptyState } from "../shared/EmptyState";
import { PatientCard } from "./patient-card/PatientCard";
import type { ComparisonSourceView } from "../../models/views/comparisons/ComparisonSourceView";
import type { DiffAcceptance } from "../../models/components/comparisons/DiffAcceptance";
import type { SideBySideViewerProps } from "../../models/components/comparisons/SideBySideViewerProps";

export function SideBySideViewer({
    comparison,
    syncScrollEnabled,
    showPatientDetails,
    onShowPatientDetails,
    expandedLists,
    setExpandedLists,
    expandedItems,
    setExpandedItems,
    acceptanceSaving,
    onToggleDiffAcceptance
}: SideBySideViewerProps) {
    const primaryPanel = useRef<HTMLDivElement>(null);
    const secondaryPanel = useRef<HTMLDivElement>(null);

    // Only the secondary card carries the ticks - the primary provider's answer is the one taken
    // as correct - but both cards need the side to read a removal and an addition the right way
    // round.
    const primaryAcceptance = useMemo<DiffAcceptance>(
        () => ({
            side: "primary",
            saving: acceptanceSaving,
            onToggleAcceptance: onToggleDiffAcceptance
        }),
        [acceptanceSaving, onToggleDiffAcceptance]);

    const secondaryAcceptance = useMemo<DiffAcceptance>(
        () => ({
            side: "secondary",
            saving: acceptanceSaving,
            onToggleAcceptance: onToggleDiffAcceptance
        }),
        [acceptanceSaving, onToggleDiffAcceptance]);

    // The two cards render the same clinical facts in the same order, so scrolling one to a
    // difference should bring the other's counterpart alongside it. Reading scrollTop back from
    // the panel being scrolled - rather than tracking it - keeps the two honest when their
    // heights differ.
    const handleScroll = (scrolledPanel: "primary" | "secondary") => {
        if (syncScrollEnabled === false) {
            return;
        }

        const primaryElement = primaryPanel.current;
        const secondaryElement = secondaryPanel.current;

        if (primaryElement === null || secondaryElement === null) {
            return;
        }

        if (scrolledPanel === "primary") {
            secondaryElement.scrollTop = primaryElement.scrollTop;
        } else {
            primaryElement.scrollTop = secondaryElement.scrollTop;
        }
    };

    return (
        <div className="card">
            <div className="card-body p-0">
                <Row className="g-0">
                    <Col md={6} className="border-end">
                        <SourcePanel
                            source={comparison.primarySource}
                            sideLabel="Primary source"
                            panelRef={primaryPanel}
                            onScroll={() => handleScroll("primary")}>
                            {comparison.primarySource !== null && (
                                <PatientCard
                                    source={comparison.primarySource}
                                    diffs={comparison.diffs}
                                    acceptance={primaryAcceptance}
                                    showPatientDetails={showPatientDetails}
                                    onShowPatientDetails={onShowPatientDetails}
                                    expandedLists={expandedLists}
                                    setExpandedLists={setExpandedLists}
                                    expandedItems={expandedItems}
                                    setExpandedItems={setExpandedItems} />
                            )}
                        </SourcePanel>
                    </Col>

                    <Col md={6}>
                        <SourcePanel
                            source={comparison.secondarySource}
                            sideLabel="Secondary source"
                            panelRef={secondaryPanel}
                            onScroll={() => handleScroll("secondary")}>
                            {comparison.secondarySource !== null && (
                                <PatientCard
                                    source={comparison.secondarySource}
                                    diffs={comparison.diffs}
                                    acceptance={secondaryAcceptance}
                                    showPatientDetails={showPatientDetails}
                                    onShowPatientDetails={onShowPatientDetails}
                                    expandedLists={expandedLists}
                                    setExpandedLists={setExpandedLists}
                                    expandedItems={expandedItems}
                                    setExpandedItems={setExpandedItems} />
                            )}
                        </SourcePanel>
                    </Col>
                </Row>
            </div>
        </div>
    );
}

type SourcePanelProps = {
    source: ComparisonSourceView | null;
    sideLabel: string;
    panelRef: React.RefObject<HTMLDivElement>;
    onScroll: () => void;
    children: React.ReactNode;
};

function SourcePanel({ source, sideLabel, panelRef, onScroll, children }: SourcePanelProps) {
    return (
        <>
            <div className="p-3 bg-light border-bottom d-flex align-items-center gap-2">
                <strong>{source?.sourceName ?? sideLabel}</strong>

                {source !== null && (
                    <>
                        <span className={source.statusText ? source.statusClassName : undefined}>
                            {source.statusText}
                        </span>

                        <span className="text-muted small ms-auto">
                            Received {source.createdDateText}
                        </span>
                    </>
                )}
            </div>

            <div
                ref={panelRef}
                onScroll={onScroll}
                className="p-0"
                style={{ maxHeight: "70vh", overflowY: "auto" }}>
                {source === null
                    ? (
                        <div className="p-3">
                            <EmptyState
                                title={`No ${sideLabel.toLowerCase()} to show`}
                                message={"This record could not be loaded. The recorded "
                                    + "differences are still listed."} />
                        </div>
                    )
                    : children}
            </div>
        </>
    );
}
