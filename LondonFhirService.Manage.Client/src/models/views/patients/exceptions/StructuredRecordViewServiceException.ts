export class StructuredRecordViewServiceException extends Error {
    public readonly innerException: unknown;

    /// Only the fields the form actually shows. The view service keeps anything the API rejected
    /// that has no input to sit under - a misconfigured environment url, say - in the message
    /// instead, because an error painted on nothing is an error nobody sees.
    public readonly fieldErrors: Readonly<Record<string, string[]>>;

    constructor(
        message: string,
        innerException: unknown,
        fieldErrors: Record<string, string[]> = {}) {
        super(message);
        this.name = "StructuredRecordViewServiceException";
        this.innerException = innerException;
        this.fieldErrors = fieldErrors;
    }
}
