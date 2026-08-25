export class AuditDependencyException extends Error {
    public readonly innerException: unknown;

    constructor(message: string, innerException: unknown) {
        super(message);
        this.name = "AuditDependencyException";
        this.innerException = innerException;
    }
}
