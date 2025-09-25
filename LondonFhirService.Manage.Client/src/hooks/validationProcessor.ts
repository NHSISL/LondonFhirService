import { Validation } from "../models/validations/validation";
import { ErrorBase } from "../types/ErrorBase";
import { ValidationMessages } from "./validationMessages";

export function ValidationProcessor<T extends ErrorBase,T1>() {

    const processRequired = (key: keyof object, values: object, isRequired?: boolean) => {
        if (!isRequired) {
            return;
        }

        if (!values[key]) {
            return ValidationMessages.requiredMessage();
        }
    }

    const processMinLength = (key: keyof object, values: object, minlength?: number) => {
        if (!minlength) {
            return;
        }

        const value : string = values[key];

        if (minlength > value.length) {
            return ValidationMessages.minimumLengthMessage(minlength);
        }
    }

    const processMaxLength = (key: keyof object, values: object, maxlength?: number) => {
        if (!maxlength) {
            return;
        }

        if(typeof values[key] !== 'string'){
            throw "attempting length check on non-string field";
        }

        if (maxlength < (values[key] as string).length) {
            return ValidationMessages.maximumLengthMessage(maxlength);
        }
    }

    const processMinValue = (key: keyof object, values: object, minValue?: number) => {
        if (!minValue) {
            return;
        }

        if(typeof values[key] !== 'number'){
            throw "attempting minimum check on non-number field";
        }

        if (minValue > values[key]) {
            return ValidationMessages.minimumValueMessage(minValue);
        }
    }

    const processMaxValue = (key: keyof object, values: object, maxValue?: number) => {
        if (!maxValue) {
            return;
        }

        if(typeof values[key] !== 'number'){
            throw "attempting minimum check on non-number field";
        }

        if (maxValue < values[key]) {
            return ValidationMessages.maximumValueMessage(maxValue);
        }
    }

    const processMinDate = (key: keyof object, values: object, minDate?: Date) => {
        if (!minDate) {
            return;
        }

        if(typeof values[key] !== 'object'){
            throw "attempting date check on non-date field";
        }

        if (minDate > (values[key] as Date)) {
            return ValidationMessages.minimumDateMessage(minDate);
        }
    }

    const processMaxDate = (key: keyof object, values: object, maxDate?: Date) => {
        if (!maxDate) {
            return;
        }

        if(typeof values[key] !== 'object'){
            throw "attempting date check on non-date field";
        }


        if (maxDate < (values[key] as Date)) {
            return ValidationMessages.maximumDateMessage(maxDate);
        }
    }

    const processEnumValidation = (key: keyof object, values: object, regex?: string | RegExp) => {
        if (!regex) {
            return;
        }

        const reg = new RegExp(regex);

        if (reg.test(values[key]) === false) {
            return ValidationMessages.regExFail();
        }
    }

    const processValidation = (errorSpecification: T, validations: Validation[], values: object, validationEnabled: boolean) : T => {
        const validationErrors: T = { ...errorSpecification };
        
        validations.forEach((validation: Validation) => {
            const propertyErrors: Array<unknown> = [];

            propertyErrors.push(processRequired(validation.property as keyof object, values, validation.isRequired));
            propertyErrors.push(processMinLength(validation.property as keyof object, values, validation.minLength));
            propertyErrors.push(processMaxLength(validation.property as keyof object, values, validation.maxLength));
            propertyErrors.push(processMinValue(validation.property as keyof object, values, validation.minValue));
            propertyErrors.push(processMaxValue(validation.property as keyof object, values, validation.maxValue));
            propertyErrors.push(processMinDate(validation.property as keyof object, values, validation.minDate));
            propertyErrors.push(processMaxDate(validation.property as keyof object, values, validation.maxDate));
            propertyErrors.push(processEnumValidation(validation.property as keyof object, values, validation.regex));

            const processedError = propertyErrors.filter((x: unknown) => x);

            if (processedError.length) {
                // eslint-disable-next-line @typescript-eslint/no-unused-vars
                const property = validationErrors[validation.property as keyof T] ;
                validationErrors[validation.property as keyof T] = `${validation.friendlyName} ${processedError.join(" and ")}.` as typeof property;
                validationErrors.hasErrors = true;
            }
        })

        if(validationEnabled) {
            return validationErrors;
        } 

        return errorSpecification;
    };

    const processApiErrors = (apiErrors: T1, errorSpecification: T ) : T  => {
        const errors = { ...errorSpecification };
        errors.hasErrors = false;
        for (const key in apiErrors) {
            // eslint-disable-next-line @typescript-eslint/no-unused-vars
            const property = errors[key as unknown as keyof T];
            if(apiErrors[key]) {
                if((apiErrors[key] as string[]).length){
                    errors[key as unknown as keyof T] = (apiErrors[key] as string[]).join(", ") as typeof property;
                    errors.hasErrors = true;
                }
            }
        }
        return errors
    }

    return {
        processApiErrors, validate: processValidation
    };
}
