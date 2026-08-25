import type { Provider } from "../../models/foundations/providers/Provider";

export interface IProviderApiBroker {
    getAllProvidersAsync(abortSignal?: AbortSignal): Promise<Provider[]>;
    getProviderByIdAsync(providerId: string, abortSignal?: AbortSignal): Promise<Provider>;
}
