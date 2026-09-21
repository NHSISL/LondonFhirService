import { useMemo, useRef } from "react";
import { Col, Row } from "react-bootstrap";
import { EmptyState } from "../shared/EmptyState";
import { PatientCard } from "./patient-card/PatientCard";
import type { CardExpansion } from "../../models/components/comparisons/CardExpansion";
import type { ComparisonSide } from "../../helpers/comparisons/diffHighlighting";
import type { ComparisonSourceView } from "../../models/views/comparisons/ComparisonSourceView";
import type { DiffAcceptance } from "../../models/components/comparisons/DiffAcceptance";
import type { SideBySideViewerProps } from "../../models/components/comparisons/SideBySideViewerProps";

export function SideBySideViewer({
    comparison,
    syncEnabled,
    expandedKeys,
    onToggleExpanded,
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

    // Each card reads its own side's open sections and reports its own clicks. Whether a click
    // then opens the other card as well is the page's decision, not the card's - see
    // handleToggleExpanded.
    const primaryExpansion = useMemo<CardExpansion>(
        () => ({
            isExpanded: expansionKey => expandedKeys.primary.has(expansionKey),
            toggleExpanded: expansionKey => onToggleExpanded("primary", expansionKey)
        }),
        [expandedKeys.primary, onToggleExpanded]);

    const secondaryExpansion = useMemo<CardExpansion>(
        () => ({
            isExpanded: expansionKey => expandedKeys.secondary.has(expansionKey),
            toggleExpanded: expansionKey => onToggleExpanded("secondary", expansionKey)
        }),
        [expandedKeys.secondary, onToggleExpanded]);

    // The two cards render the same clinical facts in the same order, so scrolling one to a
    // difference should bring the other's counterpart alongside it. Reading scrollTop back from
    // the panel being scrolled - rather than tracking it - keeps the two honest when their
    // heights differ.
    const handleScroll = (scrolledPanel: ComparisonSide) => {
        if (syncEnabled === false) {
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
                {/*
                    Bootstrap's grid columns default to min-width: auto, so a column refuses to
                    shrink to its allotted 50% once anything inside it - a long reference in a
                    <code> tag, say - wants to be wider than that. Without min-width: 0 here that
                    one wide element drags the whole row, both columns and everything after them
                    off screen instead of wrapping.
                */}
                <Row className="g-0">
                    <Col md={6} className="border-end" style={{ minWidth: 0 }}>
                        <SourcePanel
                            source={comparison.primarySource}
                            sideLabel="Primary source"
                            diffCountText={comparison.diffCountText}
                            diffCountClassName={comparison.diffCountClassName}
                            panelRef={primaryPanel}
                            onScroll={() => handleScroll("primary")}>
                            {comparison.primarySource !== null && (
                                <PatientCard
                                    source={comparison.primarySource}
                                    diffs={comparison.diffs}
                                    acceptance={primaryAcceptance}
                                    expansion={primaryExpansion} />
                            )}
                        </SourcePanel>
                    </Col>

                    <Col md={6} style={{ minWidth: 0 }}>
                        <SourcePanel
                            source={comparison.secondarySource}
                            sideLabel="Secondary source"
                            diffCountText={comparison.diffCountText}
                            diffCountClassName={comparison.diffCountClassName}
                            panelRef={secondaryPanel}
                            onScroll={() => handleScroll("secondary")}>
                            {comparison.secondarySource !== null && (
                                <PatientCard
                                    source={comparison.secondarySource}
                                    diffs={comparison.diffs}
                                    acceptance={secondaryAcceptance}
                                    expansion={secondaryExpansion} />
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

    // The comparison's own total, not this source's fetch status - a card here is about how much
    // of the record differs, not whether the record came back.
    diffCountText: string;
    diffCountClassName: string;

    panelRef: React.RefObject<HTMLDivElement>;
    onScroll: () => void;
    children: React.ReactNode;
};

function SourcePanel({
    source,
    sideLabel,
    diffCountText,
    diffCountClassName,
    panelRef,
    onScroll,
    children
}: SourcePanelProps) {
    return (
        <>
            <div className="p-3 bg-light border-bottom d-flex align-items-center gap-2">
                <strong>{source?.sourceName ?? sideLabel}</strong>

                {source !== null && (
                    <>
                        <span className={source.roleClassName}>{source.roleText}</span>
                        <span className={diffCountClassName}>{diffCountText}</span>

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
