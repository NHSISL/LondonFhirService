import { useCallback, useMemo, useState } from "react";
import { Validation } from "../models/validations/validation";
import { ErrorBase } from "../types/ErrorBase";
import { ValidationProcessor } from "./validationProcessor";

export function useValidation<T extends ErrorBase, T2>(errorSpecification: T, validations: Validation[], values: object) {
    const [validationEnabled, setValidationEnabled] = useState(false);

    // Errors the API returned, held against the values they were returned for. Once the values move
    // on they no longer describe what is on screen, so they give way to client-side validation -
    // the same hand-over the effect that used to re-validate on every change performed.
    const [apiErrors, setApiErrors] = useState<{ errors: T; forValues: object } | null>(null);

    // Worked out while rendering rather than copied into state from an effect: the effect set state
    // synchronously on every change, costing a second render each time, which React 19's rules
    // now reject. While validation is off there are no errors to show.
    const validationErrors = useMemo(
        () => validationEnabled
            ? ValidationProcessor<T, T2>().validate(errorSpecification, validations, values, true)
            : errorSpecification,
        [validationEnabled, errorSpecification, validations, values]);

    const errors = validationEnabled && apiErrors !== null && apiErrors.forValues === values
        ? apiErrors.errors
        : validationErrors;

    // Answers straight away for the caller about to submit. Nothing to store: the caller has just
    // turned validation on, so the next render shows the same errors from validationErrors.
    const processValidation = useCallback((valuesToValidate: object) => {
        const submittedErrors =
            ValidationProcessor<T, T2>().validate(errorSpecification, validations, valuesToValidate, true);

        return submittedErrors.hasErrors;
    }, [validations, errorSpecification]);

    const processApiErrors = useCallback((receivedApiErrors: T2) => {
        const processedErrors =
            ValidationProcessor<T, T2>().processApiErrors(receivedApiErrors, errorSpecification);

        setApiErrors(processedErrors.hasErrors ? { errors: processedErrors, forValues: values } : null);
    }, [errorSpecification, values]);

    const enableValidationMessages = useCallback(() => setValidationEnabled(true), []);

    // Drops any API errors too, so turning validation back on starts from what is on screen.
    const disableValidationMessages = useCallback(() => {
        setValidationEnabled(false);
        setApiErrors(null);
    }, []);

    return {
        errors,
        hasErrors: errors.hasErrors,
        processApiErrors,
        enableValidationMessages,
        disableValidationMessages,
        validate: processValidation
    };
}
