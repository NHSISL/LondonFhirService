import type { FhirRecord } from "../../../models/foundations/fhirRecords/FhirRecord";

export interface IFhirRecordService {
    retrieveFhirRecordByIdAsync(
        fhirRecordId: string,
        abortSignal?: AbortSignal): Promise<FhirRecord>;
}
