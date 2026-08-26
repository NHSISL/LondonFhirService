export class FhirRecordDifferenceServiceException extends Error {
    public readonly innerException: unknown;

    constructor(message: string, innerException: unknown) {
        super(message);
        this.name = "FhirRecordDifferenceServiceException";
        this.innerException = innerException;
    }
}
