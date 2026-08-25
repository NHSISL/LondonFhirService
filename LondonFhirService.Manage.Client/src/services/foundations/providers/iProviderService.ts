import type { Provider } from "../../../models/foundations/providers/Provider";
import type { ProviderRegistration } from "../../../models/foundations/providers/ProviderRegistration";

export interface IProviderService {
    retrieveAllProvidersAsync(abortSignal?: AbortSignal): Promise<Provider[]>;
    retrieveProviderByIdAsync(providerId: string, abortSignal?: AbortSignal): Promise<Provider>;
    addProviderAsync(providerRegistration: ProviderRegistration): Promise<Provider>;
    modifyProviderAsync(provider: Provider): Promise<Provider>;
    removeProviderByIdAsync(providerId: string): Promise<Provider>;
}
