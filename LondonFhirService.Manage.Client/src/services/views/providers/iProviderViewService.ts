import type { ProviderDetailView } from "../../../models/views/providers/ProviderDetailView";
import type { ProviderListItemView } from "../../../models/views/providers/ProviderListItemView";

export interface IProviderViewService {
    retrieveProviderListItemViewsAsync(abortSignal?: AbortSignal): Promise<ProviderListItemView[]>;

    retrieveProviderDetailViewAsync(
        providerId: string,
        abortSignal?: AbortSignal): Promise<ProviderDetailView>;

    filterProviderListItemViews(
        providerListItemViews: ProviderListItemView[],
        searchTerm: string): ProviderListItemView[];
}
