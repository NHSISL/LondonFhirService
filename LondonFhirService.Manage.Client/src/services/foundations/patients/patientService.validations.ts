import { PatientValidationException } from "../../../models/foundations/patients/exceptions/PatientValidationException";
import type { StructuredRecordRequest } from "../../../models/foundations/patients/StructuredRecordRequest";

// The same format PatientService.Validations enforces on the server. Checked here too so a
// mistyped date is caught before a round trip that could only ever come back as a 400.
const datePattern = /^\d{4}-\d{2}-\d{2}$/;

export function isCompleteDate(value: string): boolean {
    if (datePattern.test(value.trim()) === false) {
        return false;
    }

    const [year, month, day] = value.trim().split("-").map(Number);
    const candidate = new Date(Date.UTC(year, month - 1, day));

    // Date.UTC rolls an impossible day forward - 2002-02-30 becomes 2 March - so the parts are
    // compared back, which is the only way to tell a real date from a rolled one.
    return candidate.getUTCFullYear() === year
        && candidate.getUTCMonth() === month - 1
        && candidate.getUTCDate() === day;
}

export function validateStructuredRecordRequest(
    structuredRecordRequest: StructuredRecordRequest)
    : void {
    if (structuredRecordRequest === null || structuredRecordRequest === undefined) {
        throw new PatientValidationException(
            "structuredRecordRequest",
            "A structured record request is required.");
    }

    const nhsNumber = structuredRecordRequest.nhsNumber?.trim() ?? "";

    if (nhsNumber.length === 0) {
        throw new PatientValidationException("nhsNumber", "An NHS number is required.");
    }

    const dateOfBirth = structuredRecordRequest.dateOfBirth?.trim() ?? "";

    // Optional, because a trace on the NHS number alone is a valid request - but a supplied one
    // has to be a real date in the format the operation expects.
    if (dateOfBirth.length > 0 && isCompleteDate(dateOfBirth) === false) {
        throw new PatientValidationException(
            "dateOfBirth",
            "A date of birth must be a valid date in the format YYYY-MM-DD.");
    }
}
