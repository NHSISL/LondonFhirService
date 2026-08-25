export class AuditApiBrokerException extends Error {
    public readonly innerException: unknown;

    constructor(message: string, innerException: unknown) {
        super(message);
        this.name = "AuditApiBrokerException";
        this.innerException = innerException;
    }
}
