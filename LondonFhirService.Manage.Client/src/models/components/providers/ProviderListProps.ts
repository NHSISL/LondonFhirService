import type { ProviderListItemView } from "../../views/providers/ProviderListItemView";

export type ProviderListProps = {
    providers: ProviderListItemView[];
    selectedProviderId?: string;
};
