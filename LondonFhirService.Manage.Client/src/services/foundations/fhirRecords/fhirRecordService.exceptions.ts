import { FhirRecordApiBrokerException } from "../../../models/foundations/fhirRecords/exceptions/FhirRecordApiBrokerException";
import { FhirRecordDependencyException } from "../../../models/foundations/fhirRecords/exceptions/FhirRecordDependencyException";
import { FhirRecordServiceException } from "../../../models/foundations/fhirRecords/exceptions/FhirRecordServiceException";
import { FhirRecordValidationException } from "../../../models/foundations/fhirRecords/exceptions/FhirRecordValidationException";

export async function tryCatchFhirRecordServiceAsync<T>(
    returningFhirRecordFunction: () => Promise<T>)
    : Promise<T> {
    try {
        return await returningFhirRecordFunction();
    } catch (exception) {
        if (exception instanceof FhirRecordValidationException) {
            throw exception;
        }

        if (exception instanceof FhirRecordApiBrokerException) {
            throw new FhirRecordDependencyException(
                "FHIR record dependency error occurred, please contact support.",
                exception);
        }

        throw new FhirRecordServiceException(
            "FHIR record service error occurred, please contact support.",
            exception);
    }
}
