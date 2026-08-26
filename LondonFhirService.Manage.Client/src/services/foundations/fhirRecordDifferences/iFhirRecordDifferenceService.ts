import type { FhirRecordDifference } from "../../../models/foundations/fhirRecordDifferences/FhirRecordDifference";
import type { FhirRecordDifferenceQuery } from "../../../models/foundations/fhirRecordDifferences/FhirRecordDifferenceQuery";

export interface IFhirRecordDifferenceService {
    retrieveFhirRecordDifferencesAsync(
        fhirRecordDifferenceQuery: FhirRecordDifferenceQuery,
        abortSignal?: AbortSignal): Promise<FhirRecordDifference[]>;

    retrieveFhirRecordDifferenceByIdAsync(
        fhirRecordDifferenceId: string,
        abortSignal?: AbortSignal): Promise<FhirRecordDifference>;

    modifyFhirRecordDifferenceAsync(
        fhirRecordDifference: FhirRecordDifference): Promise<FhirRecordDifference>;
}
