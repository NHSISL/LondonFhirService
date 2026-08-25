import type { Provider } from "../../models/foundations/providers/Provider";
import type { ProviderRegistration } from "../../models/foundations/providers/ProviderRegistration";

export interface IProviderApiBroker {
    getAllProvidersAsync(abortSignal?: AbortSignal): Promise<Provider[]>;
    getProviderByIdAsync(providerId: string, abortSignal?: AbortSignal): Promise<Provider>;
    postProviderAsync(providerRegistration: ProviderRegistration): Promise<Provider>;
    putProviderAsync(provider: Provider): Promise<Provider>;
    deleteProviderByIdAsync(providerId: string): Promise<Provider>;
}
