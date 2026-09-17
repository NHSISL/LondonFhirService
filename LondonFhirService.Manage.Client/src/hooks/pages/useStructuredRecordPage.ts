import { useCallback, useMemo, useState } from "react";
import { StructuredRecordViewService } from "../../services/views/patients/structuredRecordViewService";
import { structuredRecordFormValidations } from "../../models/views/patients/StructuredRecordFormValidations";
import { useValidation } from "../useValidation";
import type { StructuredRecordFormApiErrors } from "../../models/views/patients/StructuredRecordFormApiErrors";
import type { StructuredRecordFormErrors } from "../../models/views/patients/StructuredRecordFormErrors";
import type { StructuredRecordFormValues } from "../../models/views/patients/StructuredRecordFormValues";
import type { StructuredRecordView } from "../../models/views/patients/StructuredRecordView";

const emptyStructuredRecordFormErrors: StructuredRecordFormErrors = {
    hasErrors: false,
    nhsNumber: ""
};

export type StructuredRecordPageState = {
    values: StructuredRecordFormValues;
    errors: StructuredRecordFormErrors;
    structuredRecord: StructuredRecordView | null;
    submitting: boolean;
    error: Error | null;
    handleFieldChange: (
        fieldName: keyof StructuredRecordFormValues,
        value: string | boolean) => void;
    handleSubmit: () => void;
    handleClear: () => void;
};

export function useStructuredRecordPage(): StructuredRecordPageState {
    const structuredRecordViewService = useMemo(() => new StructuredRecordViewService(), []);

    const [values, setValues] = useState<StructuredRecordFormValues>(
        () => structuredRecordViewService.createStructuredRecordFormValues());

    const [structuredRecord, setStructuredRecord] = useState<StructuredRecordView | null>(null);
    const [submitting, setSubmitting] = useState<boolean>(false);
    const [error, setError] = useState<Error | null>(null);

    const { errors, enableValidationMessages, validate } =
        useValidation<StructuredRecordFormErrors, StructuredRecordFormApiErrors>(
            emptyStructuredRecordFormErrors,
            structuredRecordFormValidations,
            values);

    const handleFieldChange = useCallback(
        (fieldName: keyof StructuredRecordFormValues, value: string | boolean) =>
            setValues(currentValues => ({ ...currentValues, [fieldName]: value })),
        []);

    // Deliberately not a react-query cache entry. Every submission is a live call an operator
    // asked for, against credentials they may have just changed, so a cached answer would be
    // worse than no answer - and the payload is a whole patient record, which is not something to
    // leave sitting in a client side cache keyed by NHS number.
    const handleSubmit = useCallback(() => {
        enableValidationMessages();

        if (validate(values)) {
            return;
        }

        setSubmitting(true);
        setError(null);

        // Cleared before the call, so a failure does not leave the previous patient's record on
        // screen next to an error about this one.
        setStructuredRecord(null);

        structuredRecordViewService.retrieveStructuredRecordViewAsync(values)
            .then(retrievedStructuredRecord => setStructuredRecord(retrievedStructuredRecord))
            .catch((exception: Error) => setError(exception))
            .finally(() => setSubmitting(false));
    }, [enableValidationMessages, validate, values, structuredRecordViewService]);

    const handleClear = useCallback(() => {
        setValues(structuredRecordViewService.createStructuredRecordFormValues());
        setStructuredRecord(null);
        setError(null);
    }, [structuredRecordViewService]);

    return {
        values: values,
        errors: errors,
        structuredRecord: structuredRecord,
        submitting: submitting,
        error: error,
        handleFieldChange: handleFieldChange,
        handleSubmit: handleSubmit,
        handleClear: handleClear
    };
}
