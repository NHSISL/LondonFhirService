import type { PatientData } from "../../../models/foundations/fhir/PatientData";

const notSetText = "N/A";

export function formatPatientName(patient: PatientData): string {
    const parts = [
        patient.namePrefix,
        patient.nameGiven,
        patient.nameFamily,
        patient.nameSuffix
    ].filter((part): part is string => part !== null);

    return parts.join(" ");
}

export function formatPatientAddress(patient: PatientData): string {
    const parts = [
        patient.addressLine,
        patient.addressCity,
        patient.addressPostalCode,
        patient.addressCountry
    ].filter((part): part is string => part !== null);

    return parts.join(", ");
}

// The bundles carry FHIR date and dateTime strings. They are rendered in the operator's locale
// with a British fallback, and an unparseable one is shown as it arrived rather than as
// "Invalid Date" - a provider sending a malformed date is itself worth seeing.
export function formatFhirDate(value: string | null): string {
    if (value === null || value.length === 0) {
        return notSetText;
    }

    const parsedDate = new Date(value);

    return Number.isNaN(parsedDate.getTime()) ? value : parsedDate.toLocaleDateString("en-GB");
}
