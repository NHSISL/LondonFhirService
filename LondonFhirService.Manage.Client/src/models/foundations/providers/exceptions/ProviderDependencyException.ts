export class ProviderDependencyException extends Error {
    public readonly innerException: unknown;

    constructor(message: string, innerException: unknown) {
        super(message);
        this.name = "ProviderDependencyException";
        this.innerException = innerException;
    }
}
