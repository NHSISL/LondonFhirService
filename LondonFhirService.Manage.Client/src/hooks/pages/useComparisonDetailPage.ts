import { useCallback, useEffect, useMemo, useState } from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useNavigate } from "react-router-dom";
import { ComparisonViewService } from "../../services/views/comparisons/comparisonViewService";
import type { ComparisonDetailView } from "../../models/views/comparisons/ComparisonDetailView";
import type { ComparisonSide } from "../../helpers/comparisons/diffHighlighting";
import type { ComparisonFormValues } from "../../models/views/comparisons/ComparisonFormValues";

// One set of open sections per card. Held apart so that with syncing off an administrator can
// open a section on one side without the other following.
export type SideExpandedKeys = {
    primary: Set<string>;
    secondary: Set<string>;
};

export type ComparisonDetailPageState = {
    comparison: ComparisonDetailView | null;
    loading: boolean;
    error: Error | null;

    // Which sections each card has open, keyed by where the section sits in the card. While
    // syncing is on the two are kept identical, so a difference stays lined up with its
    // counterpart instead of drifting down the page as one side is opened.
    expandedKeys: SideExpandedKeys;
    handleToggleExpanded: (side: ComparisonSide, expansionKey: string) => void;

    showDifferences: boolean;
    showBothJson: boolean;
    syncScrollEnabled: boolean;
    handleShowDifferences: () => void;
    handleHideDifferences: () => void;
    handleShowBothJson: () => void;
    handleHideBothJson: () => void;
    handleToggleSyncScroll: () => void;
    handleBackToComparisons: () => void;

    editing: boolean;
    saving: boolean;
    saveError: Error | null;
    values: ComparisonFormValues;
    handleEdit: () => void;
    handleFieldChange: (
        fieldName: keyof ComparisonFormValues,
        value: string | boolean) => void;
    handleSave: () => void;
    handleCancelEdit: () => void;

    // Ticking a difference rewrites the whole stored result, so only one tick can be in flight at
    // a time - two overlapping saves would each re-read before the other wrote, and the later one
    // would silently drop the earlier tick.
    acceptanceSaving: boolean;
    acceptanceError: Error | null;
    handleToggleDiffAcceptance: (diffIndexes: number[], acceptable: boolean) => void;
};

