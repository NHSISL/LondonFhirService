// The field keyed error bag PatientController returns on a 400, as processApiErrors expects it.
export type StructuredRecordFormApiErrors = {
    nhsNumber?: string[];
};
