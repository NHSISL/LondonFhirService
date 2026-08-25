export class MetricServiceException extends Error {
    public readonly innerException: unknown;

    constructor(message: string, innerException: unknown) {
        super(message);
        this.name = "MetricServiceException";
        this.innerException = innerException;
    }
}
