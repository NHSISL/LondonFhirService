export class AuditViewServiceException extends Error {
    public readonly innerException: unknown;

    constructor(message: string, innerException: unknown) {
        super(message);
        this.name = "AuditViewServiceException";
        this.innerException = innerException;
    }
}
