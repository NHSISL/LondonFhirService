
export const ValidationMessages = {
    minimumLengthMessage: (minLength: number) => `must have a minimum length of ${minLength} characters`,
    maximumLengthMessage: (maxLength: number) => `must have a maximum length of ${maxLength} characters`,
    minimumValueMessage: (minValue: number) => `must have a minimum value of ${ minValue }`,
    maximumValueMessage: (maxValue: number) => `must have a maximum value of ${maxValue}`,
    minimumDateMessage: (minDate?: Date) => `must have a minimum date of ${minDate}`,
    maximumDateMessage: (maxDate?: Date) => `must have a maximum date of ${maxDate}`,
    regExFail: () => `must have a valid email address`,
    requiredMessage: () => `is required`
}