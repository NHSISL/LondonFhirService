import type { Provider } from "../../../models/foundations/providers/Provider";

export interface IProviderService {
    retrieveAllProvidersAsync(abortSignal?: AbortSignal): Promise<Provider[]>;
    retrieveProviderByIdAsync(providerId: string, abortSignal?: AbortSignal): Promise<Provider>;
}
