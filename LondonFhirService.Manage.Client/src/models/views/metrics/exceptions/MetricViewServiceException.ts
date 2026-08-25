export class MetricViewServiceException extends Error {
    public readonly innerException: unknown;

    constructor(message: string, innerException: unknown) {
        super(message);
        this.name = "MetricViewServiceException";
        this.innerException = innerException;
    }
}
