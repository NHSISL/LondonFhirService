import type { StructuredRecordRequest } from "../../../models/foundations/patients/StructuredRecordRequest";

export interface IPatientService {
    retrieveStructuredRecordAsync(
        structuredRecordRequest: StructuredRecordRequest,
        abortSignal?: AbortSignal): Promise<string>;
}
