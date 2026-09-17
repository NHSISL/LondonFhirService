import type { ErrorBase } from "../../../types/ErrorBase";

// The NHS number is the only field validated in the browser: the credentials are optional by
// design - a blank one means "use the host's" - and the date of birth is checked by the view
// service rather than by the shared processor, which has no date rule.
//
// The rest are here because the server can still reject them. A credential left blank on a host
// that has none configured is the ordinary case on a deployed environment, and the API names the
// field when that happens, so the form needs somewhere to put that message.
export type StructuredRecordFormErrors = ErrorBase & {
    clientId: string;
    clientSecret: string;
    scope: string;
    grantType: string;
    nhsNumber: string;
    dateOfBirth: string;
};
