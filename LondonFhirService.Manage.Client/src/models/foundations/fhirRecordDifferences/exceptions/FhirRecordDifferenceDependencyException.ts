export class FhirRecordDifferenceDependencyException extends Error {
    public readonly innerException: unknown;

    constructor(message: string, innerException: unknown) {
        super(message);
        this.name = "FhirRecordDifferenceDependencyException";
        this.innerException = innerException;
    }
}
