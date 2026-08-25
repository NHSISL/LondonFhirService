export type ProviderSearchProps = {
    searchTerm: string;
    resultCount: number;
    totalCount: number;
    onSearchTermChange: (searchTerm: string) => void;
    onSearchClear: () => void;
};
