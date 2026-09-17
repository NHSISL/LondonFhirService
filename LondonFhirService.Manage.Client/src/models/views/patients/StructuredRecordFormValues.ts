// The shape the page holds while the operator is filling it in. Everything is a string because
// that is what the inputs produce; demographicsOnly is the one exception, because it is a toggle.
export type StructuredRecordFormValues = {
    clientId: string;
    clientSecret: string;
    scope: string;
    grantType: string;
    nhsNumber: string;
    dateOfBirth: string;
    demographicsOnly: boolean;
};
