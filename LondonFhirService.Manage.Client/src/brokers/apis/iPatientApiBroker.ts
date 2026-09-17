import type { StructuredRecordRequest } from "../../models/foundations/patients/StructuredRecordRequest";
import type { StructuredRecordResponse } from "../../models/foundations/patients/StructuredRecordResponse";

export interface IPatientApiBroker {
    postStructuredRecordAsync(
        structuredRecordRequest: StructuredRecordRequest,
        abortSignal?: AbortSignal): Promise<StructuredRecordResponse>;
}
