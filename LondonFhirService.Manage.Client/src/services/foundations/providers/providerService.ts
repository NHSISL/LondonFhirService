import { ProviderApiBroker } from "../../../brokers/apis/providerApiBroker";
import { tryCatchProviderServiceAsync } from "./providerService.exceptions";
import {
    validateProviderId,
    validateProviderModification,
    validateProviderRegistration
} from "./providerService.validations";
import type { IProviderApiBroker } from "../../../brokers/apis/iProviderApiBroker";
import type { IProviderService } from "./iProviderService";
import type { Provider } from "../../../models/foundations/providers/Provider";
import type { ProviderRegistration } from "../../../models/foundations/providers/ProviderRegistration";

export class ProviderService implements IProviderService {
    private readonly providerApiBroker: IProviderApiBroker;

    constructor(providerApiBroker: IProviderApiBroker = new ProviderApiBroker()) {
        this.providerApiBroker = providerApiBroker;
    }

    public async retrieveAllProvidersAsync(abortSignal?: AbortSignal): Promise<Provider[]> {
        return await tryCatchProviderServiceAsync(async () =>
            await this.providerApiBroker.getAllProvidersAsync(abortSignal));
    }

    public async retrieveProviderByIdAsync(
        providerId: string,
        abortSignal?: AbortSignal)
        : Promise<Provider> {
        return await tryCatchProviderServiceAsync(async () => {
            validateProviderId(providerId);

            return await this.providerApiBroker.getProviderByIdAsync(providerId, abortSignal);
        });
    }

    public async addProviderAsync(
        providerRegistration: ProviderRegistration)
        : Promise<Provider> {
        return await tryCatchProviderServiceAsync(async () => {
            validateProviderRegistration(providerRegistration);

            return await this.providerApiBroker.postProviderAsync(providerRegistration);
        });
    }

    public async modifyProviderAsync(provider: Provider): Promise<Provider> {
        return await tryCatchProviderServiceAsync(async () => {
            validateProviderModification(provider);

            return await this.providerApiBroker.putProviderAsync(provider);
        });
    }

    public async removeProviderByIdAsync(providerId: string): Promise<Provider> {
        return await tryCatchProviderServiceAsync(async () => {
            validateProviderId(providerId);

            return await this.providerApiBroker.deleteProviderByIdAsync(providerId);
        });
    }
}
