import type { ProviderFormErrors } from "../../views/providers/ProviderFormErrors";
import type { ProviderFormValues } from "../../views/providers/ProviderFormValues";

export type ProviderFormProps = {
    values: ProviderFormValues;
    errors: ProviderFormErrors;
    saving: boolean;
    submitLabel: string;
    savingLabel: string;
    onFieldChange: (fieldName: keyof ProviderFormValues, value: string | boolean) => void;
    onSubmit: () => void;
    onCancel: () => void;
};
