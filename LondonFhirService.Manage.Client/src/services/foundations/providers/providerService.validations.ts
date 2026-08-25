import { ProviderValidationException } from "../../../models/foundations/providers/exceptions/ProviderValidationException";
import type { Provider } from "../../../models/foundations/providers/Provider";
import type { ProviderRegistration } from "../../../models/foundations/providers/ProviderRegistration";

// Lengths mirror ValidateProviderOnAdd on the server. Validating here too keeps a malformed
// registration from being sent at all, and gives a message naming the field that failed.
const fullyQualifiedNameMaxLength = 500;
const fhirVersionMaxLength = 10;

export function validateProviderId(providerId: string): void {
    if (providerId === null || providerId === undefined) {
        throw new ProviderValidationException("providerId", "A provider id is required.");
    }

    if (providerId.trim().length === 0) {
        throw new ProviderValidationException("providerId", "A provider id cannot be blank.");
    }
}

export function validateProviderRegistration(providerRegistration: ProviderRegistration): void {
    if (providerRegistration === null || providerRegistration === undefined) {
        throw new ProviderValidationException("provider", "A provider is required.");
    }

    validateProviderId(providerRegistration.id);
    validateRequiredText(providerRegistration.friendlyName, "friendlyName", "A friendly name");

    validateRequiredText(
        providerRegistration.fullyQualifiedName,
        "fullyQualifiedName",
        "A fully qualified name");

    validateRequiredText(providerRegistration.fhirVersion, "fhirVersion", "A FHIR version");

    validateMaximumLength(
        providerRegistration.fullyQualifiedName,
        "fullyQualifiedName",
        "A fully qualified name",
        fullyQualifiedNameMaxLength);

    validateMaximumLength(
        providerRegistration.fhirVersion,
        "fhirVersion",
        "A FHIR version",
        fhirVersionMaxLength);

    validateActivePeriod(providerRegistration);
}

export function validateProviderModification(provider: Provider): void {
    if (provider === null || provider === undefined) {
        throw new ProviderValidationException("provider", "A provider is required.");
    }

    validateProviderRegistration(provider);

    // The server rejects a modify whose created audit values are blank, and compares them against
    // storage, so an edit has to carry back exactly what it was given.
    validateRequiredText(provider.createdBy, "createdBy", "The original created by");
    validateRequiredText(provider.createdDate, "createdDate", "The original created date");
}

function validateRequiredText(value: string, fieldName: string, description: string): void {
    if (!value || value.trim().length === 0) {
        throw new ProviderValidationException(fieldName, `${description} is required.`);
    }
}

function validateMaximumLength(
    value: string,
    fieldName: string,
    description: string,
    maximumLength: number)
    : void {
    if (value.length > maximumLength) {
        throw new ProviderValidationException(
            fieldName,
            `${description} cannot be longer than ${maximumLength} characters.`);
    }
}

function validateActivePeriod(providerRegistration: ProviderRegistration): void {
    const { activeFrom, activeTo } = providerRegistration;

    if (activeFrom === null || activeTo === null) {
        return;
    }

    if (new Date(activeTo).getTime() <= new Date(activeFrom).getTime()) {
        throw new ProviderValidationException(
            "activeTo",
            "Active to must be later than active from.");
    }
}
