import type { StructuredRecordRequest } from "../../models/foundations/patients/StructuredRecordRequest";

export interface IPatientApiBroker {
    postStructuredRecordAsync(
        structuredRecordRequest: StructuredRecordRequest,
        abortSignal?: AbortSignal): Promise<string>;
}
