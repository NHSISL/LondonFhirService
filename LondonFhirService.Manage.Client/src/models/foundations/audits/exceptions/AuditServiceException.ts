export class AuditServiceException extends Error {
    public readonly innerException: unknown;

    constructor(message: string, innerException: unknown) {
        super(message);
        this.name = "AuditServiceException";
        this.innerException = innerException;
    }
}
