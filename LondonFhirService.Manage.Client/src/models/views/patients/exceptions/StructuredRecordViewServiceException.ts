export class StructuredRecordViewServiceException extends Error {
    public readonly innerException: unknown;

    constructor(message: string, innerException: unknown) {
        super(message);
        this.name = "StructuredRecordViewServiceException";
        this.innerException = innerException;
    }
}
