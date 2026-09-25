export class FileDownloadBrokerException extends Error {
    public readonly innerException: unknown;

    constructor(message: string, innerException: unknown) {
        super(message);
        this.name = "FileDownloadBrokerException";
        this.innerException = innerException;
    }
}
