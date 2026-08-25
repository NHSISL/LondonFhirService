import { useCallback, useMemo } from "react";
import { useQuery } from "@tanstack/react-query";
import { useNavigate } from "react-router-dom";
import { ProviderViewService } from "../../services/views/providers/providerViewService";
import type { ProviderDetailView } from "../../models/views/providers/ProviderDetailView";

export type ProviderDetailPageState = {
    provider: ProviderDetailView | null;
    loading: boolean;
    error: Error | null;
    handleBackToProviders: () => void;
};

export function useProviderDetailPage(providerId: string): ProviderDetailPageState {
    const providerViewService = useMemo(() => new ProviderViewService(), []);
    const navigate = useNavigate();
    const hasProviderId = providerId.trim().length > 0;

    const { data, isLoading, error } = useQuery<ProviderDetailView>({
        queryKey: ["ProviderDetailView", providerId],
        queryFn: async ({ signal }) =>
            await providerViewService.retrieveProviderDetailViewAsync(providerId, signal),
        enabled: hasProviderId
    });

    const handleBackToProviders = useCallback(() => navigate("/providers"), [navigate]);

    return {
        provider: data ?? null,
        loading: isLoading,
        error: error,
        handleBackToProviders: handleBackToProviders
    };
}
