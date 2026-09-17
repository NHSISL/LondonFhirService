import type { StructuredRecordFormErrors } from "../../views/patients/StructuredRecordFormErrors";
import type { StructuredRecordFormValues } from "../../views/patients/StructuredRecordFormValues";

export type StructuredRecordFormProps = {
    values: StructuredRecordFormValues;
    errors: StructuredRecordFormErrors;
    submitting: boolean;

    onFieldChange: (
        fieldName: keyof StructuredRecordFormValues,
        value: string | boolean) => void;

    onSubmit: () => void;
    onClear: () => void;
};
