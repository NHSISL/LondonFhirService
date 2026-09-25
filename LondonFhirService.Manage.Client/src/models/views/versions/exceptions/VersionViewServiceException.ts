export class VersionViewServiceException extends Error {
    public readonly innerException: unknown;

    constructor(message: string, innerException: unknown) {
        super(message);
        this.name = "VersionViewServiceException";
        this.innerException = innerException;
    }
}
