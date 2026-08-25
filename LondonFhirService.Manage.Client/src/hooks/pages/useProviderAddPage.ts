import { useCallback, useMemo, useState } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { useNavigate } from "react-router-dom";
import { ProviderViewService } from "../../services/views/providers/providerViewService";
import { providerFormValidations } from "../../models/views/providers/ProviderFormValidations";
import { useValidation } from "../useValidation";
import type { ProviderFormApiErrors } from "../../models/views/providers/ProviderFormApiErrors";
import type { ProviderFormErrors } from "../../models/views/providers/ProviderFormErrors";
import type { ProviderFormValues } from "../../models/views/providers/ProviderFormValues";

const emptyProviderFormErrors: ProviderFormErrors = {
    hasErrors: false,
    friendlyName: "",
    fullyQualifiedName: "",
    fhirVersion: ""
};

export type ProviderAddPageState = {
    values: ProviderFormValues;
    errors: ProviderFormErrors;
    saving: boolean;
    error: Error | null;
    handleFieldChange: (fieldName: keyof ProviderFormValues, value: string | boolean) => void;
    handleSubmit: () => void;
    handleCancel: () => void;
};

export function useProviderAddPage(): ProviderAddPageState {
    const providerViewService = useMemo(() => new ProviderViewService(), []);
    const queryClient = useQueryClient();
    const navigate = useNavigate();

    const [values, setValues] =
        useState<ProviderFormValues>(() => providerViewService.createProviderFormValues());

    const [saving, setSaving] = useState<boolean>(false);
    const [error, setError] = useState<Error | null>(null);

    const { errors, enableValidationMessages, validate } =
        useValidation<ProviderFormErrors, ProviderFormApiErrors>(
            emptyProviderFormErrors,
            providerFormValidations,
            values);

    const handleFieldChange = useCallback(
        (fieldName: keyof ProviderFormValues, value: string | boolean) =>
            setValues(currentValues => ({ ...currentValues, [fieldName]: value })),
        []);

    const handleSubmit = useCallback(() => {
        enableValidationMessages();

        if (validate(values)) {
            return;
        }

        setSaving(true);
        setError(null);

        providerViewService.addProviderAsync(values)
            .then(async addedProvider => {
                await queryClient.invalidateQueries({
                    queryKey: ["ProviderListItemViewsGetAll"]
                });

                navigate(addedProvider.detailUrl);
            })
            .catch((exception: Error) => setError(exception))
            .finally(() => setSaving(false));
    }, [enableValidationMessages, validate, values, providerViewService, queryClient, navigate]);

    const handleCancel = useCallback(() => navigate("/admin/providers"), [navigate]);

    return {
        values: values,
        errors: errors,
        saving: saving,
        error: error,
        handleFieldChange: handleFieldChange,
        handleSubmit: handleSubmit,
        handleCancel: handleCancel
    };
}
