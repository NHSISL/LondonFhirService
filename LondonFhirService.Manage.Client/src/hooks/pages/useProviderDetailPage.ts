import { useCallback, useEffect, useMemo, useState } from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useNavigate } from "react-router-dom";
import { ProviderViewService } from "../../services/views/providers/providerViewService";
import { providerFormValidations } from "../../models/views/providers/ProviderFormValidations";
import { useValidation } from "../useValidation";
import type { ProviderDetailView } from "../../models/views/providers/ProviderDetailView";
import type { ProviderFormApiErrors } from "../../models/views/providers/ProviderFormApiErrors";
import type { ProviderFormErrors } from "../../models/views/providers/ProviderFormErrors";
import type { ProviderFormValues } from "../../models/views/providers/ProviderFormValues";

const emptyProviderFormErrors: ProviderFormErrors = {
    hasErrors: false,
    friendlyName: "",
    fullyQualifiedName: "",
    fhirVersion: ""
};

export type ProviderDetailPageState = {
    provider: ProviderDetailView | null;
    loading: boolean;
    error: Error | null;
    editing: boolean;
    saving: boolean;
    saveError: Error | null;
    values: ProviderFormValues;
    errors: ProviderFormErrors;
    handleBackToProviders: () => void;
    handleEdit: () => void;
    handleFieldChange: (fieldName: keyof ProviderFormValues, value: string | boolean) => void;
    handleSave: () => void;
    handleCancelEdit: () => void;
};

export function useProviderDetailPage(providerId: string): ProviderDetailPageState {
    const providerViewService = useMemo(() => new ProviderViewService(), []);
    const queryClient = useQueryClient();
    const navigate = useNavigate();
    const hasProviderId = providerId.trim().length > 0;

    const [editing, setEditing] = useState<boolean>(false);
    const [saving, setSaving] = useState<boolean>(false);
    const [saveError, setSaveError] = useState<Error | null>(null);

    const [values, setValues] =
        useState<ProviderFormValues>(() => providerViewService.createProviderFormValues());

    const { data, isLoading, error } = useQuery<ProviderDetailView>({
        queryKey: ["ProviderDetailView", providerId],
        queryFn: async ({ signal }) =>
            await providerViewService.retrieveProviderDetailViewAsync(providerId, signal),
        enabled: hasProviderId
    });

    const { errors, enableValidationMessages, disableValidationMessages, validate } =
        useValidation<ProviderFormErrors, ProviderFormApiErrors>(
            emptyProviderFormErrors,
            providerFormValidations,
            values);

    // Seed the form from whatever the page is currently showing, so a background refetch while the
    // operator is not editing does not leave stale values behind the Edit button.
    useEffect(() => {
        if (data && editing === false) {
            setValues(data.editValues);
        }
    }, [data, editing]);

    const handleBackToProviders = useCallback(() => navigate("/admin/providers"), [navigate]);

    const handleEdit = useCallback(() => {
        setSaveError(null);
        disableValidationMessages();
        setEditing(true);
    }, [disableValidationMessages]);

    const handleFieldChange = useCallback(
        (fieldName: keyof ProviderFormValues, value: string | boolean) =>
            setValues(currentValues => ({ ...currentValues, [fieldName]: value })),
        []);

    const handleCancelEdit = useCallback(() => {
        setEditing(false);
        setSaveError(null);
        disableValidationMessages();

        if (data) {
            setValues(data.editValues);
        }
    }, [data, disableValidationMessages]);

    const handleSave = useCallback(() => {
        enableValidationMessages();

        if (validate(values)) {
            return;
        }

        setSaving(true);
        setSaveError(null);

        providerViewService.updateProviderAsync(providerId, values)
            .then(async () => {
                await queryClient.invalidateQueries({
                    queryKey: ["ProviderDetailView", providerId]
                });

                await queryClient.invalidateQueries({
                    queryKey: ["ProviderListItemViewsGetAll"]
                });

                setEditing(false);
                disableValidationMessages();
            })
            .catch((exception: Error) => setSaveError(exception))
            .finally(() => setSaving(false));
    }, [
        enableValidationMessages,
        disableValidationMessages,
        validate,
        values,
        providerViewService,
        providerId,
        queryClient
    ]);

    return {
        provider: data ?? null,
        loading: isLoading,
        error: error,
        editing: editing,
        saving: saving,
        saveError: saveError,
        values: values,
        errors: errors,
        handleBackToProviders: handleBackToProviders,
        handleEdit: handleEdit,
        handleFieldChange: handleFieldChange,
        handleSave: handleSave,
        handleCancelEdit: handleCancelEdit
    };
}
