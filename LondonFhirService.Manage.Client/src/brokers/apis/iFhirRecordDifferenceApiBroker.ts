import type { FhirRecordDifference } from "../../models/foundations/fhirRecordDifferences/FhirRecordDifference";
import type { FhirRecordDifferenceQuery } from "../../models/foundations/fhirRecordDifferences/FhirRecordDifferenceQuery";

export interface IFhirRecordDifferenceApiBroker {
    getFhirRecordDifferencesAsync(
        fhirRecordDifferenceQuery: FhirRecordDifferenceQuery,
        abortSignal?: AbortSignal): Promise<FhirRecordDifference[]>;

    getFhirRecordDifferenceByIdAsync(
        fhirRecordDifferenceId: string,
        abortSignal?: AbortSignal): Promise<FhirRecordDifference>;

    putFhirRecordDifferenceAsync(
        fhirRecordDifference: FhirRecordDifference): Promise<FhirRecordDifference>;
}
