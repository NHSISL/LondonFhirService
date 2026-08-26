// Foundation model - mirrors LondonFhirService.Core.Models.Foundations.FhirRecords.FhirRecord as
// it is serialised by the /api/fhirrecords endpoint. No UI concerns live here.
//
// JsonPayload is the whole FHIR bundle the provider returned, so a record is heavy. Nothing on
// this side should fetch a collection of these; the comparison pages fetch the two records a
// difference names, by id.
export type FhirRecord = {
    id: string;
    correlationId: string;
    jsonPayload: string;
    sourceName: string;
    isPrimarySource: boolean;
    isProcessed: boolean;
    status: FhirRecordStatus;
    insertedDate: string;
    createdBy: string;
    createdDate: string;
    updatedBy: string;
    updatedDate: string;
};

// Ordinals, because the Manage host registers no JsonStringEnumConverter and so serialises
// StatusType as a number. The values match StatusType declaration order.
export const fhirRecordStatuses = {
    pending: 0,
    processing: 1,
    completed: 2,
    failed: 3
} as const;

export type FhirRecordStatus = (typeof fhirRecordStatuses)[keyof typeof fhirRecordStatuses];
