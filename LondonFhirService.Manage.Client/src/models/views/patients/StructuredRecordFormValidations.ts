import type { Validation } from "../../validations/validation";

// The NHS number is the only field PatientService requires outright, so it is the only one the
// form blocks on. Everything else is either optional or falls back to configuration, and a form
// that demanded a client secret would defeat the fallback the endpoint exists to support.
export const structuredRecordFormValidations: Validation[] = [
    {
        property: "nhsNumber",
        friendlyName: "NHS number",
        isRequired: true,
        maxLength: 50
    }
];
