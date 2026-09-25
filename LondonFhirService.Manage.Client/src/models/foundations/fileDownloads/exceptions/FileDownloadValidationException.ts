export class FileDownloadValidationException extends Error {
    public readonly fieldName: string;

    constructor(fieldName: string, reason: string) {
        super(`${fieldName}: ${reason}`);
        this.name = "FileDownloadValidationException";
        this.fieldName = fieldName;
    }
}
