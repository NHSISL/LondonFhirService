export class FileDownloadDependencyException extends Error {
    public readonly innerException: unknown;

    constructor(message: string, innerException: unknown) {
        super(message);
        this.name = "FileDownloadDependencyException";
        this.innerException = innerException;
    }
}
