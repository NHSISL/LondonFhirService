import type { ErrorBase } from "../../../types/ErrorBase";

// Only the NHS number is validated in the browser. The credentials are optional by design - a
// blank one means "use the host's" - and the date of birth is checked by the view service rather
// than by the shared processor, which has no date rule.
export type StructuredRecordFormErrors = ErrorBase & {
    nhsNumber: string;
};
