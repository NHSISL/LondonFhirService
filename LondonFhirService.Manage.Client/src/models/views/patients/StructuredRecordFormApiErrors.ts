// The field keyed error bag PatientsController returns on a 400, as processApiErrors expects it.
export type StructuredRecordFormApiErrors = {
    nhsNumber?: string[];
};
