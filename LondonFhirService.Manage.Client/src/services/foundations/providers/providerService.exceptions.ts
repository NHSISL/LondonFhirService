import { ProviderApiBrokerException } from "../../../models/foundations/providers/exceptions/ProviderApiBrokerException";
import { ProviderDependencyException } from "../../../models/foundations/providers/exceptions/ProviderDependencyException";
import { ProviderServiceException } from "../../../models/foundations/providers/exceptions/ProviderServiceException";
import { ProviderValidationException } from "../../../models/foundations/providers/exceptions/ProviderValidationException";

export async function tryCatchProviderServiceAsync<T>(
    returningProviderFunction: () => Promise<T>)
    : Promise<T> {
    try {
        return await returningProviderFunction();
    } catch (exception) {
        if (exception instanceof ProviderValidationException) {
            throw exception;
        }

        if (exception instanceof ProviderApiBrokerException) {
            throw new ProviderDependencyException(
                "Provider dependency error occurred, please contact support.",
                exception);
        }

        throw new ProviderServiceException(
            "Provider service error occurred, please contact support.",
            exception);
    }
}
