export class PatientDependencyException extends Error {
    public readonly innerException: unknown;

    constructor(message: string, innerException: unknown) {
        super(message);
        this.name = "PatientDependencyException";
        this.innerException = innerException;
    }
}
