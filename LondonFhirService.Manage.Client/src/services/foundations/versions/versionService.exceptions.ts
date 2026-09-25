import { VersionApiBrokerException } from "../../../models/foundations/versions/exceptions/VersionApiBrokerException";
import { VersionDependencyException } from "../../../models/foundations/versions/exceptions/VersionDependencyException";
import { VersionServiceException } from "../../../models/foundations/versions/exceptions/VersionServiceException";
import { VersionValidationException } from "../../../models/foundations/versions/exceptions/VersionValidationException";

export async function tryCatchVersionServiceAsync<T>(
    returningFunction: () => Promise<T>)
    : Promise<T> {
    try {
        return await returningFunction();
    } catch (exception) {
        if (exception instanceof VersionValidationException) {
            throw exception;
        }

        if (exception instanceof VersionApiBrokerException) {
            throw new VersionDependencyException(
                "Version dependency error occurred, please contact support.",
                exception);
        }

        throw new VersionServiceException(
            "Version service error occurred, please contact support.",
            exception);
    }
}
