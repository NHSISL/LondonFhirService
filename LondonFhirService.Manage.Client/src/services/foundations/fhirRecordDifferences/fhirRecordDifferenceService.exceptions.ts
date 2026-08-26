import { FhirRecordDifferenceApiBrokerException } from "../../../models/foundations/fhirRecordDifferences/exceptions/FhirRecordDifferenceApiBrokerException";
import { FhirRecordDifferenceDependencyException } from "../../../models/foundations/fhirRecordDifferences/exceptions/FhirRecordDifferenceDependencyException";
import { FhirRecordDifferenceServiceException } from "../../../models/foundations/fhirRecordDifferences/exceptions/FhirRecordDifferenceServiceException";
import { FhirRecordDifferenceValidationException } from "../../../models/foundations/fhirRecordDifferences/exceptions/FhirRecordDifferenceValidationException";

export async function tryCatchFhirRecordDifferenceServiceAsync<T>(
    returningFhirRecordDifferenceFunction: () => Promise<T>)
    : Promise<T> {
    try {
        return await returningFhirRecordDifferenceFunction();
    } catch (exception) {
        if (exception instanceof FhirRecordDifferenceValidationException) {
            throw exception;
        }

        if (exception instanceof FhirRecordDifferenceApiBrokerException) {
            throw new FhirRecordDifferenceDependencyException(
                "FHIR record difference dependency error occurred, please contact support.",
                exception);
        }

        throw new FhirRecordDifferenceServiceException(
            "FHIR record difference service error occurred, please contact support.",
            exception);
    }
}
