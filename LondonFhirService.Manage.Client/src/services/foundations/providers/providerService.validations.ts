import { ProviderValidationException } from "../../../models/foundations/providers/exceptions/ProviderValidationException";

export function validateProviderId(providerId: string): void {
    if (providerId === null || providerId === undefined) {
        throw new ProviderValidationException("providerId", "A provider id is required.");
    }

    if (providerId.trim().length === 0) {
        throw new ProviderValidationException("providerId", "A provider id cannot be blank.");
    }
}
