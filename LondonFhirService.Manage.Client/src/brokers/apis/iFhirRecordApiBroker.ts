import type { FhirRecord } from "../../models/foundations/fhirRecords/FhirRecord";

export interface IFhirRecordApiBroker {
    getFhirRecordByIdAsync(fhirRecordId: string, abortSignal?: AbortSignal): Promise<FhirRecord>;
}
