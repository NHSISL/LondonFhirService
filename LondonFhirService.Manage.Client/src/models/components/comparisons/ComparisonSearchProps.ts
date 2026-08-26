export type ComparisonSearchProps = {
    searchTerm: string;
    unresolvedOnly: boolean;
    loadedCount: number;
    searching: boolean;
    onSearchTermChange: (searchTerm: string) => void;
    onSearchClear: () => void;
    onUnresolvedOnlyChange: (unresolvedOnly: boolean) => void;
};
