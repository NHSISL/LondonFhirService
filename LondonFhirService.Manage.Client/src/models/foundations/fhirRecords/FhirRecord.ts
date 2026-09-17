// Foundation model - mirrors LondonFhirService.Core.Models.Foundations.FhirRecords.FhirRecord as
// it is serialised by the /api/fhirrecords endpoint. No UI concerns live here.
//
// JsonPayload is the whole FHIR bundle the provider returned, so a record is heavy. A collection
// of these is only ever fetched through a projection that leaves the payload behind - see
// fhirRecordApiBroker.queries - and a record wanted whole is fetched by id, one at a time. On a
// projected record the fields the query did not ask for read as their empty values.
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
