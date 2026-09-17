import type { FhirRecord } from "../../../models/foundations/fhirRecords/FhirRecord";
import type { FhirRecordQuery } from "../../../models/foundations/fhirRecords/FhirRecordQuery";

export interface IFhirRecordService {
    retrieveFhirRecordByIdAsync(
        fhirRecordId: string,
        abortSignal?: AbortSignal): Promise<FhirRecord>;

    retrievePendingFhirRecordsAsync(
        fhirRecordQuery: FhirRecordQuery,
        abortSignal?: AbortSignal): Promise<FhirRecord[]>;
}
