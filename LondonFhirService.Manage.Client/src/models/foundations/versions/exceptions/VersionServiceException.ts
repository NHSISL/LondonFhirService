export class VersionServiceException extends Error {
    public readonly innerException: unknown;

    constructor(message: string, innerException: unknown) {
        super(message);
        this.name = "VersionServiceException";
        this.innerException = innerException;
    }
}