export function useComparisonDetailPage(
    fhirRecordDifferenceId: string)
    : ComparisonDetailPageState {
    const comparisonViewService = useMemo(() => new ComparisonViewService(), []);
    const queryClient = useQueryClient();
    const navigate = useNavigate();
    const hasFhirRecordDifferenceId = fhirRecordDifferenceId.trim().length > 0;

    const [expandedKeys, setExpandedKeys] = useState<SideExpandedKeys>(
        () => ({ primary: new Set(), secondary: new Set() }));

    const [showDifferences, setShowDifferences] = useState<boolean>(false);
    const [showBothJson, setShowBothJson] = useState<boolean>(false);
    const [syncScrollEnabled, setSyncScrollEnabled] = useState<boolean>(true);

    const [editing, setEditing] = useState<boolean>(false);
    const [saving, setSaving] = useState<boolean>(false);
    const [saveError, setSaveError] = useState<Error | null>(null);
    const [acceptanceSaving, setAcceptanceSaving] = useState<boolean>(false);
    const [acceptanceError, setAcceptanceError] = useState<Error | null>(null);

    const [values, setValues] =
        useState<ComparisonFormValues>(() => comparisonViewService.createComparisonFormValues());

    const { data, isLoading, error } = useQuery<ComparisonDetailView>({
        queryKey: ["ComparisonDetailView", fhirRecordDifferenceId],
        queryFn: async ({ signal }) =>
            await comparisonViewService.retrieveComparisonDetailViewAsync(
                fhirRecordDifferenceId,
                signal),
        enabled: hasFhirRecordDifferenceId
    });

    // Seed the form from whatever the page is currently showing, so a background refetch while the
    // operator is not editing does not leave stale values behind the Edit button.
    useEffect(() => {
        if (data && editing === false) {
            setValues(data.editValues);
        }
    }, [data, editing]);

    const handleShowDifferences = useCallback(() => setShowDifferences(true), []);
    const handleHideDifferences = useCallback(() => setShowDifferences(false), []);
    const handleShowBothJson = useCallback(() => setShowBothJson(true), []);
    const handleHideBothJson = useCallback(() => setShowBothJson(false), []);

    // Turning syncing on brings the two cards back into line rather than waiting for the next
    // click to do it: the point of switching it on is that the panels should match. The union is
    // taken, so nothing an administrator had opened on either side is closed underneath them.
    const handleToggleSyncScroll = useCallback(() => {
        setSyncScrollEnabled(currentValue => {
            const nextValue = currentValue === false;

            if (nextValue) {
                setExpandedKeys(currentKeys => {
                    const union = new Set([...currentKeys.primary, ...currentKeys.secondary]);

                    return { primary: union, secondary: new Set(union) };
                });
            }

            return nextValue;
        });
    }, []);

    const handleToggleExpanded = useCallback(
        (side: ComparisonSide, expansionKey: string) =>
            setExpandedKeys(currentKeys => {
                const shouldExpand = currentKeys[side].has(expansionKey) === false;

                const nextKeys: SideExpandedKeys = {
                    primary: new Set(currentKeys.primary),
                    secondary: new Set(currentKeys.secondary)
                };

                const sidesToChange: ComparisonSide[] =
                    syncScrollEnabled ? ["primary", "secondary"] : [side];

                for (const sideToChange of sidesToChange) {
                    if (shouldExpand) {
                        nextKeys[sideToChange].add(expansionKey);
                    } else {
                        nextKeys[sideToChange].delete(expansionKey);
                    }
                }

                return nextKeys;
            }),
        [syncScrollEnabled]);

    const handleBackToComparisons =
        useCallback(() => navigate("/admin/comparisons"), [navigate]);

    const handleEdit = useCallback(() => {
        setSaveError(null);
        setEditing(true);
    }, []);

    const handleFieldChange = useCallback(
        (fieldName: keyof ComparisonFormValues, value: string | boolean) =>
            setValues(currentValues => ({ ...currentValues, [fieldName]: value })),
        []);

    const handleCancelEdit = useCallback(() => {
        setEditing(false);
        setSaveError(null);

        if (data) {
            setValues(data.editValues);
        }
    }, [data]);

    const handleSave = useCallback(() => {
        setSaving(true);
        setSaveError(null);

        comparisonViewService.updateComparisonAsync(fhirRecordDifferenceId, values)
            .then(async () => {
                await queryClient.invalidateQueries({
                    queryKey: ["ComparisonDetailView", fhirRecordDifferenceId]
                });

                // The list shows the comment and the resolution state, so it is stale the moment
                // either is saved here.
                await queryClient.invalidateQueries({ queryKey: ["ComparisonPageViews"] });

                setEditing(false);
            })
            .catch((exception: Error) => setSaveError(exception))
            .finally(() => setSaving(false));
    }, [values, comparisonViewService, fhirRecordDifferenceId, queryClient]);

    const handleToggleDiffAcceptance = useCallback(
        (diffIndexes: number[], acceptable: boolean) => {
            if (acceptanceSaving || diffIndexes.length === 0) {
                return;
            }

            setAcceptanceSaving(true);
            setAcceptanceError(null);

            comparisonViewService
                .setDiffAcceptanceAsync(fhirRecordDifferenceId, diffIndexes, acceptable)
                .then(async () => {
                    await queryClient.invalidateQueries({
                        queryKey: ["ComparisonDetailView", fhirRecordDifferenceId]
                    });

                    // The list shows the accepted count and the breakdown, so it is stale the
                    // moment a difference is ticked here.
                    await queryClient.invalidateQueries({ queryKey: ["ComparisonPageViews"] });
                })
                .catch((exception: Error) => setAcceptanceError(exception))
                .finally(() => setAcceptanceSaving(false));
        },
        [acceptanceSaving, comparisonViewService, fhirRecordDifferenceId, queryClient]);

    return {
        comparison: data ?? null,
        loading: isLoading,
        error: error,
        expandedKeys: expandedKeys,
        handleToggleExpanded: handleToggleExpanded,
        showDifferences: showDifferences,
        showBothJson: showBothJson,
        syncScrollEnabled: syncScrollEnabled,
        handleShowDifferences: handleShowDifferences,
        handleHideDifferences: handleHideDifferences,
        handleShowBothJson: handleShowBothJson,
        handleHideBothJson: handleHideBothJson,
        handleToggleSyncScroll: handleToggleSyncScroll,
        handleBackToComparisons: handleBackToComparisons,
        editing: editing,
        saving: saving,
        saveError: saveError,
        values: values,
        handleEdit: handleEdit,
        handleFieldChange: handleFieldChange,
        handleSave: handleSave,
        handleCancelEdit: handleCancelEdit,
        acceptanceSaving: acceptanceSaving,
        acceptanceError: acceptanceError,
        handleToggleDiffAcceptance: handleToggleDiffAcceptance
    };
}

