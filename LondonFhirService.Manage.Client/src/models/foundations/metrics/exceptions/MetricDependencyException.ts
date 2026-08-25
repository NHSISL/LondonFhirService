export class MetricDependencyException extends Error {
    public readonly innerException: unknown;

    constructor(message: string, innerException: unknown) {
        super(message);
        this.name = "MetricDependencyException";
        this.innerException = innerException;
    }
}
