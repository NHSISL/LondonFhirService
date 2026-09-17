// The field keyed error bag PatientsController returns on a 400, as processApiErrors expects it.
//
// Every key here has an input on the form. The server also validates AuthUrl and
// GetStructuredRecordUrl, which come from configuration and have no field to sit under, so those
// are deliberately absent - the view service routes them to the page's error banner instead.
export type StructuredRecordFormApiErrors = {
    clientId?: string[];
    clientSecret?: string[];
    scope?: string[];
    grantType?: string[];
    nhsNumber?: string[];
    dateOfBirth?: string[];
};
