export class MetricApiBrokerException extends Error {
    public readonly innerException: unknown;

    constructor(message: string, innerException: unknown) {
        super(message);
        this.name = "MetricApiBrokerException";
        this.innerException = innerException;
    }
}
