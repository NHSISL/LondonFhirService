export class PatientServiceException extends Error {
    public readonly innerException: unknown;

    constructor(message: string, innerException: unknown) {
        super(message);
        this.name = "PatientServiceException";
        this.innerException = innerException;
    }
}
