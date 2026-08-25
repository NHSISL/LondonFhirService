export class ProviderServiceException extends Error {
    public readonly innerException: unknown;

    constructor(message: string, innerException: unknown) {
        super(message);
        this.name = "ProviderServiceException";
        this.innerException = innerException;
    }
}
