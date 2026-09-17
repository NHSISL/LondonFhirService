import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { StructuredRecordViewService } from "../../services/views/patients/structuredRecordViewService";
import { structuredRecordFormValidations } from "../../models/views/patients/StructuredRecordFormValidations";
import { useValidation } from "../useValidation";
import {
    StructuredRecordViewServiceException
} from "../../models/views/patients/exceptions/StructuredRecordViewServiceException";
import type { StructuredRecordFormApiErrors } from "../../models/views/patients/StructuredRecordFormApiErrors";
import type { StructuredRecordFormErrors } from "../../models/views/patients/StructuredRecordFormErrors";
import type { StructuredRecordFormValues } from "../../models/views/patients/StructuredRecordFormValues";
import type { StructuredRecordView } from "../../models/views/patients/StructuredRecordView";

const emptyStructuredRecordFormErrors: StructuredRecordFormErrors = {
    hasErrors: false,
    clientId: "",
    clientSecret: "",
    scope: "",
    grantType: "",
    nhsNumber: "",
    dateOfBirth: ""
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

    // The call in flight, so leaving the page or starting another one can abandon it. A structured
    // record is a whole patient bundle and a slow provider is the normal case here, so without this
    // an operator who navigates away leaves the fetch running to completion and its result landing
    // on an unmounted component - and two quick submissions race, with the loser able to overwrite
    // the winner's record on screen.
    const inFlightRequest = useRef<AbortController | null>(null);

    const abandonInFlightRequest = useCallback(() => {
        inFlightRequest.current?.abort();
        inFlightRequest.current = null;
    }, []);

    useEffect(() => abandonInFlightRequest, [abandonInFlightRequest]);

    const { errors, processApiErrors, enableValidationMessages, disableValidationMessages, validate } =
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

        abandonInFlightRequest();
        const abortController = new AbortController();
        inFlightRequest.current = abortController;

        setSubmitting(true);
        setError(null);

        // Cleared before the call, so a failure does not leave the previous patient's record on
        // screen next to an error about this one.
        setStructuredRecord(null);

        structuredRecordViewService
            .retrieveStructuredRecordViewAsync(values, abortController.signal)
            .then(retrievedStructuredRecord => {
                if (abortController.signal.aborted) {
                    return;
                }

                setStructuredRecord(retrievedStructuredRecord);
            })
            .catch((exception: Error) => {
                // An abandoned call is not a failure the operator needs told about - they are the
                // one who abandoned it - and by now the component may be gone anyway.
                if (abortController.signal.aborted) {
                    return;
                }

                // The API names the field it rejected, and on this form a blank credential is
                // the ordinary case rather than a mistake - a deployed environment configures
                // none, so the operator is expected to supply them here. Painting that under
                // the input is the whole difference between "clientId: Text is invalid" in a
                // banner and a message beside the box to type it in.
                if (exception instanceof StructuredRecordViewServiceException
                    && Object.keys(exception.fieldErrors).length > 0) {
                    processApiErrors(exception.fieldErrors as StructuredRecordFormApiErrors);

                    // Only when something remains that no field can carry. The view service says
                    // so by leaving a message about configuration rather than about the form.
                    setError(
                        exception.message.startsWith("Some of the details below")
                            ? null
                            : exception);

                    return;
                }

                setError(exception);
            })
            .finally(() => {
                if (abortController.signal.aborted) {
                    return;
                }

                inFlightRequest.current = null;
                setSubmitting(false);
            });
    }, [
        enableValidationMessages,
        processApiErrors,
        validate,
        values,
        structuredRecordViewService,
        abandonInFlightRequest
    ]);

    const handleClear = useCallback(() => {
        abandonInFlightRequest();
        setValues(structuredRecordViewService.createStructuredRecordFormValues());
        setStructuredRecord(null);
        setError(null);
        setSubmitting(false);

        // Without this the form comes back blank with a required-field error already on it.
        // enableValidationMessages latches on at the first submit and never turns itself off, so
        // the effect in useValidation re-validates the freshly emptied values and paints an error
        // under an input the operator has not touched.
        disableValidationMessages();
    }, [structuredRecordViewService, abandonInFlightRequest, disableValidationMessages]);

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
