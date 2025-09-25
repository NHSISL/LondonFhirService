import { useCallback, useEffect, useState } from "react";
import { Validation } from "../models/validations/validation";
import { ErrorBase } from "../types/ErrorBase";
import { ValidationProcessor } from "./validationProcessor";

export function useValidation<T extends ErrorBase, T2>(errorSpecification: T, validations: Validation[], values: object) {
    const [errors, setErrors] = useState(errorSpecification);
    const [validationEnabled, setValidationEnabled] = useState(false);
    const [hasErrors, setHasErrors] = useState(false);

    const watchValidation = useCallback((values: object) => {
        const validationErrors = ValidationProcessor<T, T2>().validate(errorSpecification, validations, values, validationEnabled)
        setErrors(validationErrors);
        setHasErrors(validationErrors.hasErrors);
        return validationErrors.hasErrors;
    }, [validations, errorSpecification, validationEnabled]);

    const processValidation = useCallback((values: object) => {
        const validationErrors = ValidationProcessor<T, T2>().validate(errorSpecification, validations, values, true)
        setErrors(validationErrors);
        setHasErrors(validationErrors.hasErrors);
        return validationErrors.hasErrors;
    }, [validations, errorSpecification]);

    useEffect(() => {
        if (!validationEnabled) {
            setErrors(errorSpecification);
            return;
        }
        watchValidation(values);
    }, [values, watchValidation, validationEnabled, errorSpecification])

    const processApiErrors = useCallback((apiErrors: T2) => {
        const processedErrors = ValidationProcessor<T,T2>().processApiErrors(apiErrors, errorSpecification);
        setHasErrors(processedErrors.hasErrors);
        if(processedErrors.hasErrors) {
            setErrors(processedErrors);
        } else {
            setErrors(errorSpecification);
        }
    }, [errorSpecification])

    const enableValidationMessages = () => {
        setValidationEnabled(true)
    }

    const disableValidationMessages = () => {
        setValidationEnabled(false)
    }

    return {
        errors, hasErrors, processApiErrors, enableValidationMessages, disableValidationMessages, validate: processValidation
    };
}