// What PatientsController accepts on the wire. The four credential fields are optional to the
// server - a blank one falls back to the value the Manage host is configured with - which is why
// none of them are required by the form's own validation either.
export type StructuredRecordRequest = {
    clientId: string;
    clientSecret: string;
    scope: string;
    grantType: string;
    nhsNumber: string;
    dateOfBirth: string;
    demographicsOnly: boolean;
};
