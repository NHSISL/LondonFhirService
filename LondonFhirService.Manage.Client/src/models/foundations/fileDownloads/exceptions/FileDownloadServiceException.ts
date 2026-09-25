export class FileDownloadServiceException extends Error {
    public readonly innerException: unknown;

    constructor(message: string, innerException: unknown) {
        super(message);
        this.name = "FileDownloadServiceException";
        this.innerException = innerException;
    }
}
