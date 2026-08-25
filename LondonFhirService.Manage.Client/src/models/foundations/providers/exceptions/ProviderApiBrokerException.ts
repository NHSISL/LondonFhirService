export class ProviderApiBrokerException extends Error {
    public readonly innerException: unknown;

    constructor(message: string, innerException: unknown) {
        super(message);
        this.name = "ProviderApiBrokerException";
        this.innerException = innerException;
    }
}
