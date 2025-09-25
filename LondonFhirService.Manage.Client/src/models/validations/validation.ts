export interface Validation {
    property: string,
    friendlyName: string,
    isRequired?: boolean,
    minLength?: number,
    maxLength?: number,
    minValue?: number,
    maxValue?: number,
    minDate?: Date,
    maxDate?: Date,
    regex?: string | RegExp,
    errorMessage?: string
}
