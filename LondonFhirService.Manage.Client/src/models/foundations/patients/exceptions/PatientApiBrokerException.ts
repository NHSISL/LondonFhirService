export class PatientApiBrokerException extends Error {
    public readonly innerException: unknown;

    /// The API's field keyed error bag, kept as it arrived rather than only as prose.
    /// PatientsController answers a validation failure with a ValidationProblemDetails whose
    /// errors object names each field that was wrong, and flattening that into the message - which
    /// is all this carried before - left the page unable to put the message under the input it
    /// belongs to. Empty when the failure was not a validation one.
    public readonly fieldErrors: Readonly<Record<string, string[]>>;

    constructor(
        message: string,
        innerException: unknown,
        fieldErrors: Record<string, string[]> = {}) {
        super(message);
        this.name = "PatientApiBrokerException";
        this.innerException = innerException;
        this.fieldErrors = fieldErrors;
    }
}
