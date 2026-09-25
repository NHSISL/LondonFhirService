export class VersionApiBrokerException extends Error {
    public readonly innerException: unknown;

    constructor(message: string, innerException: unknown) {
        super(message);
        this.name = "VersionApiBrokerException";
        this.innerException = innerException;
    }
}
