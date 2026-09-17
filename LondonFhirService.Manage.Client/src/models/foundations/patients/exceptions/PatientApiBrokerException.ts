export class PatientApiBrokerException extends Error {
    public readonly innerException: unknown;

    constructor(message: string, innerException: unknown) {
        super(message);
        this.name = "PatientApiBrokerException";
        this.innerException = innerException;
    }
}
