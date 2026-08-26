export class ComparisonViewServiceException extends Error {
    public readonly innerException: unknown;

    constructor(message: string, innerException: unknown) {
        super(message);
        this.name = "ComparisonViewServiceException";
        this.innerException = innerException;
    }
}
