import type { StructuredRecordFormValues } from "../../../models/views/patients/StructuredRecordFormValues";
import type { StructuredRecordView } from "../../../models/views/patients/StructuredRecordView";

export interface IStructuredRecordViewService {
    createStructuredRecordFormValues(): StructuredRecordFormValues;

    retrieveStructuredRecordViewAsync(
        structuredRecordFormValues: StructuredRecordFormValues,
        abortSignal?: AbortSignal): Promise<StructuredRecordView>;
}
