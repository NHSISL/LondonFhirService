export type AuditSearchProps = {
    searchTerm: string;
    loadedCount: number;
    searching: boolean;
    onSearchTermChange: (searchTerm: string) => void;
    onSearchClear: () => void;
};
