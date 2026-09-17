import type { StructuredRecordRequest } from "../../../models/foundations/patients/StructuredRecordRequest";
import type { StructuredRecordResponse } from "../../../models/foundations/patients/StructuredRecordResponse";

export interface IPatientService {
    retrieveStructuredRecordAsync(
        structuredRecordRequest: StructuredRecordRequest,
        abortSignal?: AbortSignal): Promise<StructuredRecordResponse>;
}
