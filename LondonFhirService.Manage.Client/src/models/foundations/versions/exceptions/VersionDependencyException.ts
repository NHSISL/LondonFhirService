export class VersionDependencyException extends Error {
    public readonly innerException: unknown;

    constructor(message: string, innerException: unknown) {
        super(message);
        this.name = "VersionDependencyException";
        this.innerException = innerException;
    }
}
