import type { FhirRecord } from "../../models/foundations/fhirRecords/FhirRecord";
import type { FhirRecordQuery } from "../../models/foundations/fhirRecords/FhirRecordQuery";

export interface IFhirRecordApiBroker {
    getFhirRecordByIdAsync(fhirRecordId: string, abortSignal?: AbortSignal): Promise<FhirRecord>;

    getPendingFhirRecordsAsync(
        fhirRecordQuery: FhirRecordQuery,
        abortSignal?: AbortSignal): Promise<FhirRecord[]>;
}
