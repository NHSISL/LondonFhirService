import { ProviderApiBroker } from "../../../brokers/apis/providerApiBroker";
import { tryCatchProviderServiceAsync } from "./providerService.exceptions";
import { validateProviderId } from "./providerService.validations";
import type { IProviderApiBroker } from "../../../brokers/apis/iProviderApiBroker";
import type { IProviderService } from "./iProviderService";
import type { Provider } from "../../../models/foundations/providers/Provider";

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
}
