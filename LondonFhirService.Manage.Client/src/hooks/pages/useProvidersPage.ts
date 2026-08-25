import { useCallback, useMemo, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { ProviderViewService } from "../../services/views/providers/providerViewService";
import type { ProviderListItemView } from "../../models/views/providers/ProviderListItemView";

export type ProvidersPageState = {
    providers: ProviderListItemView[];
    totalCount: number;
    searchTerm: string;
    loading: boolean;
    error: Error | null;
    handleSearchTermChange: (searchTerm: string) => void;
    handleSearchClear: () => void;
};

export function useProvidersPage(): ProvidersPageState {
    const providerViewService = useMemo(() => new ProviderViewService(), []);
    const [searchTerm, setSearchTerm] = useState<string>("");

    const { data, isLoading, error } = useQuery<ProviderListItemView[]>({
        queryKey: ["ProviderListItemViewsGetAll"],
        queryFn: async ({ signal }) =>
            await providerViewService.retrieveProviderListItemViewsAsync(signal)
    });

    const providers = useMemo(
        () => providerViewService.filterProviderListItemViews(data ?? [], searchTerm),
        [providerViewService, data, searchTerm]);

    const handleSearchTermChange = useCallback(
        (nextSearchTerm: string) => setSearchTerm(nextSearchTerm),
        []);

    const handleSearchClear = useCallback(() => setSearchTerm(""), []);

    return {
        providers: providers,
        totalCount: data?.length ?? 0,
        searchTerm: searchTerm,
        loading: isLoading,
        error: error,
        handleSearchTermChange: handleSearchTermChange,
        handleSearchClear: handleSearchClear
    };
}
