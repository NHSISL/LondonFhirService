import ApiBroker from "../apiBroker";
import { ProviderApiBrokerException } from "../../models/foundations/providers/exceptions/ProviderApiBrokerException";
import type { IProviderApiBroker } from "./iProviderApiBroker";
import type { Provider } from "../../models/foundations/providers/Provider";

export class ProviderApiBroker implements IProviderApiBroker {
    private readonly relativeProvidersUrl = "/api/providers";
    private readonly apiBroker: ApiBroker;

    constructor(apiBroker: ApiBroker = new ApiBroker()) {
        this.apiBroker = apiBroker;
    }

    public async getAllProvidersAsync(abortSignal?: AbortSignal): Promise<Provider[]> {
        try {
            const response =
                await this.apiBroker.GetAsync(this.relativeProvidersUrl, abortSignal);

            const rawProviders: unknown = response.data;

            if (Array.isArray(rawProviders) === false) {
                throw new Error("The providers endpoint did not return a collection.");
            }

            return (rawProviders as unknown[]).map(rawProvider => this.toProvider(rawProvider));
        } catch (exception) {
            throw new ProviderApiBrokerException(
                "Failed to retrieve providers from the API.",
                exception);
        }
    }

    public async getProviderByIdAsync(
        providerId: string,
        abortSignal?: AbortSignal)
        : Promise<Provider> {
        try {
            const response = await this.apiBroker.GetAsync(
                `${this.relativeProvidersUrl}/${encodeURIComponent(providerId)}`,
                abortSignal);

            return this.toProvider(response.data);
        } catch (exception) {
            throw new ProviderApiBrokerException(
                `Failed to retrieve provider '${providerId}' from the API.`,
                exception);
        }
    }

    // Format conversion only - the API is an untyped boundary, so every field is read
    // defensively rather than asserted into shape.
    private toProvider(rawProvider: unknown): Provider {
        if (typeof rawProvider !== "object" || rawProvider === null) {
            throw new Error("The providers endpoint returned an unreadable provider.");
        }

        const source = rawProvider as Record<string, unknown>;

        return {
            id: this.readString(source.id),
            friendlyName: this.readString(source.friendlyName),
            fullyQualifiedName: this.readString(source.fullyQualifiedName),
            fhirVersion: this.readString(source.fhirVersion),
            isActive: source.isActive === true,
            activeFrom: this.readNullableString(source.activeFrom),
            activeTo: this.readNullableString(source.activeTo),
            isForComparisonOnly: source.isForComparisonOnly === true,
            isPrimary: source.isPrimary === true,
            createdBy: this.readString(source.createdBy),
            createdDate: this.readString(source.createdDate),
            updatedBy: this.readString(source.updatedBy),
            updatedDate: this.readString(source.updatedDate)
        };
    }

    private readString(rawValue: unknown): string {
        return typeof rawValue === "string" ? rawValue : "";
    }

    private readNullableString(rawValue: unknown): string | null {
        return typeof rawValue === "string" && rawValue.length > 0 ? rawValue : null;
    }
}
